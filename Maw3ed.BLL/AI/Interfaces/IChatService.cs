using Maw3ed.BLL.AI.DTOs;

namespace Maw3ed.BLL.AI.Interfaces;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request, int patientId);

    Task<string> AnalyzeMedicalReportAsync(string extractedText);
}