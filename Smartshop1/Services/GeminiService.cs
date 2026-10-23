
using System.Net.Http.Json;
using System.Text.Json;
using Smartshop1.Models;

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
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    // Lấy API key và model từ appsettings.json
    private string GetApiUrl()
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"];

        if (string.IsNullOrWhiteSpace(apiKey) ||
            apiKey == "YOUR_GEMINI_API_KEY")
        {
            throw new InvalidOperationException(
                "Gemini API key chưa được cấu hình.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InvalidOperationException(
                "Gemini:Model chưa được cấu hình trong appsettings.json.");
        }

        Console.WriteLine($"Gemini model đang dùng: {model}");

        return $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
    }

    // Gửi yêu cầu đến Gemini và xử lý phản hồi chung
    private async Task<string> SendToGeminiAsync(object requestBody)
    {
        var url = GetApiUrl();

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                url, requestBody);

            var responseText =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Gemini API lỗi {(int)response.StatusCode}: {responseText}");
            }

            using var document = JsonDocument.Parse(responseText);
            var root = document.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];

                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts))
                {
                    var texts = parts.EnumerateArray()
                        .Where(p => p.TryGetProperty("text", out _))
                        .Select(p => p.GetProperty("text").GetString())
                        .Where(t => !string.IsNullOrWhiteSpace(t));

                    var result = string.Join("\n", texts);

                    if (!string.IsNullOrWhiteSpace(result))
                        return result.Trim();
                }
            }

            throw new InvalidOperationException(
                "Gemini không trả về nội dung hợp lệ.");
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException(
                "Gemini không phản hồi trong 30 giây. " +
                "Hãy kiểm tra kết nối Internet và model Gemini.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                "Không kết nối được tới Gemini API. Hãy kiểm tra Internet.", ex);
        }
    }

    // API 1: Tạo mô tả sản phẩm
    public async Task<string> GenerateDescriptionAsync(
        string productName,
        string category,
        string features)
    {
        var prompt = $"""
            You are a professional e-commerce copywriter.

            Write a professional product description in English
            with at least 80 words.

            Only use the product information provided.
            Do not invent specifications or features.

            Product name: {productName}
            Category: {category}
            Features: {features}

            Return only the description, without a title.
            """;

        var body = new
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

        return await SendToGeminiAsync(body);
    }

    // API 2: Phân tích đánh giá
    public async Task<string> AnalyzeReviewAsync(string content)
    {
        var prompt = $"""
            Analyze this customer review.

            Return a valid JSON object only, without Markdown:
            - sentiment: Positive, Negative, or Neutral
            - score: number from 0 to 1
            - summary: short summary, maximum 256 characters

            Review: {content}
            """;

        var body = new
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
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        return await SendToGeminiAsync(body);
    }

    // API 3: Chatbot tư vấn sản phẩm
    public async Task<string> ChatAsync(
        string message,
        string productContext,
        List<ChatMessage> history)
    {
        var contents = new List<object>();

        foreach (var item in history)
        {
            if (item.Role != "user" && item.Role != "model")
                continue;

            contents.Add(new
            {
                role = item.Role,
                parts = new[]
                {
                    new { text = item.Content }
                }
            });
        }

        contents.Add(new
        {
            role = "user",
            parts = new[]
            {
                new { text = message }
            }
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
                            Bạn là trợ lý tư vấn sản phẩm SmartShop.

                            Chỉ trả lời câu hỏi liên quan đến sản phẩm
                            trong dữ liệu được cung cấp.

                            Nếu câu hỏi không liên quan đến sản phẩm,
                            hãy từ chối lịch sự.

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

        return await SendToGeminiAsync(body);
    }
}
