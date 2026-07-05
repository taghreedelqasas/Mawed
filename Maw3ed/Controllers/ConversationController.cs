using Maw3ed.BLL.DTOs.ConversationDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IUnitOfWork _unitOfWork;
        public ConversationController(
            IConversationService conversationService,
            IUnitOfWork unitOfWork)
        {
            _conversationService = conversationService;
            _unitOfWork = unitOfWork;
        }

        // ============ Helpers ============
        private async Task<int?> GetPatientIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var patients = await _unitOfWork.GetRepository<Patient>()
                .FindAsync(p => p.UserId == userId);

            return patients.FirstOrDefault()?.Id;
        }

        private async Task<int?> GetDoctorIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var doctors = await _unitOfWork.GetRepository<Doctor>()
                .FindAsync(d => d.UserId == userId);

            return doctors.FirstOrDefault()?.Id;
        }

        // المريض يبدأ محادثة مع دكتور
        // POST: api/Conversation/start/{doctorId}
        [HttpPost("start/{doctorId}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> StartConversation(int doctorId)
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            try
            {
                var conversation = await _conversationService
                    .GetOrCreateConversationAsync(patientId.Value, doctorId);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // الدكتور يبدأ محادثة مع مريض ← جديد
        // POST: api/Conversation/doctor-start/{patientId}
        [HttpPost("doctor-start/{patientId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DoctorStartConversation(int patientId)
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == null) return Unauthorized();

            try
            {
                var conversation = await _conversationService
                    .GetOrCreateConversationAsync(patientId, doctorId.Value);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Conversation/my-conversations
        [HttpGet("my-conversations")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyConversations()
        {
            var patientId = await GetPatientIdAsync();
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
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == null) return Unauthorized();

            var conversations = await _conversationService
                .GetDoctorConversationsAsync(doctorId.Value);
            return Ok(conversations);
        }

        // GET: api/Conversation/{conversationId}/messages
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var messages = await _conversationService
                .GetMessagesAsync(conversationId);
            return Ok(messages);
        }

        // POST: api/Conversation/{conversationId}/messages
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
        // PUT: api/Conversation/1/read
        [HttpPut("{conversationId}/read")]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            await _conversationService.MarkMessagesAsReadAsync(conversationId, userId);
            return Ok("تم تحديد الرسايل كمقروءة");
        }
    }
}