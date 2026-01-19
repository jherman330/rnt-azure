using Moq;
using SimpleTodo.Api.Models;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Services;
using SimpleTodo.Api.Validation;
using Todo.Api.Tests.Fixtures;
using Todo.Api.Tests.TestUtilities;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Unit tests for StoryRootService commit operation with version conflict detection.
/// Tests optimistic concurrency control scenarios.
/// </summary>
public class StoryRootServiceCommitVersionConflictTests
{
    private readonly Mock<IStoryRootRepository> _repositoryMock;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly Mock<IStoryRootPromptBuilder> _storyRootPromptBuilderMock;
    private readonly Mock<IPromptFactory> _promptFactoryMock;
    private readonly Mock<IUserContextService> _userContextServiceMock;
    private readonly StoryRootValidator _validator;
    private readonly StoryRootService _service;

    public StoryRootServiceCommitVersionConflictTests()
    {
        _repositoryMock = new Mock<IStoryRootRepository>();
        _llmServiceMock = new Mock<ILlmService>();
        _storyRootPromptBuilderMock = new Mock<IStoryRootPromptBuilder>();
        _promptFactoryMock = new Mock<IPromptFactory>();
        _userContextServiceMock = TestUtilities.MockExtensions.CreateUserContextServiceMock();
        _validator = new StoryRootValidator();

        _service = new StoryRootService(
            _repositoryMock.Object,
            _llmServiceMock.Object,
            _storyRootPromptBuilderMock.Object,
            _promptFactoryMock.Object,
            _userContextServiceMock.Object,
            _validator);
    }

    #region Version Conflict Detection Tests

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_WithMatchingExpectedVersionId_Succeeds()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var expectedVersionId = TestConstants.TestVersionId;
        var currentVersion = new VersionMetadata
        {
            VersionId = expectedVersionId,
            UserId = TestConstants.TestUserId,
            Timestamp = DateTimeOffset.UtcNow
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync("new-version-id");

        // Act
        var versionId = await _service.CommitStoryRootVersionAsync(
            proposal,
            identity: null,
            expectedVersionId: expectedVersionId);

        // Assert
        Assert.NotNull(versionId);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Once);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_WithMismatchedExpectedVersionId_ThrowsVersionConflictException()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var expectedVersionId = TestConstants.TestVersionId;
        var actualCurrentVersionId = "different-version-id";
        var currentVersion = new VersionMetadata
        {
            VersionId = actualCurrentVersionId,
            UserId = TestConstants.TestUserId,
            Timestamp = DateTimeOffset.UtcNow
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<VersionConflictException>(() =>
            _service.CommitStoryRootVersionAsync(
                proposal,
                identity: null,
                expectedVersionId: expectedVersionId));

        Assert.NotNull(ex);
        Assert.Equal(expectedVersionId, ex.ExpectedVersionId);
        Assert.Equal(actualCurrentVersionId, ex.CurrentVersionId);
        Assert.Contains("Expected version", ex.Message);
        Assert.Contains("current version", ex.Message);

        // Verify that SaveNewVersionAsync was never called
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Never);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_WithExpectedVersionIdButNoCurrentVersion_ThrowsVersionConflictException()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var expectedVersionId = TestConstants.TestVersionId;

        // No current version exists
        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata>());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<VersionConflictException>(() =>
            _service.CommitStoryRootVersionAsync(
                proposal,
                identity: null,
                expectedVersionId: expectedVersionId));

        Assert.NotNull(ex);
        Assert.Equal(expectedVersionId, ex.ExpectedVersionId);
        Assert.Null(ex.CurrentVersionId);
        Assert.Contains("(none)", ex.Message);

        // Verify that SaveNewVersionAsync was never called
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Never);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_WithNullExpectedVersionId_ProceedsWithoutVersionCheck()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var currentVersion = MetadataFixtures.InitialVersion;
        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        var versionId = await _service.CommitStoryRootVersionAsync(
            proposal,
            identity: null,
            expectedVersionId: null);

        // Assert
        Assert.NotNull(versionId);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Once);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_WithEmptyExpectedVersionId_ProceedsWithoutVersionCheck()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var currentVersion = MetadataFixtures.InitialVersion;
        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        var versionId = await _service.CommitStoryRootVersionAsync(
            proposal,
            identity: null,
            expectedVersionId: string.Empty);

        // Assert
        Assert.NotNull(versionId);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Once);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_WithWhitespaceExpectedVersionId_ProceedsWithoutVersionCheck()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var currentVersion = MetadataFixtures.InitialVersion;
        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        var versionId = await _service.CommitStoryRootVersionAsync(
            proposal,
            identity: null,
            expectedVersionId: "   ");

        // Assert
        Assert.NotNull(versionId);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Once);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_WithoutExpectedVersionId_BackwardCompatible()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var currentVersion = MetadataFixtures.InitialVersion;
        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act - Call without expectedVersionId parameter (backward compatible)
        var versionId = await _service.CommitStoryRootVersionAsync(proposal);

        // Assert
        Assert.NotNull(versionId);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Once);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task CommitStoryRootVersionAsync_VersionConflict_DoesNotCallSaveNewVersion()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        var expectedVersionId = TestConstants.TestVersionId;
        var actualCurrentVersionId = "different-version-id";
        var currentVersion = new VersionMetadata
        {
            VersionId = actualCurrentVersionId,
            UserId = TestConstants.TestUserId,
            Timestamp = DateTimeOffset.UtcNow
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });

        // Act & Assert
        await Assert.ThrowsAsync<VersionConflictException>(() =>
            _service.CommitStoryRootVersionAsync(
                proposal,
                identity: null,
                expectedVersionId: expectedVersionId));

        // Verify SaveNewVersionAsync was never called
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        ), Times.Never);
    }

    #endregion
}
