// Maw3ed.Api/Controllers/PaymentsController.cs
using Maw3ed.BLL.DTOs.Payment;
using Maw3ed.BLL.Services.Classes;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.Extensions;
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
        public async Task<IActionResult> Initiate(int appointmentId, [FromBody] InitiatePaymentDto? dto = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _paymentService.InitiatePaymentAsync(userId, appointmentId, dto?.PaymentMethod);
            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("paymob-webhook")]
        public async Task<IActionResult> PaymobWebhook(
            [FromHeader(Name = "x-hmac")] string? hmac,
            [FromBody] PaymobWebhookWrapperDto wrapper)
        {
            if (string.IsNullOrEmpty(hmac))
                return Ok();

            try
            {
                await _paymentService.HandlePaymobWebhookAsync(wrapper.Obj, hmac);
            }
            catch (Exception)
            {
                // Always return 200 to Paymob to prevent retry storms
            }

            return Ok();
        }
    }
}