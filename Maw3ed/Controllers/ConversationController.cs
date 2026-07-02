using Maw3ed.BLL.DTOs.ConversationDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;

        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        // POST: api/Conversation/start/3
        [HttpPost("start/{doctorId}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> StartConversation(int doctorId)
        {
            var patientId = GetPatientId();
            if (patientId == null) return Unauthorized();

            var conversation = await _conversationService
                .GetOrCreateConversationAsync(patientId.Value, doctorId);
            return Ok(conversation);
        }

        // GET: api/Conversation/my-conversations
        [HttpGet("my-conversations")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyConversations()
        {
            var patientId = GetPatientId();
            if (patientId == null) return Unauthorized();

            var conversations = await _conversationService
                .GetPatientConversationsAsync(patientId.Value);
            return Ok(conversations);
        }

        // GET: api/Conversation/doctor-conversations
        [HttpGet("doctor-conversations")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorConversations()
        {
            var doctorId = GetDoctorId();
            if (doctorId == null) return Unauthorized();

            var conversations = await _conversationService
                .GetDoctorConversationsAsync(doctorId.Value);
            return Ok(conversations);
        }

        // GET: api/Conversation/5/messages
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var messages = await _conversationService
                .GetMessagesAsync(conversationId);
            return Ok(messages);
        }

        // POST: api/Conversation/5/messages
        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(
            int conversationId, [FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var message = await _conversationService
                    .SendMessageAsync(conversationId, userId, dto);
                return Ok(message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ============ Helpers ============
        private int? GetPatientId()
        {
            var value = User.FindFirst("PatientId")?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }

        private int? GetDoctorId()
        {
            var value = User.FindFirst("DoctorId")?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
