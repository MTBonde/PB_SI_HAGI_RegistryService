using RegistryService.Models;

namespace RegistryService.Services;

public class GameServerRegistry : IGameServerRegistry
{
    
    private readonly List<GameServer> servers = new();
    private readonly object lockObject = new();
    
    public GameServerRegistry()
    {
        Console.WriteLine("GameServerRegistry initialized (empty - waiting for Unreal server registration)");
    }
    
    public void RegisterServer(GameServer server)
    {
        lock (lockObject)
        {
            var existing = servers.FirstOrDefault(s => s.ServerId == server.ServerId);
            if (existing != null)
            {
                // Update existing server
                existing.Host = server.Host;
                existing.Port = server.Port;
                existing.MaxPlayers = server.MaxPlayers;
                existing.LastHeartbeat = DateTime.UtcNow;
                existing.Status = "available";
                Console.WriteLine($"Updated game server: {server.ServerId} at {server.Host}:{server.Port}");
            }
            else
            {
                // Add new server
                server.LastHeartbeat = DateTime.UtcNow;
                server.Status = "available";
                server.CurrentPlayers = 0;
                server.MaxPlayers = server.MaxPlayers;
                servers.Add(server);
                Console.WriteLine($"Registered NEW game server: {server.ServerId} at {server.Host}:{server.Port} (max {server.MaxPlayers} players)");
            }
        }
    }
    // round robin?
    
    // allocate chosen server
    
    // let GS self register
    
    // get all servers method
    
    // GS healthcheck
    public string? AllocateServer()
    {
        throw new NotImplementedException();
    }

    public GameServer? AllocateServer(string userId)
    {
        throw new NotImplementedException();
    }


    public void Heartbeat(string serverId, int currentPlayers)
    {
        throw new NotImplementedException();
    }
}