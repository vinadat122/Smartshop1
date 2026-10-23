
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
    public async Task<string> ChatAsync(
    string message,
    string productContext,
    List<Smartshop1.Models.ChatMessage> history)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API key chưa được cấu hình.");

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var contents = new List<object>();

        foreach (var item in history)
        {
            if (item.Role != "user" && item.Role != "model")
                continue;

            contents.Add(new
            {
                role = item.Role,
                parts = new[] { new { text = item.Content } }
            });
        }

        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = message } }
        });

        var body = new
        {
            systemInstruction = new
            {
                parts = new[]
                {
                new
                {
                    text = $"""
                    Bạn là trợ lý tư vấn sản phẩm của SmartShop.

                    CHỈ trả lời câu hỏi liên quan đến các sản phẩm có trong dữ liệu
                    được cung cấp, ví dụ: tên, giá, danh mục, tính năng và mô tả.

                    Nếu câu hỏi không liên quan đến sản phẩm SmartShop,
                    hãy từ chối lịch sự và hướng người dùng quay lại chủ đề sản phẩm.

                    Chỉ sử dụng dữ liệu sản phẩm được cung cấp.
                    Không tự bịa giá, tính năng hoặc thông số.
                    Trả lời bằng tiếng Việt, rõ ràng và thân thiện.

                    DỮ LIỆU SẢN PHẨM:
                    {productContext}
                    """
                }
            }
            },
            contents = contents
        };

        using var response = await _httpClient.PostAsJsonAsync(url, body);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Gemini API lỗi {(int)response.StatusCode}: {responseText}");

        using var document = JsonDocument.Parse(responseText);

        var reply = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(reply))
            throw new InvalidOperationException("Gemini không trả về câu trả lời.");

        return reply.Trim();
    }
}
