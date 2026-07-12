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
    [Authorize(Roles = Roles.Patient)]
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
        [EnableRateLimiting("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == null)
                return Unauthorized("لا يوجد ملف مريض مرتبط بهذا الحساب.");

            var response = await _chatService.SendMessageAsync(request, patientId.Value);
            return Ok(response);
        }

        [HttpPost("analyze-pdf")]
        [EnableRateLimiting("chat")]
        public async Task<IActionResult> AnalyzePdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a PDF file.");

            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == null)
                return Unauthorized("لا يوجد ملف مريض مرتبط بهذا الحساب.");

            var savedFileName = await FileStorageHelper.SavePdfAsync(
                file,
                _environment.WebRootPath);

            using var stream = file.OpenReadStream();

            var response = await _medicalReportService.AnalyzePdfAsync(
                stream,
                savedFileName,
                patientId.Value);

            return Ok(response);
        }

        [HttpPost("analyze-image")]
        [EnableRateLimiting("chat")]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload an image.");

            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == null)
                return Unauthorized("لا يوجد ملف مريض مرتبط بهذا الحساب.");

            using var stream = file.OpenReadStream();

            var response = await _medicalImageService.AnalyzeImageAsync(
                stream,
                file.FileName,
                patientId.Value);

            return Ok(response);
        }
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == null)
                return Unauthorized("لا يوجد ملف مريض مرتبط بهذا الحساب.");

            var messages = await _context.ChatMessages
                .Where(m => m.ChatSession.PatientId == patientId.Value)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatHistoryItem
                {
                    Sender = m.Sender,
                    Message = m.Message,
                    IsAiResponse = m.IsAiResponse,
                    SentAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
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