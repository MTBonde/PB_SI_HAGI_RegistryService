using System.Text;
using Hagi.Robust;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RegistryService;
using RegistryService.Services;
using k8s;
using k8s.KubeConfigModels;

string GetApplicationVersion()
{
    const string versionFilePath = "version.txt";
    const string fallbackVersion = ApiVersion.Current;

    try
    {
        if (File.Exists(versionFilePath))
        {
            return File.ReadAllText(versionFilePath).Trim();
        }
    }
    catch (Exception)
    {
        // If reading fails, fall back to ApiVersion
    }

    return fallbackVersion;
}

// create web app builder
var builder = WebApplication.CreateBuilder(args);

// configure API controllers and documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHagiResilience(); // add rubostness from our nuget

try
{
    var k8sConfig = KubernetesClientConfiguration.InClusterConfig();
    Kubernetes kubernetes = new Kubernetes(k8sConfig);

    builder.Services.AddSingleton<IKubernetes>(kubernetes);
    Console.WriteLine("Registered Kubernetes client");
}
catch (Exception e)
{
    Console.WriteLine("ERROR! Failed to create Kubernetes. Reason: " + e);
    throw;
}

// register gameserverregistry as singleton 
builder.Services.AddSingleton<IGameServerRegistry, GameServerRegistry>();

// configure JWT authentication for securing user endpoints
var jwtSecret = "superSecretKey@345superSecretKey@345";
var key = Encoding.UTF8.GetBytes(jwtSecret);

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

var applicationVersion = GetApplicationVersion();
app.Logger.LogInformation("RegistryService v{Version} starting...", applicationVersion);

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
app.MapGet("/version", () => new { service = "RegistryService", version = applicationVersion });

app.MapReadinessEndpoint(); // add end point readyness from our nuget

// start the application server
Console.WriteLine("RegistryService listening on http://+:8080");
app.Run();

// make Program class accessible for integration tests
public partial class Program { }