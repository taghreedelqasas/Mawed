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
        // [EnableRateLimiting("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var patientId = await GetCurrentPatientIdAsync();

            var response = await _chatService.SendMessageAsync(request, patientId ?? 0);
            return Ok(response);
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
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var patientId = await GetCurrentPatientIdAsync();

            // 1. جلب البيانات والرسائل من الداتابيز أولاً
            var sessionsData = await _context.ChatSessions
                .Where(s => s.PatientId == (patientId ?? 0))
                .Include(s => s.Messages)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            // 2. عمل الـ Mapping وتوليد الـ Title تلقائياً في الـ Memory
            var sessions = sessionsData.Select(s => {
                // ترتيب الرسائل تصاعدياً لمعرفة أول رسالة
                var orderedMessages = s.Messages.OrderBy(m => m.CreatedAt).ToList();

                // البحث عن أول رسالة أرسلها المستخدم لتكون هي العنوان
                var firstUserMessage = orderedMessages.FirstOrDefault(m => !m.IsAiResponse)?.Message;

                // لو الرسالة طويلة، بنقصها ونأخذ أول 30 حرف فقط
                var generatedTitle = !string.IsNullOrEmpty(firstUserMessage)
                    ? (firstUserMessage.Length > 30 ? firstUserMessage.Substring(0, 30) + "..." : firstUserMessage)
                    : "محادثة جديدة";

                return new
                {
                    id = s.Id.ToString(),
                    title = generatedTitle, // تم حل المشكلة هنا
                    createdAt = s.CreatedAt,
                    updatedAt = s.UpdatedAt,
                    messages = orderedMessages.Select(m => new
                    {
                        role = m.IsAiResponse ? "assistant" : "user",
                        content = m.Message,
                        timestamp = m.CreatedAt
                    }).ToList()
                };
            }).ToList();

            return Ok(sessions);
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