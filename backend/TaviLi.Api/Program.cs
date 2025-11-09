using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaviLi.Domain.Entities;
using TaviLi.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. קריאת מחרוזת החיבור
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. רישום ה-DbContext
builder.Services.AddDbContext<TaviLi.Infrastructure.Persistence.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. רישום שירותי Identity
builder.Services.AddIdentity<TaviLi.Domain.Entities.User, TaviLi.Domain.Entities.Role>(options =>
{
    // הגדרות אופציונליות לסיסמה (מומלץ ל-MVP)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<TaviLi.Infrastructure.Persistence.ApplicationDbContext>()
    .AddDefaultTokenProviders();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
