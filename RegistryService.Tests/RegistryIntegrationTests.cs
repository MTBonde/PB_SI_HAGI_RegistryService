using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RegistryService.Models;

namespace RegistryService.Tests;

[TestClass]
public class RegistryIntegrationTests
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
    public async Task RegisterServer_ReturnsOk()
    {
        // Arrange
        var request = new RegisterRequest
        {
            ServerId = "test-1",
            Host = "localhost",
            Port = 7777,
            MaxPlayers = 10
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task Heartbeat_ReturnsOk()
    {
        // Arrange
        var heartbeat = new HeartbeatRequest
        {
            ServerId = "test-2",
            CurrentPlayers = 5
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/{ApiVersion.Route}/registry/heartbeat", heartbeat);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task Health_ReturnsHealthy()
    {
        // Arrange

        // Act
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("healthy");
    }
}
