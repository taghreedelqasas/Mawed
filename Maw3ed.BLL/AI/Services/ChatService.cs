using Maw3ed.BLL.AI.DTOs;
using Maw3ed.BLL.AI.Interfaces;
using Maw3ed.BLL.AI.Prompts;
using Maw3ed.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Maw3ed.BLL.AI.Services;

public class ChatService : IChatService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public ChatService(
        HttpClient http,
        IConfiguration config,
        AppDbContext context)
    {
        _http = http;
        _config = config;
        _context = context;
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request, int patientId)
    {
        // هات جلسة الشات بتاعة المريض ده، أو اعمل واحدة جديدة لو أول مرة
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(s => s.PatientId == patientId);

        if (session == null)
        {
            session = new ChatSession
            {
                PatientId = patientId,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
        }

        // احفظ رسالة المريض
        _context.ChatMessages.Add(new ChatMessage
        {
            ChatSessionId = session.Id,
            Sender = "Patient",
            Message = request.Message,
            IsAiResponse = false,
            CreatedAt = DateTime.UtcNow
        });

        var apiKey = _config["StudentBedrock:ApiKey"];

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var body = new
        {
            model_id = _config["StudentBedrock:Model"],
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = request.Message
                }
            },
            system_prompt = SystemPrompts.MedicalAssistant,
            max_tokens = 300
        };

        var response = await _http.PostAsJsonAsync(
            $"{_config["StudentBedrock:BaseUrl"]}/student/chat",
            body);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BedrockChatResponse>();

        var reply = result?.OutputText ?? "No response received.";

        // احفظ رد الـ AI
        _context.ChatMessages.Add(new ChatMessage
        {
            ChatSessionId = session.Id,
            Sender = "AI",
            Message = reply,
            IsAiResponse = true,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new ChatResponse
        {
            Reply = reply
        };
    }

    public async Task<string> AnalyzeMedicalReportAsync(string extractedText)
    {
        var apiKey = _config["StudentBedrock:ApiKey"];

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var body = new
        {
            model_id = _config["StudentBedrock:Model"],
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = extractedText
                }
            },
            system_prompt = SystemPrompts.MedicalReportAnalyzer,
            max_tokens = 700
        };

        var response = await _http.PostAsJsonAsync(
            $"{_config["StudentBedrock:BaseUrl"]}/student/chat",
            body);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BedrockChatResponse>();

        return result?.OutputText ?? "No explanation received.";
    }
}