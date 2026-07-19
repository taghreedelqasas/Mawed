using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Maw3ed.APIs.Helpers;
using Maw3ed.BLL.AI.DTOs;
using Maw3ed.BLL.AI.Interfaces;
using Maw3ed.DAL;
using System.Security.Claims;

namespace Maw3ed.APIs.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = Roles.Patient)]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IMedicalReportService _medicalReportService;
        private readonly IWebHostEnvironment _environment;
        private readonly IMedicalImageService _medicalImageService;
        private readonly AppDbContext _context;

        public ChatController(
            IChatService chatService,
            IMedicalReportService medicalReportService,
            IMedicalImageService medicalImageService,
            IWebHostEnvironment environment,
            AppDbContext context)
        {
            _chatService = chatService;
            _medicalReportService = medicalReportService;
            _medicalImageService = medicalImageService;
            _environment = environment;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == null)
                return Unauthorized("User is not a valid patient.");

            string activeSessionId = request.SessionId;

            if (string.IsNullOrEmpty(activeSessionId))
            {
                var newSession = new ChatSession
                {
                    PatientId = patientId.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.ChatSessions.AddAsync(newSession);
                await _context.SaveChangesAsync(); // هنا الـ Id بيتولد في القاعدة تلقائياً

                activeSessionId = newSession.Id.ToString(); // هيتحول لـ string كدة كدة
            }

            request.SessionId = activeSessionId;

            var response = await _chatService.SendMessageAsync(request, patientId.Value);

            return Ok(new
            {
                reply = response.Reply,
                sessionId = activeSessionId
            });
        }

        [HttpPost("analyze-pdf")]
        // [EnableRateLimiting("chat")]
        public async Task<IActionResult> AnalyzePdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a PDF file.");

            var patientId = await GetCurrentPatientIdAsync();

            var savedFileName = await FileStorageHelper.SavePdfAsync(
                file,
                _environment.WebRootPath);

            using var stream = file.OpenReadStream();

            var response = await _medicalReportService.AnalyzePdfAsync(
                stream,
                savedFileName,
                patientId ?? 0);

            return Ok(response);
        }

        [HttpPost("analyze-image")]
        // [EnableRateLimiting("chat")]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload an image.");

            var patientId = await GetCurrentPatientIdAsync();

            using var stream = file.OpenReadStream();

            var response = await _medicalImageService.AnalyzeImageAsync(
                stream,
                file.FileName,
                patientId ?? 0);

            return Ok(response);
        }


        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    // لو المريض مش عامل تسجيل دخول أو التوكن مش مبعوت، بنرجع لستة فاضية بدل ما يضرب 500
                    return Ok(new List<object>());
                }

                var sessionsData = await _context.ChatSessions
                    .Where(s => s.PatientId == patientId.Value)
                    .Include(s => s.Messages)
                    .OrderByDescending(s => s.UpdatedAt)
                    .ToListAsync();

                var sessions = sessionsData.Select(s => {
                    // ترتيب الرسائل مع الحماية من وجود Messages بـ null
                    var orderedMessages = (s.Messages ?? new List<ChatMessage>())
                        .OrderBy(m => m.CreatedAt)
                        .ToList();

                    // البحث عن أول رسالة للمستخدم مع الحماية من النصوص الفاضية
                    var firstUserMessage = orderedMessages
                        .FirstOrDefault(m => !m.IsAiResponse)?.Message;

                    string generatedTitle = "محادثة جديدة";

                    if (!string.IsNullOrEmpty(firstUserMessage))
                    {
                        var trimmedMessage = firstUserMessage.Trim();
                        generatedTitle = trimmedMessage.Length > 30
                            ? trimmedMessage.Substring(0, 30) + "..."
                            : trimmedMessage;
                    }

                    return new
                    {
                        id = s.Id.ToString(),
                        title = generatedTitle,
                        createdAt = s.CreatedAt,
                        updatedAt = s.UpdatedAt,
                        messages = orderedMessages.Select(m => new
                        {
                            role = m.IsAiResponse ? "assistant" : "user",
                            content = m.Message ?? "", // حماية لو الكونتنت null
                            timestamp = m.CreatedAt
                        }).ToList()
                    };
                }).ToList();

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                // عشان لو حصل أي خطأ تاني يظهرلك في الـ Console بتاع الباك إند وتعرفي سببه
                Console.WriteLine($"Error in GetHistory: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task<int?> GetCurrentPatientIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return null;

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return patient?.Id;
        }
    }
}