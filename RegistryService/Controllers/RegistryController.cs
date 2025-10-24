using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryService.Models;
using RegistryService.Services;

namespace RegistryService.Controllers;

/// <summary>
/// API controller for managing game server registration, allocation, heartbeat monitoring, and player connections.
/// Provides endpoints for game servers to register and send heartbeats, and for authenticated users to allocate servers.
/// </summary>
[ApiController]
[Route($"api/{ApiVersion.Route}/[controller]")]
public class RegistryController : ControllerBase
{
    private readonly IGameServerRegistry registry;

    // DI GameServerRegistry
    public RegistryController(IGameServerRegistry registry)
    {
        this.registry = registry;
    }

    /// <summary>
    /// Registers a new game server or updates an existing one in the registry.
    /// </summary>
    /// <param name="request">The registration request containing server details (ServerId, Host, Port, MaxPlayers).</param>
    /// <returns>An IActionResult indicating success or validation errors.</returns>
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        // EO; if ServerId is missing
        if (string.IsNullOrEmpty(request.ServerId))
        {
            return BadRequest(new { error = "ServerId is required" });
        }

        // EO; if Host is missing
        if (string.IsNullOrEmpty(request.Host))
        {
            return BadRequest(new { error = "Host is required" });
        }

        // create game server instance and register it
        var server = new GameServer
        {
            ServerId = request.ServerId,
            Host = request.Host,
            Port = request.Port,
            MaxPlayers = request.MaxPlayers,
            CurrentPlayers = 0,
            Status = "available"
        };

        registry.RegisterServer(server);

        // return ok and set hearbeat inteval
        return Ok(new
        {
            message = "Server registered successfully",
            serverId = request.ServerId,
            heartbeatInterval = 30  // Unreal should send heartbeat every 30 seconds
        });
    }

    [Authorize]
    [HttpGet("getlist")]
    public IActionResult GetList()
    {
        return Ok(registry.GetList());
    }

    /// <summary>
    /// Receives heartbeat updates from game servers to track their health and current player count.
    /// </summary>
    /// <param name="request">The heartbeat request containing ServerId and CurrentPlayers count.</param>
    /// <returns>An IActionResult indicating success or validation errors.</returns>
    [HttpPost("heartbeat")]
    public IActionResult Heartbeat([FromBody] HeartbeatRequest request)
    {
        // EO; if ServerId is missing
        if (string.IsNullOrEmpty(request.ServerId))
        {
            return BadRequest(new { error = "ServerId is required" });
        }

        // update server heartbeat and player count
        registry.Heartbeat(request.ServerId, request.CurrentPlayers);

        return Ok(new { message = "Heartbeat received" });
    }

    /// <summary>
    /// Allocates an available game server to an authenticated user.
    /// </summary>
    /// <returns>An IActionResult with server connection details or an error if no servers are available.</returns>
    [Authorize]
    [HttpPost("allocate")]
    public IActionResult Allocate()
    {
        // Exstract user id from token
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // EO; if user is not authenticated
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // attempt to allocate an available server
        var server = registry.AllocateServer(userId);

        // EO; if no servers are available
        if (server == null)
        {
            return StatusCode(503, new
            {
                error = "No available servers",
                message = "All game servers are currently full or no servers registered. Please try again later."
            });
        }

        return Ok(new
        {
            serverId = server.ServerId,
            host = server.Host,
            port = server.Port,
            message = $"Allocated to {server.ServerId}"
        });
    }

    /// <summary>
    /// Allocates a specific game server chosen by the authenticated user.
    /// </summary>
    /// <param name="request">The allocation request containing the chosen ServerId.</param>
    /// <returns>An IActionResult with server connection details or an error if the server is unavailable.</returns>
    [Authorize]
    [HttpPost("allocate-chosen")]
    public IActionResult AllocateChosen([FromBody] AllocateChosenRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // EO; if user is not authenticated
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // EO; if ServerId is missing
        if (string.IsNullOrEmpty(request.ServerId))
        {
            return BadRequest(new { error = "ServerId is required" });
        }

        // attempt to allocate the chosen server
        var server = registry.AllocateChosenServer(userId, request.ServerId);

        // EO; if server is not available
        if (server == null)
        {
            return StatusCode(503, new
            {
                error = "Server unavailable",
                message = $"Server {request.ServerId} is either full or does not exist."
            });
        }

        return Ok(new
        {
            serverId = server.ServerId,
            host = server.Host,
            port = server.Port,
            message = $"Allocated to chosen server {server.ServerId}"
        });
    }

    /// <summary>
    /// Handles player disconnection from a game server, updating the player count and server status.
    /// </summary>
    /// <param name="request">The disconnect request containing ServerId.</param>
    /// <returns>An IActionResult indicating success or validation errors.</returns>
    [Authorize]
    [HttpPost("disconnect")]
    public IActionResult Disconnect([FromBody] DisconnectRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // EO; if user is not authenticated
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // EO; if ServerId is missing
        if (string.IsNullOrEmpty(request.ServerId))
        {
            return BadRequest(new { error = "ServerId is required" });
        }

        // disconnect player from server
        registry.DisconnectPlayer(request.ServerId, userId);

        return Ok(new { message = $"User {userId} disconnected from server {request.ServerId}" });
    }
}
