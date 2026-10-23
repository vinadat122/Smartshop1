
using Microsoft.EntityFrameworkCore;
using Smartshop1.Models;

namespace Smartshop1.Data;

public class SmartShopContext : DbContext
{
    public SmartShopContext(DbContextOptions<SmartShopContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product table
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.HasKey(p => p.ProductId);

            entity.Property(p => p.ProductName)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(p => p.Category)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.Features)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasColumnType("longtext")
                .IsRequired(false);
        });

        // Review table
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Review");

            entity.HasKey(r => r.ReviewId);

            entity.Property(r => r.ReviewId)
                .ValueGeneratedOnAdd();

            entity.Property(r => r.CustomerName)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(r => r.Content)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(r => r.Rating)
                .IsRequired();

            entity.Property(r => r.Sentiment)
                .HasMaxLength(16)
                .IsRequired();

            entity.Property(r => r.Score)
                .HasColumnType("double")
                .IsRequired();

            entity.Property(r => r.Summary)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(r => r.CreatedAt)
                .HasColumnType("datetime")
                .IsRequired();

            entity.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage table
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessage");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.SessionId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Role)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(c => c.Content)
                .HasColumnType("longtext")
                .IsRequired();

            entity.Property(c => c.CreatedAt)
                .HasColumnType("datetime")
                .IsRequired();

            entity.HasIndex(c => new
            {
                c.SessionId,
                c.CreatedAt
            });
        });
    }
}
