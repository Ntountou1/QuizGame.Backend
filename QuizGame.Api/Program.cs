using QuizGame.Application.Services;
using QuizGame.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Dependency Injection
builder.Services.AddScoped<PlayerRepository>();
builder.Services.AddScoped<PlayerService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
