using Maw3ed.BLL.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
   public  interface IPaymobGateway
    {
        Task<string> CreatePaymentLinkAsync(
            int amountCents,
            string merchantOrderId,
            string billingEmail,
            string billingFirstName,
            string billingLastName,
            string billingPhone);
        bool VerifyHmac(PaymobWebhookDto payload, string receivedHmac);
    }
}
