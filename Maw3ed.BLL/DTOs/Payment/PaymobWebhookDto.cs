// Maw3ed.BLL/DTOs/Payment/PaymobWebhookDto.cs
using System.Text.Json.Serialization;

namespace Maw3ed.BLL.DTOs.Payment
{
    public class PaymobWebhookDto
    {
        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("created_at")]
        public string Created { get; set; } = "";

        public string Currency { get; set; } = "";

        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        public long Id { get; set; }

        [JsonPropertyName("integration_id")]
        public long IntegrationId { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool Is3dSecure { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("order")]
        public PaymobOrderRef? Order { get; set; }

        [JsonIgnore]
        public long OrderId => Order?.Id ?? 0;

        public long Owner { get; set; }

        public bool Pending { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobSourceData? SourceData { get; set; }

        public bool Success { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; } = "";

        // بنسهّل الوصول للحقول الجوانية عشان نستخدمها في حساب الـ HMAC
        [JsonIgnore]
        public string SourceDataPan => SourceData?.Pan ?? "";
        [JsonIgnore]
        public string SourceDataSubType => SourceData?.SubType ?? "";
        [JsonIgnore]
        public string SourceDataType => SourceData?.Type ?? "";
    }

    public class PaymobSourceData
    {
        public string Pan { get; set; } = "";
        [JsonPropertyName("sub_type")]
        public string SubType { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public class PaymobOrderRef
    {
        public long Id { get; set; }
    }
}