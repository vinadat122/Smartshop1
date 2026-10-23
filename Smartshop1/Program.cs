
using Microsoft.EntityFrameworkCore;
using Smartshop1.Data;
using Smartshop1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SmartShopContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("SmartShop")!
    )
);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<GeminiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
