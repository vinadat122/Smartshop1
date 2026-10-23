using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Smartshop1.Data;
using Smartshop1.Models;
using Smartshop1.Services;

namespace Smartshop1.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly SmartShopContext _db;
    private readonly GeminiService _gemini;

    public ReviewsController(SmartShopContext db, GeminiService gemini)
    {
        _db = db;
        _gemini = gemini;
    }

    public record ReviewRequest(
        int ProductId, string CustomerName, string Content, int Rating);

    [HttpPost("analyzereview")]
    public async Task<IActionResult> AnalyzeReview(ReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest("Rating must be between 1 and 5.");

        var product = await _db.Products.FindAsync(request.ProductId);
        if (product == null) return NotFound("Product not found.");

        try
        {
            var json = await _gemini.AnalyzeReviewAsync(request.Content);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var sentiment = root.GetProperty("sentiment").GetString() ?? "Neutral";
            var score = root.GetProperty("score").GetDouble();
            var summary = root.GetProperty("summary").GetString() ?? "";

            if (!new[] { "Positive", "Negative", "Neutral" }
                .Contains(sentiment) || score < 0 || score > 1)
                return StatusCode(502, "Invalid Gemini response.");

            var review = new Review
            {
                ProductId = request.ProductId,
                CustomerName = request.CustomerName,
                Content = request.Content,
                Rating = request.Rating,
                Sentiment = sentiment,
                Score = score,
                Summary = summary.Length > 256 ? summary[..256] : summary,
                CreatedAt = DateTime.Now
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();
            return Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetReviews()
        => Ok(await _db.Reviews.ToListAsync());
}