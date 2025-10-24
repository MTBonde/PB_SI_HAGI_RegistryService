using RegistryService.Models;

namespace RegistryService.Services;

public class GameServerRegistration : IGameServerRegistration
{
    
    // hardcode liste af GS adresse og porte
    
    
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

    public void RegisterServer(GameServer server)
    {
        throw new NotImplementedException();
    }

    public void Heartbeat(string serverId, int currentPlayers)
    {
        throw new NotImplementedException();
    }
}