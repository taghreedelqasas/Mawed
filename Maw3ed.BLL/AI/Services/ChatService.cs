using Maw3ed.BLL.AI.DTOs;
using Maw3ed.BLL.AI.Interfaces;
using Maw3ed.BLL.AI.Prompts;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Maw3ed.BLL.AI.Services;

public class ChatService : IChatService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public ChatService(
        HttpClient http,
        IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
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

        return new ChatResponse
        {
            Reply = result?.OutputText ?? "No response received."
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