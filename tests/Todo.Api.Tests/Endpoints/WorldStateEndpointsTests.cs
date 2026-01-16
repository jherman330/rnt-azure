using System.Net;
using System.Net.Http.Json;
using Moq;
using SimpleTodo.Api.Models;
using Todo.Api.Tests.Fixtures;
using Todo.Api.Tests.TestUtilities;
using Xunit;

namespace Todo.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for World State API endpoints.
/// Tests all HTTP methods, status codes, request/response serialization, and error handling.
/// These tests are completely independent from Story Root endpoint tests.
/// </summary>
public class WorldStateEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WorldStateEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/world-state Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentWorldState_Returns200_WhenWorldStateExists()
    {
        // Arrange
        var expectedWorldState = new WorldState
        {
            WorldStateId = "world-123",
            PhysicalLaws = "Gravity exists",
            SocialStructures = "Democratic",
            HistoricalContext = "Medieval",
            MagicOrTechnology = "Magic",
            Notes = "Test world"
        };

        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(expectedWorldState);

        // Act
        var response = await _client.GetAsync("/api/world-state");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var worldState = await response.Content.ReadFromJsonAsync<WorldState>();
        Assert.NotNull(worldState);
        Assert.Equal(expectedWorldState.WorldStateId, worldState.WorldStateId);
        Assert.Equal(expectedWorldState.PhysicalLaws, worldState.PhysicalLaws);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentWorldState_Returns404_WhenWorldStateDoesNotExist()
    {
        // Arrange
        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync((WorldState?)null);

        // Act
        var response = await _client.GetAsync("/api/world-state");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentWorldState_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ThrowsAsync(new Exception("Storage error"));

        // Act
        var response = await _client.GetAsync("/api/world-state");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
        Assert.Contains("Storage error", errorResponse.Error);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentWorldState_IncludesCorrelationId_WhenError()
    {
        // Arrange
        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await _client.GetAsync("/api/world-state");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotEmpty(correlationId);
    }

    #endregion

    #region GET /api/world-state/versions/{versionId} Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetWorldStateVersion_Returns200_WhenVersionExists()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        var expectedWorldState = new WorldState
        {
            WorldStateId = "world-123",
            PhysicalLaws = "Gravity exists",
            SocialStructures = "Democratic",
            HistoricalContext = "Medieval",
            MagicOrTechnology = "Magic"
        };

        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, versionId))
            .ReturnsAsync(expectedWorldState);

        // Act
        var response = await _client.GetAsync($"/api/world-state/versions/{versionId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var worldState = await response.Content.ReadFromJsonAsync<WorldState>();
        Assert.NotNull(worldState);
        Assert.Equal(expectedWorldState.WorldStateId, worldState.WorldStateId);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetWorldStateVersion_Returns404_WhenVersionDoesNotExist()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, versionId))
            .ReturnsAsync((WorldState?)null);

        // Act
        var response = await _client.GetAsync($"/api/world-state/versions/{versionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetWorldStateVersion_Returns400_WhenVersionIdIsEmpty()
    {
        // Arrange - Mock service to throw ArgumentException for empty versionId
        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, It.Is<string>(v => string.IsNullOrWhiteSpace(v))))
            .ThrowsAsync(new ArgumentException("Version ID cannot be null or empty"));

        // Act - Using URL-encoded space (ASP.NET Core will decode to space, then endpoint validates)
        var response = await _client.GetAsync("/api/world-state/versions/%20");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
    }

    #endregion

    #region GET /api/world-state/versions Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetWorldStateVersions_Returns200_WithVersionList()
    {
        // Arrange
        var expectedVersions = new List<VersionMetadata>
        {
            new VersionMetadata
            {
                VersionId = TestConstants.TestVersionId,
                UserId = TestConstants.TestUserId,
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        _factory.WorldStateRepositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(expectedVersions);

        // Act
        var response = await _client.GetAsync("/api/world-state/versions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var versions = await response.Content.ReadFromJsonAsync<List<VersionMetadata>>();
        Assert.NotNull(versions);
        Assert.Single(versions);
        Assert.Equal(expectedVersions[0].VersionId, versions[0].VersionId);
    }

    #endregion

    #region POST /api/world-state/propose-merge Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeWorldStateMerge_Returns200_WithProposalAndCurrent()
    {
        // Arrange
        var currentWorldState = new WorldState
        {
            WorldStateId = "world-123",
            PhysicalLaws = "Gravity exists",
            SocialStructures = "Democratic",
            HistoricalContext = "Medieval",
            MagicOrTechnology = "Magic"
        };

        _factory.WorldStateRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(currentWorldState);

        _factory.LlmServiceMock.SetResponse("world_state", LlmResponseFixtures.ValidWorldState);

        var request = new ProposeWorldStateMergeRequest { RawInput = "Add more magic" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/world-state/propose-merge", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var proposalResponse = await response.Content.ReadFromJsonAsync<ProposalResponse<WorldState>>();
        Assert.NotNull(proposalResponse);
        Assert.NotNull(proposalResponse.Proposal);
        Assert.NotNull(proposalResponse.Current);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeWorldStateMerge_Returns400_WhenRawInputIsEmpty()
    {
        // Arrange
        var request = new ProposeWorldStateMergeRequest { RawInput = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/world-state/propose-merge", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
        Assert.Contains("raw_input", errorResponse.Error);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeWorldStateMerge_Returns400_WhenRequestIsNull()
    {
        // Act
        var response = await _client.PostAsync("/api/world-state/propose-merge", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeWorldStateMerge_Returns500_WhenLlmServiceFails()
    {
        // Arrange
        _factory.LlmServiceMock.SetResponse("world_state", LlmResponseFixtures.WorldStateMalformedJson);

        var request = new ProposeWorldStateMergeRequest { RawInput = "Test input" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/world-state/propose-merge", request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
    }

    #endregion

    #region POST /api/world-state/commit Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitWorldState_Returns200_WithVersionId()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        var worldState = new WorldState
        {
            WorldStateId = "world-123",
            PhysicalLaws = "Gravity exists",
            SocialStructures = "Democratic",
            HistoricalContext = "Medieval",
            MagicOrTechnology = "Magic",
            Notes = "Test world"
        };

        // Mock ListVersionsAsync (called by CommitWorldStateVersionAsync)
        _factory.WorldStateRepositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata>());

        _factory.WorldStateRepositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<WorldState>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(versionId);

        var request = new CommitWorldStateRequest { WorldState = worldState };

        // Act
        var response = await _client.PostAsJsonAsync("/api/world-state/commit", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var commitResponse = await response.Content.ReadFromJsonAsync<CommitResponse<WorldState>>();
        Assert.NotNull(commitResponse);
        Assert.Equal(versionId, commitResponse.VersionId);
        Assert.NotNull(commitResponse.Artifact);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitWorldState_Returns400_WhenWorldStateIsNull()
    {
        // Arrange
        var request = new CommitWorldStateRequest { WorldState = null! };

        // Act
        var response = await _client.PostAsJsonAsync("/api/world-state/commit", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitWorldState_IncludesCorrelationId_InCommit()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        var worldState = new WorldState
        {
            WorldStateId = "world-123",
            PhysicalLaws = "Gravity exists",
            SocialStructures = "Democratic",
            HistoricalContext = "Medieval",
            MagicOrTechnology = "Magic"
        };

        string? capturedCorrelationId = null;

        _factory.WorldStateRepositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<WorldState>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .Callback<WorldState, string?, string?, string?, bool>((ws, pvid, srid, env, llm) =>
            {
                capturedCorrelationId = srid;
            })
            .ReturnsAsync(versionId);

        var request = new CommitWorldStateRequest { WorldState = worldState };

        // Act
        var response = await _client.PostAsJsonAsync("/api/world-state/commit", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotNull(capturedCorrelationId);
    }

    #endregion
}
