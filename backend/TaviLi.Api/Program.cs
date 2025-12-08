using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaviLi.Domain.Entities;
using TaviLi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- 1. הוספת שירותים (Services) ---

//  מפעיל את ה-Controllers
builder.Services.AddControllers(); 

//  הגדרת CORS כדי לאפשר לאנגולר לתקשר עם ה-API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // הכתובת של האנגולר
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
//  חיבור לבסיס הנתונים (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<TaviLi.Application.Common.Interfaces.IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
    
//  הגדרת Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//  הגדרת אימות (Authentication) עם JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found.")))
    };
});

//  MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(TaviLi.Application.Common.Dtos.AuthResponseDto).Assembly));

//  שירות יצירת הטוקנים
builder.Services.AddScoped<TaviLi.Application.Common.Interfaces.IAuthService, TaviLi.Infrastructure.Services.AuthService>();

// מאפשר לכל שירות לבקש גישה ל-HttpContext הנוכחי
builder.Services.AddHttpContextAccessor(); 

// רושם את השירות החדש שיצרנו
builder.Services.AddScoped<TaviLi.Application.Common.Interfaces.ICurrentUserService, TaviLi.Infrastructure.Services.CurrentUserService>();
builder.Services.AddScoped<TaviLi.Application.Common.Interfaces.IImageService, TaviLi.Infrastructure.Services.CloudinaryService>();

var app = builder.Build();

// --- 2. הרצת Seeding (יצירת תפקידים אוטומטית) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        await ApplicationDbContextSeed.SeedRolesAsync(roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// --- 3. הגדרת הצינור (Pipeline) ---

app.UseHttpsRedirection();

//  הפעלת CORS
app.UseCors("AllowAngularDev");

//  הפעלת מערכת האימות וההרשאות
app.UseAuthentication();
app.UseAuthorization();

// מיפוי ה-Controllers 
app.MapControllers();

app.Run();
