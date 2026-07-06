using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ServiceResult<PaymentInitiateResponseDto>> InitiatePaymentAsync(
           string patientUserId, int appointmentId);
        // في IPaymentService.cs ضيفي:
        Task<ServiceResult> HandlePaymobWebhookAsync(PaymobWebhookDto payload, string receivedHmac);
    }
}
