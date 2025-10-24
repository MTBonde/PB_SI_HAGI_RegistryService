using RegistryService.Models;

namespace RegistryService.Services;

public interface IGameServerRegistry
{
    GameServer? AllocateServer(string userId);
    GameServer? AllocateChosenServer(string userId, string serverId);
    void DisconnectPlayer(string serverId, string userId);
    void RegisterServer(GameServer server);
    void Heartbeat(string serverId, int currentPlayers);
}