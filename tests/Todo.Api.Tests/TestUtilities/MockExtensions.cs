using Moq;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Services;
using Todo.Api.Tests.Fixtures;
using Todo.Api.Tests.Mocks;
using Todo.Api.Tests.TestUtilities;

namespace Todo.Api.Tests.TestUtilities;

/// <summary>
/// Extension methods for common mocking patterns used in tests.
/// </summary>
public static class MockExtensions
{
    /// <summary>
    /// Creates a mock IBlobArtifactRepository with default behavior.
    /// </summary>
    public static Mock<IBlobArtifactRepository> CreateBlobArtifactRepositoryMock()
    {
        var mock = new Mock<IBlobArtifactRepository>();
        return mock;
    }

    /// <summary>
    /// Configures a mock IBlobArtifactRepository to return a specific blob content.
    /// </summary>
    public static Mock<IBlobArtifactRepository> SetupGetBlob(
        this Mock<IBlobArtifactRepository> mock,
        string blobPath,
        string? content)
    {
        mock.Setup(r => r.GetBlobAsync(blobPath))
            .ReturnsAsync(content);
        return mock;
    }

    /// <summary>
    /// Configures a mock IBlobArtifactRepository to verify that SaveBlobAsync was called.
    /// </summary>
    public static void VerifySaveBlob(
        this Mock<IBlobArtifactRepository> mock,
        string blobPath,
        Times times)
    {
        mock.Verify(r => r.SaveBlobAsync(blobPath, It.IsAny<string>()), times);
    }

    /// <summary>
    /// Configures a mock IBlobArtifactRepository to return true for BlobExistsAsync.
    /// </summary>
    public static Mock<IBlobArtifactRepository> SetupBlobExists(
        this Mock<IBlobArtifactRepository> mock,
        string blobPath,
        bool exists)
    {
        mock.Setup(r => r.BlobExistsAsync(blobPath))
            .ReturnsAsync(exists);
        return mock;
    }

    /// <summary>
    /// Configures a mock IBlobArtifactRepository to return a list of blob names for ListBlobsAsync.
    /// </summary>
    public static Mock<IBlobArtifactRepository> SetupListBlobs(
        this Mock<IBlobArtifactRepository> mock,
        string prefix,
        List<string> blobNames)
    {
        mock.Setup(r => r.ListBlobsAsync(prefix))
            .ReturnsAsync(blobNames);
        return mock;
    }

    /// <summary>
    /// Creates a mock IUserContextService that returns the test user ID.
    /// </summary>
    public static Mock<IUserContextService> CreateUserContextServiceMock()
    {
        var mock = new Mock<IUserContextService>();
        mock.Setup(s => s.GetCurrentUserId())
            .Returns(TestConstants.TestUserId);
        return mock;
    }

    /// <summary>
    /// Configures a mock IUserContextService to return a specific user ID.
    /// </summary>
    public static Mock<IUserContextService> SetupUserId(
        this Mock<IUserContextService> mock,
        string userId)
    {
        mock.Setup(s => s.GetCurrentUserId())
            .Returns(userId);
        return mock;
    }

    /// <summary>
    /// Creates a MockLlmService configured with default valid responses.
    /// </summary>
    public static MockLlmService CreateLlmServiceMock()
    {
        return new MockLlmService();
    }

    /// <summary>
    /// Creates a MockLlmService configured for a specific scenario (e.g., missing fields, malformed JSON).
    /// </summary>
    public static MockLlmService CreateLlmServiceMockForScenario(string scenario)
    {
        var mockService = new MockLlmService();
        
        switch (scenario.ToLowerInvariant())
        {
            case "story_root_missing_field":
                mockService.SetResponse("story_root", LlmResponseFixtures.StoryRootMissingRequiredField);
                break;
            case "story_root_extra_fields":
                mockService.SetResponse("story_root", LlmResponseFixtures.StoryRootWithExtraFields);
                break;
            case "story_root_malformed":
                mockService.SetResponse("story_root", LlmResponseFixtures.StoryRootMalformedJson);
                break;
            case "story_root_empty":
                mockService.SetResponse("story_root", LlmResponseFixtures.StoryRootEmpty);
                break;
            case "world_state_missing_field":
                mockService.SetResponse("world_state", LlmResponseFixtures.WorldStateMissingRequiredField);
                break;
            case "world_state_extra_fields":
                mockService.SetResponse("world_state", LlmResponseFixtures.WorldStateWithExtraFields);
                break;
            case "world_state_malformed":
                mockService.SetResponse("world_state", LlmResponseFixtures.WorldStateMalformedJson);
                break;
            case "world_state_empty":
                mockService.SetResponse("world_state", LlmResponseFixtures.WorldStateEmpty);
                break;
            case "null_response":
                mockService.SetResponse("story_root", LlmResponseFixtures.NullResponse);
                mockService.SetResponse("world_state", LlmResponseFixtures.NullResponse);
                break;
            case "empty_string_response":
                mockService.SetResponse("story_root", LlmResponseFixtures.EmptyStringResponse);
                mockService.SetResponse("world_state", LlmResponseFixtures.EmptyStringResponse);
                break;
        }
        
        return mockService;
    }
}
