using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuizGame.Application.Interfaces;
using QuizGame.Application.Services;
using QuizGame.Domain.Interfaces;
using QuizGame.Infrastructure.Repositories;
using Serilog;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

//Add Serilog to Application
builder.Host.UseSerilog();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Dependency Injection
builder.Services.AddScoped<PlayerRepository>();
builder.Services.AddSingleton<IPlayerRepository, PlayerRepository>();
builder.Services.AddSingleton<IPlayerService, PlayerService>();

// Add TokenService for JWT
var secretKey = builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddSingleton(new TokenService(secretKey));

// Update PlayerService to inject TokenService
builder.Services.AddScoped<PlayerService>(provider =>
{
    var repo = provider.GetRequiredService<PlayerRepository>();
    var tokenService = provider.GetRequiredService<TokenService>();
    var logger = provider.GetRequiredService<ILogger<PlayerService>>();
    return new PlayerService(repo, logger, tokenService);
});

// Keep your existing controllers registration
builder.Services.AddControllers();

// --- New JWT authentication & authorization ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// --- Add authentication/authorization middleware ---
app.UseAuthentication();
app.UseAuthorization();

// Keep your existing endpoint mapping
app.MapControllers();

app.Run();
