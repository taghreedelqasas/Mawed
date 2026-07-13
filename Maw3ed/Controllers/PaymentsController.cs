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
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
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
            [FromQuery] string? hmac,
            [FromBody] PaymobWebhookWrapperDto wrapper)
      {
            _logger.LogInformation("Paymob webhook received. HMAC present: {HasHmac}", !string.IsNullOrEmpty(hmac));

            if (string.IsNullOrEmpty(hmac))
            {
                _logger.LogWarning("Paymob webhook received with no HMAC");
                return Ok();
            }

            try
            {
                var result = await _paymentService.HandlePaymobWebhookAsync(wrapper.Obj, hmac);
                _logger.LogInformation("Webhook processing result: Success={Success}, Message={Message}", result.Success, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Paymob webhook");
            }

            return Ok();
        }
    }
}