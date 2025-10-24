namespace RegistryService.Models;

public class HeartbeatRequest
{
    public string ServerId { get; set; } = string.Empty;
    public int CurrentPlayers { get; set; }
}