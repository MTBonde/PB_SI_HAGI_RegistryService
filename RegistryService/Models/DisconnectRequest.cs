namespace RegistryService.Models;

/// <summary>
/// Request model for handling player disconnection from a game server.
/// </summary>
public class DisconnectRequest
{
    public string ServerId { get; set; } = string.Empty;
}
