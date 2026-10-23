
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smartshop1.Data;
using Smartshop1.Services;

namespace Smartshop1.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly SmartShopContext _context;
    private readonly GeminiService _geminiService;

    public ProductsController(
        SmartShopContext context,
        GeminiService geminiService)
    {
        _context = context;
        _geminiService = geminiService;
    }

    [HttpPost("{id:int}/generatedescription")]
    public async Task<IActionResult> GenerateDescription(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound(new { message = "Không tìm thấy sản phẩm." });

        try
        {
            var description = await _geminiService.GenerateDescriptionAsync(
                product.ProductName,
                product.Category,
                product.Features);

            product.Description = description;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                product.ProductId,
                product.ProductName,
                product.Description
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(502, new { message = ex.Message });
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
