using Maw3ed.BLL.DTOs.ConversationDTOs;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maw3ed.APIs.Controllers
{
    /// <summary>
    /// Provides REST API endpoints for doctor-patient conversations and messaging.
    /// Supports text messages, file attachments, read receipts, and conversation management.
    /// </summary>
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

        /// <summary>
        /// Resolves the current user's Patient record ID from JWT claims.
        /// Returns null if the user is not a patient.
        /// </summary>
        private async Task<int?> GetPatientIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var patients = await _unitOfWork.GetRepository<Patient>()
                .FindAsync(p => p.UserId == userId);

            return patients.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Resolves the current user's Doctor record ID from JWT claims.
        /// Returns null if the user is not a doctor.
        /// </summary>
        private async Task<int?> GetDoctorIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var doctors = await _unitOfWork.GetRepository<Doctor>()
                .FindAsync(d => d.UserId == userId);

            return doctors.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Patient starts or resumes a conversation with a doctor.
        /// If a conversation already exists between the patient and doctor, it is returned.
        /// Otherwise, a new conversation is created.
        /// </summary>
        /// <param name="doctorId">The ID of the doctor to converse with.</param>
        /// <returns>The created or existing <see cref="ConversationDto"/>.</returns>
        /// <response code="200">Conversation created or retrieved successfully.</response>
        /// <response code="401">User is not authenticated or has no patient profile.</response>
        /// <response code="400">An error occurred while creating the conversation.</response>
        [HttpPost("start/{doctorId}")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ConversationDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> StartConversation(int doctorId)
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var conversation = await _conversationService
                    .GetOrCreateConversationAsync(patientId.Value, doctorId, userId);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Doctor starts or resumes a conversation with a patient.
        /// If a conversation already exists between the doctor and patient, it is returned.
        /// Otherwise, a new conversation is created.
        /// </summary>
        /// <param name="patientId">The ID of the patient to converse with.</param>
        /// <returns>The created or existing <see cref="ConversationDto"/>.</returns>
        /// <response code="200">Conversation created or retrieved successfully.</response>
        /// <response code="401">User is not authenticated or has no doctor profile.</response>
        /// <response code="400">An error occurred while creating the conversation.</response>
        [HttpPost("doctor-start/{patientId}")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(ConversationDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> DoctorStartConversation(int patientId)
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == null) return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var conversation = await _conversationService
                    .GetOrCreateConversationAsync(patientId, doctorId.Value, userId);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all conversations for the authenticated patient.
        /// Each conversation includes doctor name, image, last message preview, and unread count.
        /// Results are ordered by most recent message first.
        /// </summary>
        /// <returns>A list of <see cref="ConversationDto"/> for the patient.</returns>
        /// <response code="200">Conversations retrieved successfully.</response>
        /// <response code="401">User is not authenticated or has no patient profile.</response>
        [HttpGet("my-conversations")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(IEnumerable<ConversationDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMyConversations()
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null) return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var conversations = await _conversationService
                .GetPatientConversationsAsync(patientId.Value, userId);
            return Ok(conversations);
        }

        /// <summary>
        /// Gets all conversations for the authenticated doctor.
        /// Each conversation includes patient name, last message preview, and unread count.
        /// Results are ordered by most recent message first.
        /// </summary>
        /// <returns>A list of <see cref="ConversationDto"/> for the doctor.</returns>
        /// <response code="200">Conversations retrieved successfully.</response>
        /// <response code="401">User is not authenticated or has no doctor profile.</response>
        [HttpGet("doctor-conversations")]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(IEnumerable<ConversationDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDoctorConversations()
        {
            var doctorId = await GetDoctorIdAsync();
            if (doctorId == null) return Unauthorized();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var conversations = await _conversationService
                .GetDoctorConversationsAsync(doctorId.Value, userId);
            return Ok(conversations);
        }

        /// <summary>
        /// Gets all messages in a conversation. The caller must be the patient or doctor
        /// in the conversation. Messages are ordered chronologically (oldest first).
        /// </summary>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <returns>A list of <see cref="MessageDto"/> in the conversation.</returns>
        /// <response code="200">Messages retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not a participant in this conversation.</response>
        /// <response code="400">Conversation not found or an error occurred.</response>
        [HttpGet("{conversationId}/messages")]
        [ProducesResponseType(typeof(IEnumerable<MessageDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(string), 400)]
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

        /// <summary>
        /// Sends a text message in a conversation. The caller must be the patient or doctor
        /// in the conversation. If using SignalR, the message is also broadcast in real-time
        /// to all participants via the ChatHub.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <param name="dto">The message payload containing the text content.</param>
        /// <returns>The created <see cref="MessageDto"/>.</returns>
        /// <response code="200">Message sent successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not a participant in this conversation.</response>
        /// <response code="400">Conversation not found or an error occurred.</response>
        [HttpPost("{conversationId}/messages")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(string), 400)]
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

        /// <summary>
        /// Uploads a file attachment in a conversation (e.g. medical reports, lab results).
        /// The file is saved to wwwroot/uploads/chat-attachments/ with a unique GUID filename.
        /// Supported file types: .pdf, .jpg, .jpeg, .png.
        /// Maximum file size is limited by the server configuration.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <param name="file">The file to upload (multipart form data).</param>
        /// <param name="caption">Optional caption text for the attachment.</param>
        /// <returns>The created <see cref="MessageDto"/> with attachment metadata.</returns>
        /// <response code="200">Attachment uploaded and message created successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not a participant in this conversation.</response>
        /// <response code="400">No file provided, unsupported file type, or an error occurred.</response>
        [HttpPost("{conversationId}/attachments")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> SendAttachment(
            int conversationId, [FromForm] IFormFile file, [FromForm] string? caption)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("لازم تختار ملف");

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

        /// <summary>
        /// Marks all unread messages from the other party as read for the current user
        /// in the specified conversation.
        /// </summary>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <returns>A success confirmation message.</returns>
        /// <response code="200">Messages marked as read successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not a participant in this conversation.</response>
        /// <response code="400">Conversation not found or an error occurred.</response>
        [HttpPut("{conversationId}/read")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(string), 400)]
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
