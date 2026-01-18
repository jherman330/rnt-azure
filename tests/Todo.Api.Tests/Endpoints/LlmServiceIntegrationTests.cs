using Microsoft.Extensions.DependencyInjection;
using SimpleTodo.Api.Services;
using Todo.Api.Tests.Endpoints;
using Xunit;

namespace Todo.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for LLM service through the application's dependency injection container.
/// </summary>
public class LlmServiceIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public LlmServiceIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void ResolveILlmService_ReturnsMockLlmService()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var serviceProvider = _factory.Services;

        // Act
        var llmService = serviceProvider.GetRequiredService<ILlmService>();

        // Assert
        Assert.NotNull(llmService);
        // In test environment, should be MockLlmService (not OpenAILlmService)
        Assert.IsType<Todo.Api.Tests.Mocks.MockLlmService>(llmService);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task InvokeAsync_ThroughDI_ReturnsConfiguredResponse()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        var llmService = serviceProvider.GetRequiredService<ILlmService>();
        var mockService = _factory.LlmServiceMock;

        // Set a specific response
        mockService.SetResponse("default", "Test response from mock");

        // Act
        var result = await llmService.InvokeAsync("test prompt");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Test response", result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task InvokeAsync_MultipleCalls_ReturnsConsistentResponse()
    {
        // Arrange
        var serviceProvider = _factory.Services;
        var llmService = serviceProvider.GetRequiredService<ILlmService>();
        var mockService = _factory.LlmServiceMock;

        mockService.SetResponse("default", "Consistent response");

        // Act
        var result1 = await llmService.InvokeAsync("prompt 1");
        var result2 = await llmService.InvokeAsync("prompt 2");

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal("Consistent response", result1);
    }
}
