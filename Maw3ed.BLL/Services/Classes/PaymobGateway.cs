using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.BLL.DTOs.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Maw3ed.BLL.Services.Classes
{
    public class PaymobGateway : IPaymobGateway
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymobGateway> _logger;

        private string? _cachedAuthToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public PaymobGateway(HttpClient http, IConfiguration config, ILogger<PaymobGateway> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        private async Task<string> GetAuthTokenAsync()
        {
            if (_cachedAuthToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedAuthToken;

            var apiKey = _config["Paymob:ApiKey"];

            var authRes = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = apiKey });

            if (!authRes.IsSuccessStatusCode)
            {
                var body = await authRes.Content.ReadAsStringAsync();
                _logger.LogError("Paymob auth failed ({Status}): {Body}", authRes.StatusCode, body);
                throw new InvalidOperationException($"Paymob auth failed: {authRes.StatusCode}");
            }

            var auth = await authRes.Content.ReadFromJsonAsync<PaymobAuthResponse>();
            if (auth is null || string.IsNullOrEmpty(auth.Token))
            {
                _logger.LogError("Paymob auth returned empty token");
                throw new InvalidOperationException("Paymob auth returned empty token");
            }

            _cachedAuthToken = auth.Token;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(55);

            return _cachedAuthToken;
        }

        public async Task<string> CreatePaymentLinkAsync(
            int amountCents,
            string merchantOrderId,
            string billingEmail,
            string billingFirstName,
            string billingLastName,
            string billingPhone)
        {
            var authToken = await GetAuthTokenAsync();
            var integrationId = _config["Paymob:IntegrationId"];
            var iframeId = _config["Paymob:IframeId"];

            var orderRes = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/ecommerce/orders",
                new
                {
                    auth_token = authToken,
                    delivery_needed = false,
                    amount_cents = amountCents,
                    currency = "EGP",
                    merchant_order_id = merchantOrderId,
                    items = Array.Empty<object>()
                });

            if (!orderRes.IsSuccessStatusCode)
            {
                var body = await orderRes.Content.ReadAsStringAsync();
                _logger.LogError("Paymob order creation failed ({Status}): {Body}", orderRes.StatusCode, body);
                throw new InvalidOperationException($"Paymob order creation failed: {orderRes.StatusCode}");
            }

            var order = await orderRes.Content.ReadFromJsonAsync<PaymobOrderResponse>();
            if (order is null)
            {
                _logger.LogError("Paymob order returned null");
                throw new InvalidOperationException("Paymob order returned null");
            }

            var keyRes = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys",
                new
                {
                    auth_token = authToken,
                    amount_cents = amountCents,
                    expiration = 3600,
                    order_id = order.Id,
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

            if (!keyRes.IsSuccessStatusCode)
            {
                var body = await keyRes.Content.ReadAsStringAsync();
                _logger.LogError("Paymob payment key creation failed ({Status}): {Body}", keyRes.StatusCode, body);
                throw new InvalidOperationException($"Paymob payment key creation failed: {keyRes.StatusCode}");
            }

            var paymentKeyResp = await keyRes.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>();
            if (paymentKeyResp is null || string.IsNullOrEmpty(paymentKeyResp.Token))
            {
                _logger.LogError("Paymob payment key returned empty token");
                throw new InvalidOperationException("Paymob payment key returned empty token");
            }

            return $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentKeyResp.Token}";
        }

        public bool VerifyHmac(PaymobWebhookDto payload, string receivedHmac)
        {
            var hmacSecret = _config["Paymob:HmacSecret"]!;

            var concatenated =
                $"{payload.AmountCents}{payload.Created}{payload.Currency}{payload.ErrorOccured}".ToLowerInvariant() +
                $"{payload.HasParentTransaction}{payload.Id}{payload.IntegrationId}{payload.IsAuth}".ToLowerInvariant() +
                $"{payload.IsCapture}{payload.IsRefunded}{payload.IsStandalonePayment}{payload.IsVoided}".ToLowerInvariant() +
                $"{payload.MerchantOrderId}{payload.OrderId}{payload.Owner}{payload.Pending}{payload.SourceDataPan}".ToLowerInvariant() +
                $"{payload.SourceDataSubType}{payload.SourceDataType}{payload.Success}".ToLowerInvariant();

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
            var computedBytes = hash;
            var receivedBytes = Convert.FromHexString(receivedHmac);

            return CryptographicOperations.FixedTimeEquals(computedBytes, receivedBytes);
        }

        public async Task<bool> RefundAsync(string transactionId, int amountCents)
        {
            var authToken = await GetAuthTokenAsync();

            var refundRes = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/acceptance/void_refund/refund",
                new
                {
                    auth_token = authToken,
                    transaction_id = transactionId,
                    amount_cents = amountCents
                });

            if (!refundRes.IsSuccessStatusCode)
            {
                var body = await refundRes.Content.ReadAsStringAsync();
                _logger.LogError("Paymob refund failed ({Status}): {Body}", refundRes.StatusCode, body);
                return false;
            }

            var result = await refundRes.Content.ReadFromJsonAsync<PaymobRefundResponse>();
            return result?.Success ?? false;
        }
    }
}
