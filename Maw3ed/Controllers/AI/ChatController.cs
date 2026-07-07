using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Maw3ed.APIs.Helpers;
using Maw3ed.BLL.AI.DTOs;
using Maw3ed.BLL.AI.Interfaces;

namespace Maw3ed.APIs.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IMedicalReportService _medicalReportService;
        private readonly IWebHostEnvironment _environment;
        private readonly IMedicalImageService _medicalImageService;

        public ChatController(
    IChatService chatService,
    IMedicalReportService medicalReportService,
    IMedicalImageService medicalImageService,
    IWebHostEnvironment environment)
        {
            _chatService = chatService;
            _medicalReportService = medicalReportService;
            _medicalImageService = medicalImageService;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var response = await _chatService.SendMessageAsync(request);
            return Ok(response);
        }

        [HttpPost("analyze-pdf")]
        public async Task<IActionResult> AnalyzePdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a PDF file.");

            var savedFileName = await FileStorageHelper.SavePdfAsync(
                file,
                _environment.WebRootPath);

            using var stream = file.OpenReadStream();

            var response = await _medicalReportService.AnalyzePdfAsync(
                stream,
                savedFileName,
                1); // Temporary patient id

            return Ok(response);
        }

        [HttpPost("analyze-image")]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload an image.");

            using var stream = file.OpenReadStream();

            var response = await _medicalImageService.AnalyzeImageAsync(
                stream,
                file.FileName,
                1); // Temporary patient id

            return Ok(response);
        }
    }
}