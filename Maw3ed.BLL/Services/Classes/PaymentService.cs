// Maw3ed.BLL/Services/Classes/PaymentService.cs
using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Payment;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.BLL.Services.Classes
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;
        private readonly IPaymobGateway _paymob;
       

        public PaymentService(IUnitOfWork unitOfWork, AppDbContext context, IPaymobGateway paymob)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _paymob = paymob;
        }

        public async Task<ServiceResult<PaymentInitiateResponseDto>> InitiatePaymentAsync(
            string patientUserId, int appointmentId)
        {
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
            payment.Method = PaymentMethod.CreditCard;

            if (payment.Id == 0)
                await _unitOfWork.GetRepository<Payment>().AddAsync(payment);
            else
                _unitOfWork.GetRepository<Payment>().Update(payment);

            await _unitOfWork.SaveChangesAsync();

            // TODO (الخطوة 3): هنستبدل السطر ده باستدعاء حقيقي لـ IPaymobGateway
            var iframeUrl = await _paymob.CreatePaymentLinkAsync(
      amountCents: (int)(payment.Amount * 100),
      merchantOrderId: $"APPT-{appointment.Id}-{payment.Id}",
      billingEmail: appointment.Patient.User.Email!,
      billingFirstName: appointment.Patient.User.FirstName,
      billingLastName: appointment.Patient.User.LastName,
      billingPhone: appointment.Patient.User.PhoneNumber ?? "01000000000"
  );

            return new(true, "Payment initiated.", new PaymentInitiateResponseDto
            {
                PaymentId = payment.Id,
                IframeUrl = iframeUrl
            });
        }

        public async Task<ServiceResult> HandlePaymobWebhookAsync(PaymobWebhookDto payload, string receivedHmac)
        {
            if (!_paymob.VerifyHmac(payload, receivedHmac))
                return new(false, "Invalid HMAC signature.", ServiceError.BadRequest);

            var parts = payload.MerchantOrderId.Split('-');
            if (parts.Length != 3 || !int.TryParse(parts[2], out var paymentId))
                return new(false, "Invalid order reference.", ServiceError.BadRequest);

            var payment = await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment is null)
                return new(false, "Payment not found.", ServiceError.NotFound);

            if (payment.Status == PaymentStatus.Paid)
                return new(true, "Already processed.");

            if (payload.Success)
            {
                payment.Status = PaymentStatus.Paid;
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

            return new(true, "Webhook processed.");
        }

        private async Task CreditDoctorWalletAsync(Payment payment)
        {
            var doctorId = payment.Appointment.DoctorId;
            var netAmount = payment.Amount - payment.SystemFee;

            var wallet = (await _unitOfWork.GetRepository<DoctorWallet>()
                .GetAllAsync(w => w.DoctorId == doctorId)).FirstOrDefault();

            if (wallet is null) return;

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
    }
}