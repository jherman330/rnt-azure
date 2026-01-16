using Moq;
using SimpleTodo.Api.Models;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Services;
using SimpleTodo.Api.Validation;
using Todo.Api.Tests.Fixtures;
using Todo.Api.Tests.Mocks;
using Todo.Api.Tests.TestUtilities;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Comprehensive unit tests for StoryRootService covering all service methods,
/// LLM response parsing, schema validation, error handling, persistence, and provenance.
/// </summary>
public class StoryRootServiceTests
{
    private readonly Mock<IStoryRootRepository> _repositoryMock;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly Mock<IPromptTemplateService> _promptTemplateServiceMock;
    private readonly Mock<IUserContextService> _userContextServiceMock;
    private readonly StoryRootValidator _validator;
    private readonly StoryRootService _service;

    public StoryRootServiceTests()
    {
        _repositoryMock = new Mock<IStoryRootRepository>();
        _llmServiceMock = new Mock<ILlmService>();
        _promptTemplateServiceMock = new Mock<IPromptTemplateService>();
        _userContextServiceMock = TestUtilities.MockExtensions.CreateUserContextServiceMock();
        _validator = new StoryRootValidator();

        _service = new StoryRootService(
            _repositoryMock.Object,
            _llmServiceMock.Object,
            _promptTemplateServiceMock.Object,
            _userContextServiceMock.Object,
            _validator);

        // Setup default prompt template
        _promptTemplateServiceMock
            .Setup(s => s.GetPromptTemplateAsync("story_root_merge", "1.0"))
            .ReturnsAsync("Template with {current_story_root} and {user_input}");
    }

    #region GetCurrentStoryRootAsync Tests

    [Fact]
    public async Task GetCurrentStoryRootAsync_ReturnsCurrentVersion()
    {
        // Arrange
        var expectedStoryRoot = new StoryRoot
        {
            StoryRootId = "story-123",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(expectedStoryRoot);

        // Act
        var result = await _service.GetCurrentStoryRootAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStoryRoot.StoryRootId, result.StoryRootId);
        _repositoryMock.Verify(r => r.GetCurrentVersionAsync(TestConstants.TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetCurrentStoryRootAsync_NoCurrentVersion_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync((StoryRoot?)null);

        // Act
        var result = await _service.GetCurrentStoryRootAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetStoryRootVersionAsync Tests

    [Fact]
    public async Task GetStoryRootVersionAsync_ValidVersionId_ReturnsVersion()
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

        _repositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, versionId))
            .ReturnsAsync(expectedStoryRoot);

        // Act
        var result = await _service.GetStoryRootVersionAsync(versionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStoryRoot.StoryRootId, result.StoryRootId);
        _repositoryMock.Verify(r => r.GetVersionAsync(TestConstants.TestUserId, versionId), Times.Once);
    }

