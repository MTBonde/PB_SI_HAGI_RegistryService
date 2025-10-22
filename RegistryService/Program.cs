var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.Logger.LogInformation("RegistryService v{Version} starting...", RegistryService.ApiVersion.Current);

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