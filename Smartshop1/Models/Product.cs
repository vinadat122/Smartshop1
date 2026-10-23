
namespace Smartshop1.Models;

public class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Features { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<Review> Reviews { get; set; } = new();
}
