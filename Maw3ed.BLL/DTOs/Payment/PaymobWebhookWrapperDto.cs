// Maw3ed.BLL/DTOs/Payment/PaymobWebhookWrapperDto.cs
namespace Maw3ed.BLL.DTOs.Payment
{
    public class PaymobWebhookWrapperDto
    {
        public string Type { get; set; } = "";
        public PaymobWebhookDto Obj { get; set; } = new();
    }
}