using RegistryService.Models;

namespace RegistryService.Services;

public interface IGameServerRegistration
{
    GameServer? AllocateServer(string userId);
    void RegisterServer(GameServer server);
    void Heartbeat(string serverId, int currentPlayers);
}