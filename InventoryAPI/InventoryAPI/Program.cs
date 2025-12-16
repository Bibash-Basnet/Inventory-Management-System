using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();

// Services registration
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

//  JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyText = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");

byte[] keyBytes = Encoding.UTF8.GetBytes(keyText);
if (keyBytes.Length < 32)
{
    using var sha256 = SHA256.Create();
    keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyText));
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[AUTH FAILED] {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var username = context.Principal?.Identity?.Name;
            var role = context.Principal?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            Console.WriteLine($"[TOKEN VALID] User: {username}, Role: {role}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7271",
            "http://localhost:5140",
            "https://localhost:7106",
            "http://localhost:5106"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowMVC");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "product-images");
if (!Directory.Exists(imagePath))
{
    Directory.CreateDirectory(imagePath);
    Console.WriteLine($"[CREATED] Directory: {imagePath}");
}

Console.WriteLine("    Inventory API is Running!");
Console.WriteLine($"    URL: https://localhost:7105");
Console.WriteLine($"    JWT Authentication: ENABLED");
Console.WriteLine($"    CORS: ENABLED for MVC apps");

app.Run();