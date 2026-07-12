using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Payment;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ServiceResult<PaymentInitiateResponseDto>> InitiatePaymentAsync(
           string patientUserId, int appointmentId, string? paymentMethod = null);
        Task<ServiceResult> HandlePaymobWebhookAsync(PaymobWebhookDto payload, string receivedHmac);
        Task<ServiceResult> RefundAppointmentPaymentAsync(int appointmentId);
    }
}
