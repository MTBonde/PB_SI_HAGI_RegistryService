

using RegistryService;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"RegistryService v{ApiVersion.Current} starting...");

// lave gameserver registration service som singleton?

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// JWT validation 

var app = builder.Build();

// setup http contolller


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

// Ping endpoint
app.MapGet("/ping", () => "pong");

// Version endpoint
app.MapGet("/version", () => new { service = "RegistryService", version = RegistryService.ApiVersion.Current });

app.Run();