using Maw3ed.BLL.AI.DTOs;
using Maw3ed.BLL.AI.Helpers;
using Maw3ed.BLL.AI.Interfaces;
using Maw3ed.BLL.AI.Prompts;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Maw3ed.BLL.AI.MedicalImages;

public class MedicalImageService : IMedicalImageService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public MedicalImageService(
        HttpClient http,
        IConfiguration config,
        AppDbContext context)
    {
        _http = http;
        _config = config;
        _context = context;
    }

    public async Task<AnalyzeImageResponse> AnalyzeImageAsync(
        Stream imageStream,
        string fileName,
        int? patientId)
    {
        // Save image
        var webRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot");

        var savedFileName = await ImageStorageHelper.SaveImageAsync(
            imageStream,
            fileName,
            webRoot);

        // Convert image to Base64
        imageStream.Position = 0;

        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms);

        var imageBytes = ms.ToArray();
        var base64 = Convert.ToBase64String(imageBytes);

        // Detect format
        var extension = Path.GetExtension(fileName)
            .TrimStart('.')
            .ToLower();

        if (extension == "jpg" || extension == "jfif")
            extension = "jpeg";

        // Read configuration
        var apiKey = _config["StudentBedrock:ApiKey"];
        var baseUrl = _config["StudentBedrock:BaseUrl"];

        _http.DefaultRequestHeaders.Clear();

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        // Build request
        var request = new
        {
            model_id = "qwen.qwen3-vl-235b-a22b",

            messages = new[]
            {
                new
                {
                    role = "user",

                    text = SystemPrompts.MedicalImageAnalyzer,

                    images = new[]
                    {
                        new
                        {
                            format = extension,
                            data_base64 = base64
                        }
                    }
                }
            },

            max_tokens = 700
        };

        // Call Student Bedrock
        var response = await _http.PostAsJsonAsync(
            $"{baseUrl}/student/multimodal-chat",
            request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {(int)response.StatusCode}");
        Console.WriteLine(responseBody);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Gateway returned {(int)response.StatusCode}: {responseBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<ImageGatewayResponse>();

        if (result == null)
            throw new Exception("No response returned from AI model.");

        // Save analysis
        var analysis = new MedicalImageAnalysis
        {
            // Uncomment when PatientId exists
            // PatientId = patientId,

            FileName = savedFileName,
            FilePath = $"MedicalImages/{savedFileName}",
            AIExplanation = result.OutputText,
            CreatedAt = DateTime.UtcNow
        };

        _context.MedicalImageAnalyses.Add(analysis);

        await _context.SaveChangesAsync();

        return new AnalyzeImageResponse
        {
            Explanation = result.OutputText
        };
    }
}