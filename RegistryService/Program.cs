using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RegistryService;
using RegistryService.Services;

// create web app builder
var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"RegistryService v{ApiVersion.Current} starting...");

// configure API controllers and documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register gameserverregistry as singleton 
builder.Services.AddSingleton<IGameServerRegistry, GameServerRegistry>();

// configure JWT authentication for securing user endpoints
var jwtSecret = "superSecretKey@345superSecretKey@345";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// build the application
var app = builder.Build();

// enable OpenAPI endpoint in development environment
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// enable Swagger UI for API documentation and testing
app.UseSwagger();
app.UseSwaggerUI();

// configure authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// map API controllers to routes
app.MapControllers();

// configure health check and utility endpoints
app.MapGet("/ping", () => "pong");
app.MapGet("/health", () => "healthy");
app.MapGet("/version", () => ApiVersion.Current);

// start the application server
Console.WriteLine("RegistryService listening on http://+:8080");
app.Run();

// make Program class accessible for integration tests
public partial class Program { }