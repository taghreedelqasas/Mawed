using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Payment;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Maw3ed.BLL.Services.Classes
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;
        private readonly IPaymobGateway _paymob;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            AppDbContext context,
            IPaymobGateway paymob,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _paymob = paymob;
            _logger = logger;
        }

        public async Task<ServiceResult<PaymentInitiateResponseDto>> InitiatePaymentAsync(
            string patientUserId, int appointmentId, string? paymentMethod = null)
        {
            // Use _context directly for complex includes (repository doesn't support ThenInclude)
            var appointment = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync(a => a.Id == appointmentId
                                       && a.Patient.UserId == patientUserId);

            if (appointment is null)
                return new(false, "Appointment not found.", null, ServiceError.NotFound);

            if (appointment.PaymentStatus == PaymentStatus.Paid)
                return new(false, "This appointment is already paid.", null, ServiceError.Conflict);

            var payment = appointment.Payment ?? new Payment
            {
                AppointmentId = appointment.Id
            };

            payment.Amount = appointment.Doctor.ConsultationFee;
            payment.SystemFee = Math.Round(payment.Amount * 0.10m, 2);
            payment.Status = PaymentStatus.Pending;
            payment.Method = paymentMethod switch
            {
                "Wallet" => PaymentMethod.Wallet,
                _ => PaymentMethod.CreditCard
            };

            if (payment.Id == 0)
                await _unitOfWork.GetRepository<Payment>().AddAsync(payment);
            else
                _unitOfWork.GetRepository<Payment>().Update(payment);

            await _unitOfWork.SaveChangesAsync();

            var (iframeUrl, paymobOrderId) = await _paymob.CreatePaymentLinkAsync(
                amountCents: (int)(payment.Amount * 100),
                merchantOrderId: $"APPT-{appointment.Id}-{payment.Id}",
                billingEmail: appointment.Patient.User.Email!,
                billingFirstName: appointment.Patient.User.FirstName,
                billingLastName: appointment.Patient.User.LastName,
                billingPhone: appointment.Patient.User.PhoneNumber ?? "01000000000"
            );

            payment.PaymobOrderId = paymobOrderId;
            if (payment.Id != 0)
                _unitOfWork.GetRepository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Payment initiated.", new PaymentInitiateResponseDto
            {
                PaymentId = payment.Id,
                IframeUrl = iframeUrl,
                PaymobOrderId = paymobOrderId
            });
        }

        public async Task<ServiceResult> HandlePaymobWebhookAsync(PaymobWebhookDto payload, string receivedHmac)
        {
            if (!_paymob.VerifyHmac(payload, receivedHmac))
            {
                _logger.LogWarning("Webhook HMAC verification failed for order {OrderId}", payload.MerchantOrderId);
                return new(false, "Invalid HMAC signature.", ServiceError.BadRequest);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Payment? payment = null;
                int? fallbackAppointmentId = null;

                // Try by PaymobOrderId (obj.order.id) — most reliable
                if (payload.OrderId > 0)
                {
                    payment = await _context.Payments
                        .Include(p => p.Appointment)
                        .FirstOrDefaultAsync(p => p.PaymobOrderId == payload.OrderId);
                }

                // Try merchant_order_id (APPT-{appointmentId}-{paymentId})
                if (payment == null && !string.IsNullOrEmpty(payload.MerchantOrderId))
                {
                    var parts = payload.MerchantOrderId.Split('-');
                    if (parts.Length == 3 && int.TryParse(parts[1], out var apptId) && int.TryParse(parts[2], out var paymentId))
                    {
                        fallbackAppointmentId = apptId;
                        payment = await _context.Payments
                            .Include(p => p.Appointment)
                            .FirstOrDefaultAsync(p => p.Id == paymentId);
                    }
                }

                // Fallback: find by PaymobTransactionId
                if (payment == null)
                {
                    payment = await _context.Payments
                        .Include(p => p.Appointment)
                        .FirstOrDefaultAsync(p => p.PaymobTransactionId == payload.Id.ToString());
                }

                // Fallback: find by amount_cents + AppointmentId for existing payments without PaymobOrderId
                if (payment == null && fallbackAppointmentId.HasValue)
                {
                    var amount = payload.AmountCents / 100m;
                    payment = await _context.Payments
                        .Include(p => p.Appointment)
                        .Where(p => p.Amount == amount && p.AppointmentId == fallbackAppointmentId.Value
                                    && p.Status == PaymentStatus.Pending && p.PaymobTransactionId == null)
                        .OrderByDescending(p => p.CreatedAt)
                        .FirstOrDefaultAsync();
                }

                if (payment is null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Payment not found for TransactionId={TransactionId}, MerchantOrderId={OrderId}", payload.Id, payload.MerchantOrderId);
                    return new(false, "Payment not found.", ServiceError.NotFound);
                }

                if (payment.Status == PaymentStatus.Paid)
                {
                    await transaction.CommitAsync();
                    return new(true, "Already processed.");
                }

                if (payload.Success)
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.PaymobTransactionId = payload.Id.ToString();
                    payment.Appointment.PaymentStatus = PaymentStatus.Paid;

                    _unitOfWork.GetRepository<Payment>().Update(payment);
                    _unitOfWork.GetRepository<Appointment>().Update(payment.Appointment);
                    await _unitOfWork.SaveChangesAsync();

                    await CreditDoctorWalletAsync(payment);
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                    _unitOfWork.GetRepository<Payment>().Update(payment);
                    await _unitOfWork.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return new(true, "Webhook processed.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing webhook for payment TransactionId={TransactionId}", payload.Id);
                throw;
            }
        }

        private async Task CreditDoctorWalletAsync(Payment payment)
        {
            var doctorId = payment.Appointment.DoctorId;
            var netAmount = payment.Amount - payment.SystemFee;

            var wallet = (await _unitOfWork.GetRepository<DoctorWallet>()
                .GetAllAsync(w => w.DoctorId == doctorId)).FirstOrDefault();

            if (wallet is null)
            {
                wallet = new DoctorWallet
                {
                    DoctorId = doctorId,
                    Balance = 0,
                    PendingBalance = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.GetRepository<DoctorWallet>().AddAsync(wallet);
                _logger.LogInformation("Created wallet for DoctorId={DoctorId}", doctorId);
            }

            wallet.Balance += netAmount;
            wallet.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.GetRepository<DoctorWallet>().Update(wallet);

            var transaction = new WalletTransaction
            {
                DoctorId = doctorId,
                AppointmentId = payment.AppointmentId,
                Amount = netAmount,
                Type = "Credit",
                Status = "Available"
            };
            await _unitOfWork.GetRepository<WalletTransaction>().AddAsync(transaction);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ServiceResult> RefundAppointmentPaymentAsync(int appointmentId)
        {
            // Use _context directly for complex includes
            var payment = await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

            if (payment is null)
                return new(false, "Payment not found.", ServiceError.NotFound);

            if (payment.Status != PaymentStatus.Paid)
                return new(false, "Only paid payments can be refunded.", ServiceError.BadRequest);

            if (string.IsNullOrEmpty(payment.PaymobTransactionId))
                return new(false, "No Paymob transaction reference found.", ServiceError.BadRequest);

            var refunded = await _paymob.RefundAsync(payment.PaymobTransactionId, (int)(payment.Amount * 100));
            if (!refunded)
                return new(false, "Refund failed on Paymob's side.", ServiceError.BadRequest);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAt = DateTime.UtcNow;
                payment.Appointment.PaymentStatus = PaymentStatus.Refunded;

                _unitOfWork.GetRepository<Payment>().Update(payment);
                _unitOfWork.GetRepository<Appointment>().Update(payment.Appointment);
                await _unitOfWork.SaveChangesAsync();

                await ReverseDoctorWalletCreditAsync(payment);

                await transaction.CommitAsync();
                return new(true, "Payment refunded successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing refund for appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        private async Task ReverseDoctorWalletCreditAsync(Payment payment)
        {
            var doctorId = payment.Appointment.DoctorId;
            var netAmount = payment.Amount - payment.SystemFee;

            var wallet = (await _unitOfWork.GetRepository<DoctorWallet>()
                .GetAllAsync(w => w.DoctorId == doctorId)).FirstOrDefault();

            if (wallet is null)
            {
                _logger.LogWarning("Doctor wallet not found for DoctorId={DoctorId} during refund reversal", doctorId);
                return;
            }

            if (wallet.Balance < netAmount)
            {
                _logger.LogWarning(
                    "Insufficient wallet balance for DoctorId={DoctorId}. Balance={Balance}, RefundAmount={RefundAmount}",
                    doctorId, wallet.Balance, netAmount);
                return;
            }

            wallet.Balance -= netAmount;
            wallet.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.GetRepository<DoctorWallet>().Update(wallet);

            var transaction = new WalletTransaction
            {
                DoctorId = doctorId,
                AppointmentId = payment.AppointmentId,
                Amount = netAmount,
                Type = "Debit",
                Status = "Refund"
            };
            await _unitOfWork.GetRepository<WalletTransaction>().AddAsync(transaction);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
