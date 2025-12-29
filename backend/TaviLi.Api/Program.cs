using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaviLi.Domain.Entities;
using TaviLi.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- 1. הוספת שירותים (Services) ---

//  מפעיל את ה-Controllers
builder.Services.AddControllers(); 

//  הגדרת CORS כדי לאפשר לאנגולר לתקשר עם ה-API
    //  הגדרת CORS כדי לאפשר לאנגולר לתקשר עם ה-API
    //  הגדרת CORS דינמית
    var allowedOrigins = builder.Configuration["AllowedOrigins"]?
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) 
        ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ProductionCors",
            policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .WithExposedHeaders("X-Total-Unread");
            });
    });

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Registration", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5; // מקסימום 5 נסיונות בדקה
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});
//  חיבור לבסיס הנתונים (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));
builder.Services.AddScoped<TaviLi.Application.Common.Interfaces.IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
    
//  הגדרת Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    // מדיניות סיסמאות
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8; // מינימום 8 תווים
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // מנגנון נעילה (Lockout)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // נעילה ל-5 דקות
    options.Lockout.MaxFailedAccessAttempts = 10; // אחרי 10 כישלונות
    options.Lockout.AllowedForNewUsers = true;
})
.AddPasswordValidator<TaviLi.Infrastructure.Identity.PasswordLetterValidator<User>>() // ולידטור מותאם אישית (לפחות אות אחת)
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
builder.Services.AddScoped<TaviLi.Application.Common.Interfaces.INotificationService, TaviLi.Infrastructure.Services.FirebaseNotificationService>();

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

// שימוש ב-Middleware לטיפול בשגיאות (חייב להיות ראשון)
app.UseMiddleware<TaviLi.Infrastructure.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();

// הפעלת Rate Limiting
app.UseRateLimiter();

//  הפעלת CORS
app.UseCors("ProductionCors");

//  הפעלת מערכת האימות וההרשאות
app.UseAuthentication();
app.UseAuthorization();

// מיפוי ה-Controllers 
app.MapControllers();

// 1. נתיב קליל ל-Render (ללא קריאה לדאטה בייס)
app.MapGet("/ping", () => Results.Ok("Pong"));

// 2. נתיב כבד ל-Supabase (מבצע שאילתה כדי למנוע הקפאה)
app.MapGet("/db-ping", async (TaviLi.Infrastructure.Persistence.ApplicationDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect ? Results.Ok("DB Active") : Results.Problem("DB Down");
});

app.Run();
