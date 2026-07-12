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
        private readonly IWebHostEnvironment _env;

        public ConversationController(
            IConversationService conversationService,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment env)
        {
            _conversationService = conversationService;
            _unitOfWork = unitOfWork;
            _env = env;
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

        // الدكتور يبدأ محادثة مع مريض
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var messages = await _conversationService
                    .GetMessagesAsync(conversationId, userId);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Conversation/{conversationId}/attachments
        // بيستخدم لإرسال ملف (نتائج تحاليل / تقرير) زي شاشة الشات
        [HttpPost("{conversationId}/attachments")]
        public async Task<IActionResult> SendAttachment(
            int conversationId, [FromForm] IFormFile file, [FromForm] string? caption)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("لازم تختار ملف");

            // تحقق بسيط من نوع الملف
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return BadRequest("نوع الملف مش مدعوم");

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "chat-attachments");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachmentUrl = $"/uploads/chat-attachments/{uniqueFileName}";
                var attachmentType = ext == ".pdf" ? "pdf" : "image";

                var message = await _conversationService.SendAttachmentAsync(
                    conversationId, userId, attachmentUrl, file.FileName, attachmentType, caption);

                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Conversation/{conversationId}/read
        [HttpPut("{conversationId}/read")]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                await _conversationService.MarkMessagesAsReadAsync(conversationId, userId);
                return Ok("تم تحديد الرسايل كمقروءة");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}