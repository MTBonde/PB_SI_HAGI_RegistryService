namespace RegistryService.Models;

public class RegisterRequest
{
    public string ServerId { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public int MaxPlayers { get; set; } = 10;
}