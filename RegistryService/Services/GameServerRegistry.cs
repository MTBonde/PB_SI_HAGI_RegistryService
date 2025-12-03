using k8s;
using k8s.Models;
using RegistryService.Models;

namespace RegistryService.Services;

/// <summary>
/// Thread-safe registry for managing game server instances, tracking player counts, and handling server allocation.
/// Maintains a list of registered game servers and provides operations for registration, heartbeat updates,
/// player allocation, and disconnection handling.
/// ! - Change list to Redis in next iteration
/// </summary>
public class GameServerRegistry : IGameServerRegistry
{
    // List of GameServers
    private readonly List<GameServer> servers = new();
    
    // Lock for avoiding race condition
    private readonly object lockObject = new();
    
    //Kubernetes
    private IKubernetes? kubernetesClient = null;
    private const int maxPods = 10;
    private const string gameServerDeploymentName = "gameserver";
    private string namespaceParameter = "dev";


    public GameServerRegistry(IKubernetes? kubernetesClient)
    {
        this.kubernetesClient = kubernetesClient;

        if (this.kubernetesClient == null)
        {
            Console.WriteLine("Kubernetes was null...");
        }
        
        Console.WriteLine("GameServerRegistry initialized (empty - waiting for Unreal server registration)");
    }

    /// <summary>
    /// Registers a game server or updates an existing one in the registry.
    /// </summary>
    /// <param name="server">The game server to be registered or updated in the registry.</param>
    public void RegisterServer(GameServer server)
    {
        lock (lockObject)
        {
            var existing = servers.FirstOrDefault(s => s.ServerId == server.ServerId);

            // EO; update existing server
            if (existing != null)
            {
                existing.Host = server.Host;
                existing.Port = server.Port;
                existing.MaxPlayers = server.MaxPlayers;
                existing.LastHeartbeat = DateTime.UtcNow;
                existing.Status = "available";
                Console.WriteLine($"Updated game server: {server.ServerId} at {server.Host}:{server.Port}");
                return;
            }

            // add new server to registry with default values
            server.LastHeartbeat = DateTime.UtcNow;
            server.Status = "available";
            server.CurrentPlayers = 0;
            server.MaxPlayers = server.MaxPlayers;
            servers.Add(server);
            Console.WriteLine($"Registered NEW game server: {server.ServerId} at {server.Host}:{server.Port} (max {server.MaxPlayers} players)");
        }
    }

    /// <summary>
    /// Updates the heartbeat of a game server and the current player count in the registry.
    /// </summary>
    /// <param name="serverId">The unique identifier of the game server to update.</param>
    /// <param name="currentPlayers">The current number of active players on the game server.</param>
    /// <exception cref="NotImplementedException">Thrown when the method is not yet implemented.</exception>
    public void Heartbeat(string serverId, int currentPlayers) //TODO: get current players
    {
        lock (lockObject)
        {
            var server = servers.FirstOrDefault(s => s.ServerId == serverId);

            // EO; if server not found
            if (server == null)
            {
                Console.WriteLine($"WARNING: Heartbeat from UNKNOWN server: {serverId}");
                return;
            }

            // update server heartbeat timestamp, player count, and status
            server.LastHeartbeat = DateTime.UtcNow;
            server.CurrentPlayers = currentPlayers;
            server.Status = currentPlayers >= server.MaxPlayers ? "full" : "available";
            Console.WriteLine($"Heartbeat from {serverId}: {currentPlayers}/{server.MaxPlayers} players (status: {server.Status})");

            Task.Run(async()=>await CheckServers());
        }
    }

    public List<GameServer> GetList()
    {
        lock (lockObject)
        {
            return servers;
        }
    }

    /// <summary>
    /// Allocates an available game server for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting a game server.</param>
    /// <returns>A <see cref="GameServer"/> instance representing an available game server, or null if no servers are available.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not yet implemented.</exception>
    public GameServer? AllocateServer(string userId)
    {
        lock (lockObject)
        {
            // Find first available server that is not full
            var server = servers.FirstOrDefault(s =>
                s.Status == "available" &&
                s.CurrentPlayers < s.MaxPlayers);

            // EO; if no available server
            if (server == null)
            {
                Console.WriteLine($"No available servers for user {userId} (all servers full or no servers registered)");
                return null;
            }

            // add player to server, and check if server is now full, if so change server status
            server.CurrentPlayers++;
            if (server.CurrentPlayers >= server.MaxPlayers)
            {
                server.Status = "full";
            }

            Console.WriteLine($"Allocated {server.ServerId} to user {userId}. Players: {server.CurrentPlayers}/{server.MaxPlayers}");
            return server;
        }
    }

