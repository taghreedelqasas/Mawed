// Maw3ed.Api/Controllers/PaymentsController.cs
using Maw3ed.BLL.DTOs.Payment;
using Maw3ed.BLL.Services.Classes;
using Maw3ed.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("initiate/{appointmentId}")]
        public async Task<IActionResult> Initiate(int appointmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _paymentService.InitiatePaymentAsync(userId, appointmentId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("paymob-webhook")]
        public async Task<IActionResult> PaymobWebhook(
            [FromQuery] string hmac,
            [FromBody] PaymobWebhookWrapperDto wrapper)
        {
            await _paymentService.HandlePaymobWebhookAsync(wrapper.Obj, hmac);
            return Ok(); // دايمًا 200 لـ Paymob حتى لو فشل التحقق الداخلي، عشان متعملش retry storm
        }
    }
}