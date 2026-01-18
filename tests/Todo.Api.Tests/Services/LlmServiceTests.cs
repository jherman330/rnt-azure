using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services;
using Todo.Api.Tests.Fixtures;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Unit tests for LLM service implementations (MockLlmService and OpenAILlmService).
/// </summary>
public class LlmServiceTests
{
    #region MockLlmService Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task MockLlmService_InvokeAsync_ReturnsDefaultResponse()
    {
        // Arrange
        var defaultResponse = LlmResponseFixtures.ValidStoryRoot;
        var service = new MockLlmService(defaultResponse);

        // Act
        var result = await service.InvokeAsync("test prompt");

        // Assert
        Assert.Equal(defaultResponse, result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task MockLlmService_InvokeAsync_WithSetDefaultResponse_ReturnsUpdatedResponse()
    {
        // Arrange
        var service = new MockLlmService("initial");
        service.SetDefaultResponse(LlmResponseFixtures.ValidWorldState);

        // Act
        var result = await service.InvokeAsync("test prompt");

        // Assert
        Assert.Equal(LlmResponseFixtures.ValidWorldState, result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task MockLlmService_InvokeAsync_ReturnsEmptyJson_WhenDefaultResponseEmpty()
    {
        // Arrange
        var service = new MockLlmService("{}");

        // Act
        var result = await service.InvokeAsync("test prompt");

        // Assert
        Assert.Equal("{}", result);
    }

    #endregion

    #region OpenAILlmService Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task OpenAILlmService_InvokeAsync_Success_ReturnsContent()
    {
        // Arrange
        var config = new LlmServiceConfiguration
        {
            ApiKey = "test-api-key",
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Model = "gpt-4"
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""choices"": [{
                        ""message"": {
                            ""content"": ""Test LLM response""
                        }
                    }]
                }", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = new Mock<ILogger<OpenAILlmService>>();
        var service = new OpenAILlmService(httpClientFactory.Object, Options.Create(config), logger.Object);

        // Act
        var result = await service.InvokeAsync("test prompt");

        // Assert
        Assert.Equal("Test LLM response", result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task OpenAILlmService_InvokeAsync_Unauthorized_ThrowsLlmAuthenticationException()
    {
        // Arrange
        var config = new LlmServiceConfiguration
        {
            ApiKey = "invalid-key",
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Model = "gpt-4"
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = new Mock<ILogger<OpenAILlmService>>();
        var service = new OpenAILlmService(httpClientFactory.Object, Options.Create(config), logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<LlmAuthenticationException>(() => service.InvokeAsync("test prompt"));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task OpenAILlmService_InvokeAsync_RateLimit_ThrowsLlmRateLimitException()
    {
        // Arrange
        var config = new LlmServiceConfiguration
        {
            ApiKey = "test-key",
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Model = "gpt-4"
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)429
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = new Mock<ILogger<OpenAILlmService>>();
        var service = new OpenAILlmService(httpClientFactory.Object, Options.Create(config), logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<LlmRateLimitException>(() => service.InvokeAsync("test prompt"));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task OpenAILlmService_InvokeAsync_NetworkError_ThrowsLlmNetworkException()
    {
        // Arrange
        var config = new LlmServiceConfiguration
        {
            ApiKey = "test-key",
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Model = "gpt-4"
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = new Mock<ILogger<OpenAILlmService>>();
        var service = new OpenAILlmService(httpClientFactory.Object, Options.Create(config), logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<LlmNetworkException>(() => service.InvokeAsync("test prompt"));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task OpenAILlmService_InvokeAsync_EmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var config = new LlmServiceConfiguration
        {
            ApiKey = "test-key",
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Model = "gpt-4"
        };

        var httpClientFactory = new Mock<IHttpClientFactory>();
        var logger = new Mock<ILogger<OpenAILlmService>>();
        var service = new OpenAILlmService(httpClientFactory.Object, Options.Create(config), logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.InvokeAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => service.InvokeAsync("   "));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task OpenAILlmService_InvokeAsync_MissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new LlmServiceConfiguration
        {
            ApiKey = "",
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Model = "gpt-4"
        };

        var httpClientFactory = new Mock<IHttpClientFactory>();
        var logger = new Mock<ILogger<OpenAILlmService>>();
        var service = new OpenAILlmService(httpClientFactory.Object, Options.Create(config), logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.InvokeAsync("test prompt"));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task OpenAILlmService_InvokeAsync_EmptyResponseContent_ReturnsEmptyString()
    {
        // Arrange
        var config = new LlmServiceConfiguration
        {
            ApiKey = "test-key",
            Endpoint = "https://api.openai.com/v1/chat/completions",
            Model = "gpt-4"
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""choices"": [{
                        ""message"": {
                            ""content"": """"
                        }
                    }]
                }", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = new Mock<ILogger<OpenAILlmService>>();
        var service = new OpenAILlmService(httpClientFactory.Object, Options.Create(config), logger.Object);

        // Act
        var result = await service.InvokeAsync("test prompt");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion
}
