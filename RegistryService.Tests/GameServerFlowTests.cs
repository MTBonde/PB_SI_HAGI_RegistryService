/*
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RegistryService.Models;

namespace RegistryService.Tests;

/// <summary>
/// Tests the complete game server lifecycle: registration, heartbeats, and updates.
/// </summary>
[TestClass]
public class GameServerFlowTests
{
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    [TestInitialize]
    public void Setup()
    {
        factory = new WebApplicationFactory<Program>();
        client = factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        client.Dispose();
        factory.Dispose();
    }

    [TestMethod]
    public async Task GameServer_CanRegisterAndSendHeartbeats()
    {
        // Arrange: simulate a game server starting up
        var registerRequest = new RegisterRequest
        {
            ServerId = "unreal-server-1",
            Host = "192.168.1.100",
            Port = 7777,
            MaxPlayers = 16
        };

        // Act: register the game server
        var registerResponse = await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/register", registerRequest);

        // Assert: registration successful
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        registerContent.Should().Contain("unreal-server-1");
        registerContent.Should().Contain("heartbeatInterval");


        // Act: send first heartbeat (server is empty)
        var heartbeat1 = new HeartbeatRequest
        {
            ServerId = "unreal-server-1",
            CurrentPlayers = 0
        };
        var heartbeat1Response = await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/heartbeat", heartbeat1);

        // Assert: heartbeat accepted
        heartbeat1Response.StatusCode.Should().Be(HttpStatusCode.OK);


        // Act: send second heartbeat (players joined)
        var heartbeat2 = new HeartbeatRequest
        {
            ServerId = "unreal-server-1",
            CurrentPlayers = 10
        };
        var heartbeat2Response = await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/heartbeat", heartbeat2);

        // Assert: heartbeat accepted
        heartbeat2Response.StatusCode.Should().Be(HttpStatusCode.OK);


        // Act: send third heartbeat (server is full)
        var heartbeat3 = new HeartbeatRequest
        {
            ServerId = "unreal-server-1",
            CurrentPlayers = 16
        };
        var heartbeat3Response = await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/heartbeat", heartbeat3);

        // Assert: heartbeat accepted
        heartbeat3Response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GameServer_CanUpdateItsConfiguration()
    {
        // Arrange: register initial server
        var initialRequest = new RegisterRequest
        {
            ServerId = "server-update-test",
            Host = "10.0.0.5",
            Port = 8000,
            MaxPlayers = 10
        };
        await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/register", initialRequest);

        // Act: server restarts with new configuration
        var updatedRequest = new RegisterRequest
        {
            ServerId = "server-update-test",
            Host = "10.0.0.5",
            Port = 9000,
            MaxPlayers = 32
        };
        var updateResponse = await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/register", updatedRequest);

        // Assert: update successful
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task MultipleGameServers_CanRegisterSimultaneously()
    {
        // Arrange: prepare multiple servers
        var server1 = new RegisterRequest { ServerId = "multi-1", Host = "192.168.1.1", Port = 7777, MaxPlayers = 16 };
        var server2 = new RegisterRequest { ServerId = "multi-2", Host = "192.168.1.2", Port = 7778, MaxPlayers = 20 };
        var server3 = new RegisterRequest { ServerId = "multi-3", Host = "192.168.1.3", Port = 7779, MaxPlayers = 24 };

        // Act: register all servers
        var task1 = client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/register", server1);
        var task2 = client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/register", server2);
        var task3 = client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/register", server3);

        var responses = await Task.WhenAll(task1, task2, task3);

        // Assert: all registrations successful
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }
}
*/
