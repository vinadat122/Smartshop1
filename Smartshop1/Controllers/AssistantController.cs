
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartshop1.Data;
using Smartshop1.Models;
using Smartshop1.Services;

namespace Smartshop1.Controllers;

[ApiController]
[Route("api/assistant")]
public class AssistantController : ControllerBase
{
    private readonly SmartShopContext _db;
    private readonly GeminiService _gemini;

    public AssistantController(
        SmartShopContext db,
        GeminiService gemini)
    {
        _db = db;
        _gemini = gemini;
    }

    public record ChatRequest(string SessionId, string Message);

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new
            {
                message = "SessionId và Message không được để trống."
            });
        }

        if (request.SessionId.Length > 100)
        {
            return BadRequest(new
            {
                message = "SessionId tối đa 100 ký tự."
            });
        }

        if (request.Message.Length > 2000)
        {
            return BadRequest(new
            {
                message = "Tin nhắn tối đa 2000 ký tự."
            });
        }

        var products = await _db.Products
            .AsNoTracking()
            .ToListAsync();

        if (products.Count == 0)
        {
            return BadRequest(new
            {
                message = "Chưa có dữ liệu sản phẩm trong hệ thống."
            });
        }

        var productContext = string.Join(
            "\n\n",
            products.Select(p => $"""
                Tên: {p.ProductName}
                Danh mục: {p.Category}
                Giá: {p.Price}
                Tính năng: {p.Features}
                Mô tả: {p.Description ?? "Chưa có mô tả"}
                """));

        // Lấy tối đa 10 tin nhắn gần nhất trong phiên chat
        var history = await _db.ChatMessages
            .Where(c => c.SessionId == request.SessionId)
            .OrderByDescending(c => c.Id)
            .Take(10)
            .ToListAsync();

        history.Reverse();

        try
        {
            var reply = await _gemini.ChatAsync(
                request.Message.Trim(),
                productContext,
                history);

            // Lưu câu hỏi của khách hàng
            _db.ChatMessages.Add(new ChatMessage
            {
                SessionId = request.SessionId,
                Role = "user",
                Content = request.Message.Trim(),
                CreatedAt = DateTime.Now
            });

            // Lưu câu trả lời của chatbot
            _db.ChatMessages.Add(new ChatMessage
            {
                SessionId = request.SessionId,
                Role = "model",
                Content = reply,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                sessionId = request.SessionId,
                message = request.Message.Trim(),
                reply
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(502, new
            {
                message = ex.Message
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                message = "Không kết nối được tới Gemini API."
            });
        }
    }
}
