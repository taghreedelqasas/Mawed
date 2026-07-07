using Maw3ed.BLL.AI.DTOs;
using Maw3ed.BLL.AI.Helpers;
using Maw3ed.BLL.AI.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;

namespace Maw3ed.BLL.AI.MedicalReports
{
    public class MedicalReportService : IMedicalReportService
    {
        private readonly IChatService _chatService;
        private readonly AppDbContext _context;

        public MedicalReportService(
            IChatService chatService,
            AppDbContext context)
        {
            _chatService = chatService;
            _context = context;
        }

        public async Task<AnalyzeFileResponse> AnalyzePdfAsync(
            Stream pdfStream,
            string fileName,
            int patientId)
        {
            var extractedText = PdfTextExtractor.ExtractText(pdfStream);

            var explanation =
                await _chatService.AnalyzeMedicalReportAsync(extractedText);

            var report = new MedicalReportAnalysis
            {
                PatientId = null,
                FileName = fileName,
                FilePath = $"MedicalReports/{fileName}",
                ReportText = extractedText,
                AIExplanation = explanation
            };

            try
            {
                _context.MedicalReportAnalyses.Add(report);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

            return new AnalyzeFileResponse
            {
                Explanation = explanation
            };
        }
    }
}