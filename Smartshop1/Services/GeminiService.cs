
using System.Net.Http.Json;
using System.Text.Json;

namespace Smartshop1.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GenerateDescriptionAsync(
        string productName,
        string category,
        string features)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API key chưa được cấu hình.");

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var prompt = $"""
            Bạn là chuyên gia viết nội dung thương mại điện tử.
            Hãy viết mô tả sản phẩm chuyên nghiệp bằng tiếng Anh, tối thiểu 80 từ.
            Chỉ sử dụng thông tin được cung cấp, không bịa thông số.

            Product name: {productName}
            Category: {category}
            Features: {features}

            Chỉ trả về nội dung mô tả, không thêm tiêu đề hay lời dẫn.
            """;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Gemini API lỗi {(int)response.StatusCode}: {responseText}");

        using var document = JsonDocument.Parse(responseText);

        var description = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidOperationException("Gemini không trả về mô tả.");

        return description.Trim();
    }
    public async Task<string> AnalyzeReviewAsync(string content)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var prompt = $"""
        Analyze this customer review.
        Return JSON only, with these fields:
        sentiment: Positive, Negative, or Neutral
        score: number from 0 to 1
        summary: short summary, maximum 256 characters
        Review: {content}
        """;

        var body = new
        {
            contents = new[]
            {
            new { parts = new[] { new { text = prompt } } }
        },
            generationConfig = new { responseMimeType = "application/json" }
        };

        using var response = await _httpClient.PostAsJsonAsync(url, body);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(responseText);

        using var doc = JsonDocument.Parse(responseText);
        return doc.RootElement.GetProperty("candidates")[0]
            .GetProperty("content").GetProperty("parts")[0]
            .GetProperty("text").GetString()!;
    }
}
