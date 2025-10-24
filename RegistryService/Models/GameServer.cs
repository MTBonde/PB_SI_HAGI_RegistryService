namespace RegistryService.Models;

public class GameServer
{
    public string ServerId { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; } = 10;
    public string Status { get; set; } = "available";
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
}