// Maw3ed.BLL/DTOs/Payment/PaymobWebhookWrapperDto.cs
using System.Text.Json.Serialization;

namespace Maw3ed.BLL.DTOs.Payment
{
    public class PaymobWebhookWrapperDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("obj")]
        public PaymobWebhookDto Obj { get; set; } = new();
    }
}