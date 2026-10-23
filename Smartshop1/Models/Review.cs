
namespace Smartshop1.Models;

public class Review
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Sentiment { get; set; } = "Neutral";

    public double Score { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Product? Product { get; set; }
}
