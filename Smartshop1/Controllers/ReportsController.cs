using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartshop1.Data;

namespace Smartshop1.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly SmartShopContext _db;

    public ReportsController(SmartShopContext db) => _db = db;

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReport()
    {
        var report = await _db.Products.Select(p => new
        {
            p.ProductId,
            p.ProductName,
            TotalReviews = p.Reviews.Count,
            Positive = p.Reviews.Count(r => r.Sentiment == "Positive"),
            Negative = p.Reviews.Count(r => r.Sentiment == "Negative"),
            Neutral = p.Reviews.Count(r => r.Sentiment == "Neutral"),
            AverageRating = p.Reviews.Any()
                ? p.Reviews.Average(r => (double)r.Rating) : 0
        }).ToListAsync();

        return Ok(report);
    }
}