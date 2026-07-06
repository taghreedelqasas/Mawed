// Maw3ed.BLL/Services/Classes/PaymobGateway.cs
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.BLL.DTOs.Payment;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Maw3ed.BLL.Services.Classes
{
    public class PaymobGateway : IPaymobGateway
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public PaymobGateway(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> CreatePaymentLinkAsync(
            int amountCents,
            string merchantOrderId,
            string billingEmail,
            string billingFirstName,
            string billingLastName,
            string billingPhone)
        {
            var apiKey = _config["Paymob:ApiKey"];
            var integrationId = _config["Paymob:IntegrationId"];
            var iframeId = _config["Paymob:IframeId"];

            var authRes = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = apiKey });
            var authToken = (await authRes.Content.ReadFromJsonAsync<PaymobAuthResponse>())!.Token;

            var orderRes = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/ecommerce/orders",
                new
                {
                    auth_token = authToken,
                    delivery_needed = "false",
                    amount_cents = amountCents,
                    currency = "EGP",
                    merchant_order_id = merchantOrderId,
                    items = Array.Empty<object>()
                });
            var order = await orderRes.Content.ReadFromJsonAsync<PaymobOrderResponse>();

            var keyRes = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys",
                new
                {
                    auth_token = authToken,
                    amount_cents = amountCents,
                    expiration = 3600,
                    order_id = order!.Id,
                    currency = "EGP",
                    integration_id = integrationId,
                    billing_data = new
                    {
                        email = billingEmail,
                        first_name = billingFirstName,
                        last_name = billingLastName,
                        phone_number = billingPhone,
                        apartment = "NA",
                        floor = "NA",
                        street = "NA",
                        building = "NA",
                        city = "NA",
                        country = "NA",
                        state = "NA",
                        postal_code = "NA"
                    }
                });
            var paymentKey = (await keyRes.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>())!.Token;

            return $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentKey}";
        }

        public bool VerifyHmac(PaymobWebhookDto payload, string receivedHmac)
        {
            var hmacSecret = _config["Paymob:HmacSecret"]!;

            var concatenated =
                $"{payload.AmountCents}{payload.Created}{payload.Currency}{payload.ErrorOccured}".ToLowerInvariant() +
                $"{payload.HasParentTransaction}{payload.Id}{payload.IntegrationId}{payload.IsAuth}".ToLowerInvariant() +
                $"{payload.IsCapture}{payload.IsRefunded}{payload.IsStandalonePayment}{payload.IsVoided}".ToLowerInvariant() +
                $"{payload.OrderId}{payload.Owner}{payload.Pending}{payload.SourceDataPan}".ToLowerInvariant() +
                $"{payload.SourceDataSubType}{payload.SourceDataType}{payload.Success}".ToLowerInvariant();

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
            var computedHmac = Convert.ToHexString(hash).ToLowerInvariant();

            return computedHmac == receivedHmac.ToLowerInvariant();
        }
    }
}