    /// <summary>
    /// Disconnects a player from a game server, decreasing the player count and updating server status.
    /// </summary>
    /// <param name="serverId">The unique identifier of the game server from which the player is disconnecting.</param>
    /// <param name="userId">The unique identifier of the user disconnecting from the game server.</param>
    public void DisconnectPlayer(string serverId, string userId)
    {
        lock (lockObject)
        {
            var server = servers.FirstOrDefault(s => s.ServerId == serverId);

            // EO; if server not found
            if (server == null)
            {
                Console.WriteLine($"WARNING: Disconnect from UNKNOWN server: {serverId} for user {userId}");
                return;
            }

            // EO; if server already empty
            if (server.CurrentPlayers <= 0)
            {
                Console.WriteLine($"WARNING: Disconnect from server {serverId} but CurrentPlayers already at {server.CurrentPlayers}");
                return;
            }

            // remove player from server, and update server status based on player count
            server.CurrentPlayers--;
            server.Status = server.CurrentPlayers >= server.MaxPlayers ? "full" : "available";
            Console.WriteLine($"User {userId} disconnected from {serverId}. Players: {server.CurrentPlayers}/{server.MaxPlayers} (status: {server.Status})");
        }
    }

    /// <summary>
    /// Allocates a specific game server identified by serverId for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the game server.</param>
    /// <param name="serverId">The unique identifier of the specific game server to allocate.</param>
    /// <returns>A <see cref="GameServer"/> instance representing the requested game server if available, or null if the server does not exist or is full.</returns>
    public GameServer? AllocateChosenServer(string userId, string serverId)
    {
        lock (lockObject)
        {
            var server = servers.FirstOrDefault(s => s.ServerId == serverId);

            // EO; if server doesnt exist
            if (server == null)
            {
                Console.WriteLine($"Server {serverId} not found for user {userId}");
                return null;
            }

            // EO; if server is full
            if (server.CurrentPlayers >= server.MaxPlayers)
            {
                Console.WriteLine($"Server {serverId} is full. Cannot allocate to user {userId}. Players: {server.CurrentPlayers}/{server.MaxPlayers}");
                return null;
            }

            // add player to server, and check if the filled swerver, if so change server status
            server.CurrentPlayers++;
            if (server.CurrentPlayers >= server.MaxPlayers)
            {
                server.Status = "full";
            }

            Console.WriteLine($"Allocated chosen server {server.ServerId} to user {userId}. Players: {server.CurrentPlayers}/{server.MaxPlayers}");
            return server;
        }
    }

    public async Task CheckServers()
    {
        if(kubernetesClient == null)
        {
            Console.WriteLine("KubernetesClient not set - cannot create server pods");
            return;
        }

        List<GameServer> serversToCheck = new List<GameServer>();

        bool serverNeeded = false;
        serversToCheck = servers;

        if (serversToCheck.Count == 0)
        {
            return;
        }
            
        var currentState = await kubernetesClient.AppsV1.ReadNamespacedDeploymentScaleAsync(gameServerDeploymentName, namespaceParameter, true);

        int totalServers = 0;
        int desiredReplicas = 0;
        
        if (currentState.Spec.Replicas != null)
        {
            int currentReplicas = currentState.Spec.Replicas.Value;
            totalServers = Math.Max(serversToCheck.Count, currentReplicas);
            desiredReplicas = currentReplicas;
        }

        List<GameServer> emptyGameServers = serversToCheck.Where(server => server.CurrentPlayers <= 0).ToList();
        
        //------------Scaling------------
            
        if (emptyGameServers.Count == 0 && totalServers < maxPods) 
        {
            //Create new server
            desiredReplicas = totalServers + 1;
            Console.WriteLine("SCALING - New server amount desired: " + desiredReplicas);
        }
        else if (emptyGameServers.Count > 1 && totalServers > 1)
        {
            //Close server
            desiredReplicas = totalServers - 1;
            Console.WriteLine("SCALING - Closing server, new desired: " + desiredReplicas);
        }
        else
        {
            Console.WriteLine("SCALING - Not needed");
            return;
        }

        //Apply desired scaling
        try
        {
            V1Scale newSpec = new V1Scale();
            newSpec.Spec.Replicas = desiredReplicas;

            await kubernetesClient.AppsV1.ReplaceNamespacedDeploymentScaleAsync(newSpec, gameServerDeploymentName, namespaceParameter);
            Console.WriteLine("SCALING - Scale completed, with size: " +  newSpec.Spec.Replicas.Value);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR! Scaling is jank and didn't work" + e);
            throw;
        }
    }
}