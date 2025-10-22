using Microsoft.AspNetCore.Mvc;

namespace RegistryService.Controllers;

[ApiController]
[Route($"api/{ApiVersion.Route}/[controller]")]
public class RegistryController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterServerRequest request)
    {
        // hardcoded registration
        return Ok(new { message = "Server registered" });
    }

    [HttpGet("allocate")]
    public IActionResult Allocate()
    {
        // Hardcoded allocation always returns game1
        return Ok(new
        {
            gameUrl = "http://game1:8080",
            serverId = "game1"
        });
    }
}

public record RegisterServerRequest(string ServerUrl, int Port);