    [Fact]
    public async Task GetStoryRootVersionAsync_EmptyVersionId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetStoryRootVersionAsync(""));
    }

    [Fact]
    public async Task GetStoryRootVersionAsync_NullVersionId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetStoryRootVersionAsync(null!));
    }

    #endregion

    #region ListStoryRootVersionsAsync Tests

    [Fact]
    public async Task ListStoryRootVersionsAsync_ReturnsVersionList()
    {
        // Arrange
        var expectedVersions = new List<VersionMetadata>
        {
            MetadataFixtures.InitialVersion,
            MetadataFixtures.UpdatedVersion
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(expectedVersions);

        // Act
        var result = await _service.ListStoryRootVersionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _repositoryMock.Verify(r => r.ListVersionsAsync(TestConstants.TestUserId), Times.Once);
    }

    #endregion

    #region ProposeStoryRootMergeAsync Tests - LLM Response Parsing (Mandatory Coverage)

    [Fact]
    public async Task ProposeStoryRootMergeAsync_ValidLLMResponse_ParsesSuccessfully()
    {
        // Arrange
        var rawInput = "Add more details about the AI consciousness theme";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidStoryRoot);

        // Act
        var result = await _service.ProposeStoryRootMergeAsync(rawInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("story-12345", result.StoryRootId);
        Assert.Equal("Science Fiction", result.Genre);
        _llmServiceMock.Verify(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"), Times.Once);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_WithCurrentStoryRoot_IncludesInPrompt()
    {
        // Arrange
        var currentStoryRoot = new StoryRoot
        {
            StoryRootId = "story-existing",
            Genre = "Fantasy",
            Tone = "Light",
            ThematicPillars = "Adventure"
        };
        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(currentStoryRoot);

        var rawInput = "Update the genre";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidStoryRoot);

        // Act
        await _service.ProposeStoryRootMergeAsync(rawInput);

        // Assert
        _llmServiceMock.Verify(s => s.ProposeStoryRootMergeAsync(
            It.Is<string>(p => p.Contains("story-existing")), "1.0"), Times.Once);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_NoCurrentStoryRoot_UsesNullInPrompt()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync((StoryRoot?)null);

        var rawInput = "Create new story root";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidStoryRoot);

        // Act
        await _service.ProposeStoryRootMergeAsync(rawInput);

        // Assert
        _llmServiceMock.Verify(s => s.ProposeStoryRootMergeAsync(
            It.Is<string>(p => p.Contains("null")), "1.0"), Times.Once);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_MalformedJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.StoryRootMalformedJson);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeStoryRootMergeAsync(rawInput));
        Assert.Contains("Failed to parse LLM response as JSON", ex.Message);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_EmptyResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeStoryRootMergeAsync(rawInput));
        Assert.Contains("empty response", ex.Message);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_NullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync((string?)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeStoryRootMergeAsync(rawInput));
        Assert.Contains("empty response", ex.Message);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_DeserializesToNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync("{}");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeStoryRootMergeAsync(rawInput));
        Assert.Contains("deserialized to null", ex.Message);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_LLMServiceThrows_PropagatesException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ThrowsAsync(new Exception("LLM service unavailable"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeStoryRootMergeAsync(rawInput));
        Assert.Contains("LLM service call failed", ex.Message);
    }

    #endregion

    #region ProposeStoryRootMergeAsync Tests - Schema Validation (Mandatory Coverage)

    [Fact]
    public async Task ProposeStoryRootMergeAsync_MissingRequiredField_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.StoryRootMissingRequiredField);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeStoryRootMergeAsync(rawInput));
        Assert.Contains("Proposal validation failed", ex.Message);
        Assert.Contains("Genre is required", ex.Message);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_ExtraFields_StillValidates()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.StoryRootWithExtraFields);

        // Act
        var result = await _service.ProposeStoryRootMergeAsync(rawInput);

        // Assert - Extra fields should be ignored during deserialization, but required fields must be present
        Assert.NotNull(result);
        Assert.Equal("story-12345", result.StoryRootId);
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_EmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ProposeStoryRootMergeAsync(""));
    }

    [Fact]
    public async Task ProposeStoryRootMergeAsync_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ProposeStoryRootMergeAsync(null!));
    }

    #endregion

    #region CommitStoryRootVersionAsync Tests - Persistence and Provenance (Mandatory Coverage)

    [Fact]
    public async Task CommitStoryRootVersionAsync_ValidProposal_CreatesVersionWithProvenance()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-new",
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
        var versionId = await _service.CommitStoryRootVersionAsync(proposal);

        // Assert
        Assert.NotNull(versionId);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            proposal,
            It.Is<string?>(pv => pv == currentVersion.VersionId), // priorVersionId
            It.Is<string?>(sr => !string.IsNullOrEmpty(sr)), // sourceRequestId
            It.IsAny<string?>(), // environment
            It.Is<bool>(llm => llm == true) // llmAssisted = true
        ), Times.Once);
    }

    [Fact]
    public async Task CommitStoryRootVersionAsync_NoPriorVersion_SetsPriorVersionIdToNull()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-new",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata>());
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        var versionId = await _service.CommitStoryRootVersionAsync(proposal);

        // Assert
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            proposal,
            It.Is<string?>(pv => pv == null), // priorVersionId = null
            It.Is<string?>(sr => !string.IsNullOrEmpty(sr)), // sourceRequestId
            It.IsAny<string?>(), // environment
            It.Is<bool>(llm => llm == true) // llmAssisted = true
        ), Times.Once);
    }

    [Fact]
    public async Task CommitStoryRootVersionAsync_InvalidProposal_ThrowsArgumentException()
    {
        // Arrange
        var invalidProposal = new StoryRoot
        {
            StoryRootId = "story-invalid",
            // Missing required fields
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CommitStoryRootVersionAsync(invalidProposal));
        Assert.Contains("Proposal validation failed", ex.Message);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task CommitStoryRootVersionAsync_NullProposal_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CommitStoryRootVersionAsync(null!));
    }

    [Fact]
    public async Task CommitStoryRootVersionAsync_SetsLlmAssistedFlagToTrue()
    {
        // Arrange
        var proposal = new StoryRoot
        {
            StoryRootId = "story-new",
            Genre = "Science Fiction",
            Tone = "Dark",
            ThematicPillars = "AI consciousness"
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata>());
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<StoryRoot>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        await _service.CommitStoryRootVersionAsync(proposal);

        // Assert - Verify llmAssisted is set to true
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<StoryRoot>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            true), Times.Once);
    }

    #endregion

    #region Error Handling and Fallback Tests (Mandatory Coverage)

    [Fact]
    public async Task ProposeStoryRootMergeAsync_InvalidJsonStructure_ThrowsWithContext()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeStoryRootMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidJsonWrongStructure);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeStoryRootMergeAsync(rawInput));
        Assert.Contains("Proposal validation failed", ex.Message);
        // Error should include the response for debugging
        Assert.Contains("Response:", ex.Message);
    }

    #endregion
}
