using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Moq;
using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services;
using Todo.Api.Tests.Fixtures;
using Todo.Api.Tests.TestUtilities;
using Xunit;

namespace Todo.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for Story Root API endpoints.
/// Tests all HTTP methods, status codes, request/response serialization, and error handling.
/// </summary>
public class StoryRootEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public StoryRootEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/story-root Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentStoryRoot_Returns200_WhenStoryRootExists()
    {
        // Arrange
        var expectedStoryRoot = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness",
            Notes = "Test story"
        };

        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(expectedStoryRoot);

        // Act
        var response = await _client.GetAsync("/api/story-root");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var storyRoot = await response.Content.ReadFromJsonAsync<StoryRoot>();
        Assert.NotNull(storyRoot);
        Assert.Equal(expectedStoryRoot.StoryRootId, storyRoot.StoryRootId);
        Assert.Equal(expectedStoryRoot.Genre, storyRoot.Genre);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentStoryRoot_Returns200_WithEmptyDefault_WhenStoryRootDoesNotExist()
    {
        // Arrange
        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync((StoryRoot?)null);

        // Act
        var response = await _client.GetAsync("/api/story-root");

        // Assert
        // Missing story root is treated as valid empty initial state (return default object, no 404)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var storyRoot = await response.Content.ReadFromJsonAsync<StoryRoot>();
        Assert.NotNull(storyRoot);
        Assert.Empty(storyRoot.StoryRootId);
        Assert.Empty(storyRoot.Genre);
        Assert.Empty(storyRoot.Tone);
        Assert.Empty(storyRoot.ThematicPillars);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentStoryRoot_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ThrowsAsync(new Exception("Storage error"));

        // Act
        var response = await _client.GetAsync("/api/story-root");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
        Assert.Contains("Storage error", errorResponse.Error);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetCurrentStoryRoot_IncludesCorrelationId_WhenError()
    {
        // Arrange
        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await _client.GetAsync("/api/story-root");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotEmpty(correlationId);
    }

    #endregion

    #region GET /api/story-root/versions/{versionId} Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetStoryRootVersion_Returns200_WhenVersionExists()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        var expectedStoryRoot = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, versionId))
            .ReturnsAsync(expectedStoryRoot);

        // Act
        var response = await _client.GetAsync($"/api/story-root/versions/{versionId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var storyRoot = await response.Content.ReadFromJsonAsync<StoryRoot>();
        Assert.NotNull(storyRoot);
        Assert.Equal(expectedStoryRoot.StoryRootId, storyRoot.StoryRootId);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetStoryRootVersion_Returns404_WhenVersionDoesNotExist()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, versionId))
            .ReturnsAsync((StoryRoot?)null);

        // Act
        var response = await _client.GetAsync($"/api/story-root/versions/{versionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetStoryRootVersion_Returns400_WhenVersionIdIsEmpty()
    {
        // Arrange - Mock service to throw ArgumentException for empty versionId
        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, It.Is<string>(v => string.IsNullOrWhiteSpace(v))))
            .ThrowsAsync(new ArgumentException("Version ID cannot be null or empty"));

        // Act - Using empty string in URL (ASP.NET Core will pass empty string)
        var response = await _client.GetAsync("/api/story-root/versions/%20");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
    }

    #endregion

    #region GET /api/story-root/versions Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task GetStoryRootVersions_Returns200_WithVersionList()
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

        _factory.StoryRootRepositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(expectedVersions);

        // Act
        var response = await _client.GetAsync("/api/story-root/versions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var versions = await response.Content.ReadFromJsonAsync<List<VersionMetadata>>();
        Assert.NotNull(versions);
        Assert.Single(versions);
        Assert.Equal(expectedVersions[0].VersionId, versions[0].VersionId);
    }

    #endregion

    #region POST /api/story-root/propose-merge Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeStoryRootMerge_Returns200_WithProposalAndCurrent()
    {
        // Arrange
        var currentStoryRoot = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var proposalStoryRoot = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark and Gritty",
            ThematicPillars = "AI consciousness, Human connection"
        };

        _factory.StoryRootRepositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(currentStoryRoot);

        _factory.LlmServiceMock.SetResponse("story_root", LlmResponseFixtures.ValidStoryRoot);

        var request = new ProposeStoryRootMergeRequest { RawInput = "Update the tone to be darker" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-root/propose-merge", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var proposalResponse = await response.Content.ReadFromJsonAsync<ProposalResponse<StoryRoot>>();
        Assert.NotNull(proposalResponse);
        Assert.NotNull(proposalResponse.Proposal);
        Assert.NotNull(proposalResponse.Current);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeStoryRootMerge_Returns400_WhenRawInputIsEmpty()
    {
        // Arrange
        var request = new ProposeStoryRootMergeRequest { RawInput = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-root/propose-merge", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
        Assert.Contains("raw_input", errorResponse.Error);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeStoryRootMerge_Returns400_WhenRequestIsNull()
    {
        // Act
        var response = await _client.PostAsync("/api/story-root/propose-merge", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task ProposeStoryRootMerge_Returns500_WhenLlmServiceFails()
    {
        // Arrange
        _factory.LlmServiceMock.SetResponse("story_root", LlmResponseFixtures.StoryRootMalformedJson);

        var request = new ProposeStoryRootMergeRequest { RawInput = "Test input" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-root/propose-merge", request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
    }

    #endregion

    #region POST /api/story-root/commit Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRoot_Returns200_WithVersionId()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        var storyRoot = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness",
            Notes = "Test story"
        };

        _factory.StoryRootRepositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(versionId);

        var request = new CommitStoryRootRequest { StoryRoot = storyRoot };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-root/commit", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var commitResponse = await response.Content.ReadFromJsonAsync<CommitResponse<StoryRoot>>();
        Assert.NotNull(commitResponse);
        Assert.Equal(versionId, commitResponse.VersionId);
        Assert.NotNull(commitResponse.Artifact);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRoot_Returns400_WhenStoryRootIsNull()
    {
        // Arrange
        var request = new CommitStoryRootRequest { StoryRoot = null! };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-root/commit", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(errorResponse);
        Assert.NotEmpty(errorResponse.CorrelationId);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRoot_IncludesCorrelationId_InCommit()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        var storyRoot = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        string? capturedCorrelationId = null;

        _factory.StoryRootRepositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .Callback<StoryRoot, string?, string?, string?, bool>((sr, pvid, srid, env, llm) =>
            {
                capturedCorrelationId = srid;
            })
            .ReturnsAsync(versionId);

        var request = new CommitStoryRootRequest { StoryRoot = storyRoot };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-root/commit", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.NotNull(capturedCorrelationId);
    }

    #endregion
}
