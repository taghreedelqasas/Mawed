using Maw3ed.BLL.AI.DTOs;

namespace Maw3ed.BLL.AI.Interfaces;

public interface IMedicalReportService
{
    Task<AnalyzeFileResponse> AnalyzePdfAsync(
        Stream pdfStream,
        string fileName,
        int patientId);
}