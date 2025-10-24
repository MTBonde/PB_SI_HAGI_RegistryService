namespace RegistryService.Models;

/// <summary>
/// Request model for allocating a specific chosen game server.
/// </summary>
public class AllocateChosenRequest
{
    public string ServerId { get; set; } = string.Empty;
}
