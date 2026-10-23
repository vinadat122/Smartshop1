
using Microsoft.EntityFrameworkCore;
using Smartshop1.Data;
using Smartshop1.Services;

var builder = WebApplication.CreateBuilder(args);

// Kết nối MySQL database SMARTSHOP
builder.Services.AddDbContext<SmartShopContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("SmartShop")!
    )
);

// Đăng ký Gemini service sử dụng HttpClient
builder.Services.AddHttpClient<GeminiService>();

// Đăng ký API Controllers
builder.Services.AddControllers();

// Đăng ký OpenAPI cho .NET 10
builder.Services.AddOpenApi();

var app = builder.Build();

// Cấu hình OpenAPI trong môi trường Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Cấu hình HTTP request pipeline
app.UseHttpsRedirection();

app.UseAuthorization();

// Ánh xạ các API Controllers
app.MapControllers();

app.Run();
