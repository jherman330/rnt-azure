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
/// Comprehensive unit tests for WorldStateService covering all service methods,
/// LLM response parsing, schema validation, error handling, persistence, and provenance.
/// Tests are independent from StoryRootService tests to ensure service independence.
/// </summary>
public class WorldStateServiceTests
{
    private readonly Mock<IWorldStateRepository> _repositoryMock;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly Mock<IPromptTemplateService> _promptTemplateServiceMock;
    private readonly Mock<IUserContextService> _userContextServiceMock;
    private readonly WorldStateValidator _validator;
    private readonly WorldStateService _service;

    public WorldStateServiceTests()
    {
        _repositoryMock = new Mock<IWorldStateRepository>();
        _llmServiceMock = new Mock<ILlmService>();
        _promptTemplateServiceMock = new Mock<IPromptTemplateService>();
        _userContextServiceMock = TestUtilities.MockExtensions.CreateUserContextServiceMock();
        _validator = new WorldStateValidator();

        _service = new WorldStateService(
            _repositoryMock.Object,
            _llmServiceMock.Object,
            _promptTemplateServiceMock.Object,
            _userContextServiceMock.Object,
            _validator);

        // Setup default prompt template
        _promptTemplateServiceMock
            .Setup(s => s.GetPromptTemplateAsync("world_state_merge", "1.0"))
            .ReturnsAsync("Template with {current_world_state} and {user_input}");
    }

    #region GetCurrentWorldStateAsync Tests

    [Fact]
    public async Task GetCurrentWorldStateAsync_ReturnsCurrentVersion()
    {
        // Arrange
        var expectedWorldState = new WorldState
        {
            WorldStateId = "world-123",
            PhysicalLaws = "Gravity works normally",
            SocialStructures = "Democratic",
            HistoricalContext = "Modern era",
            MagicOrTechnology = "Technology"
        };

        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(expectedWorldState);

        // Act
        var result = await _service.GetCurrentWorldStateAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedWorldState.WorldStateId, result.WorldStateId);
        _repositoryMock.Verify(r => r.GetCurrentVersionAsync(TestConstants.TestUserId), Times.Once);
    }

    [Fact]
    public async Task GetCurrentWorldStateAsync_NoCurrentVersion_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync((WorldState?)null);

        // Act
        var result = await _service.GetCurrentWorldStateAsync();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetWorldStateVersionAsync Tests

    [Fact]
    public async Task GetWorldStateVersionAsync_ValidVersionId_ReturnsVersion()
    {
        // Arrange
        var versionId = TestConstants.TestVersionId;
        var expectedWorldState = new WorldState
        {
            WorldStateId = "world-123",
            PhysicalLaws = "Gravity works normally",
            SocialStructures = "Democratic",
            HistoricalContext = "Modern era",
            MagicOrTechnology = "Technology"
        };

        _repositoryMock
            .Setup(r => r.GetVersionAsync(TestConstants.TestUserId, versionId))
            .ReturnsAsync(expectedWorldState);

        // Act
        var result = await _service.GetWorldStateVersionAsync(versionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedWorldState.WorldStateId, result.WorldStateId);
        _repositoryMock.Verify(r => r.GetVersionAsync(TestConstants.TestUserId, versionId), Times.Once);
    }

    [Fact]
    public async Task GetWorldStateVersionAsync_EmptyVersionId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetWorldStateVersionAsync(""));
    }

    [Fact]
    public async Task GetWorldStateVersionAsync_NullVersionId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetWorldStateVersionAsync(null!));
    }

    #endregion

    #region ListWorldStateVersionsAsync Tests

    [Fact]
    public async Task ListWorldStateVersionsAsync_ReturnsVersionList()
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
        var result = await _service.ListWorldStateVersionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _repositoryMock.Verify(r => r.ListVersionsAsync(TestConstants.TestUserId), Times.Once);
    }

    #endregion

    #region ProposeWorldStateMergeAsync Tests - LLM Response Parsing (Mandatory Coverage)

    [Fact]
    public async Task ProposeWorldStateMergeAsync_ValidLLMResponse_ParsesSuccessfully()
    {
        // Arrange
        var rawInput = "Add more details about the physical laws";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidWorldState);

        // Act
        var result = await _service.ProposeWorldStateMergeAsync(rawInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("world-67890", result.WorldStateId);
        Assert.Contains("Gravity", result.PhysicalLaws);
        _llmServiceMock.Verify(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"), Times.Once);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_WithCurrentWorldState_IncludesInPrompt()
    {
        // Arrange
        var currentWorldState = new WorldState
        {
            WorldStateId = "world-existing",
            PhysicalLaws = "Standard physics",
            SocialStructures = "Feudal",
            HistoricalContext = "Medieval",
            MagicOrTechnology = "Magic"
        };
        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync(currentWorldState);

        var rawInput = "Update the physical laws";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidWorldState);

        // Act
        await _service.ProposeWorldStateMergeAsync(rawInput);

        // Assert
        _llmServiceMock.Verify(s => s.ProposeWorldStateMergeAsync(
            It.Is<string>(p => p.Contains("world-existing")), "1.0"), Times.Once);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_NoCurrentWorldState_UsesNullInPrompt()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetCurrentVersionAsync(TestConstants.TestUserId))
            .ReturnsAsync((WorldState?)null);

        var rawInput = "Create new world state";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidWorldState);

        // Act
        await _service.ProposeWorldStateMergeAsync(rawInput);

        // Assert
        _llmServiceMock.Verify(s => s.ProposeWorldStateMergeAsync(
            It.Is<string>(p => p.Contains("null")), "1.0"), Times.Once);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_MalformedJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.WorldStateMalformedJson);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeWorldStateMergeAsync(rawInput));
        Assert.Contains("Failed to parse LLM response as JSON", ex.Message);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_EmptyResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeWorldStateMergeAsync(rawInput));
        Assert.Contains("empty response", ex.Message);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_NullResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync((string?)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeWorldStateMergeAsync(rawInput));
        Assert.Contains("empty response", ex.Message);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_DeserializesToNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync("{}");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeWorldStateMergeAsync(rawInput));
        Assert.Contains("deserialized to null", ex.Message);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_LLMServiceThrows_PropagatesException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ThrowsAsync(new Exception("LLM service unavailable"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeWorldStateMergeAsync(rawInput));
        Assert.Contains("LLM service call failed", ex.Message);
    }

    #endregion

    #region ProposeWorldStateMergeAsync Tests - Schema Validation (Mandatory Coverage)

    [Fact]
    public async Task ProposeWorldStateMergeAsync_MissingRequiredField_ThrowsInvalidOperationException()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.WorldStateMissingRequiredField);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeWorldStateMergeAsync(rawInput));
        Assert.Contains("Proposal validation failed", ex.Message);
        Assert.Contains("PhysicalLaws is required", ex.Message);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_ExtraFields_StillValidates()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.WorldStateWithExtraFields);

        // Act
        var result = await _service.ProposeWorldStateMergeAsync(rawInput);

        // Assert - Extra fields should be ignored during deserialization, but required fields must be present
        Assert.NotNull(result);
        Assert.Equal("world-67890", result.WorldStateId);
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_EmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ProposeWorldStateMergeAsync(""));
    }

    [Fact]
    public async Task ProposeWorldStateMergeAsync_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ProposeWorldStateMergeAsync(null!));
    }

    #endregion

    #region CommitWorldStateVersionAsync Tests - Persistence and Provenance (Mandatory Coverage)

    [Fact]
    public async Task CommitWorldStateVersionAsync_ValidProposal_CreatesVersionWithProvenance()
    {
        // Arrange
        var proposal = new WorldState
        {
            WorldStateId = "world-new",
            PhysicalLaws = "Gravity works normally",
            SocialStructures = "Democratic",
            HistoricalContext = "Modern era",
            MagicOrTechnology = "Technology"
        };

        var currentVersion = MetadataFixtures.InitialVersion;
        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata> { currentVersion });
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<WorldState>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        var versionId = await _service.CommitWorldStateVersionAsync(proposal);

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
    public async Task CommitWorldStateVersionAsync_NoPriorVersion_SetsPriorVersionIdToNull()
    {
        // Arrange
        var proposal = new WorldState
        {
            WorldStateId = "world-new",
            PhysicalLaws = "Gravity works normally",
            SocialStructures = "Democratic",
            HistoricalContext = "Modern era",
            MagicOrTechnology = "Technology"
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata>());
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<WorldState>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        var versionId = await _service.CommitWorldStateVersionAsync(proposal);

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
    public async Task CommitWorldStateVersionAsync_InvalidProposal_ThrowsArgumentException()
    {
        // Arrange
        var invalidProposal = new WorldState
        {
            WorldStateId = "world-invalid",
            // Missing required fields
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CommitWorldStateVersionAsync(invalidProposal));
        Assert.Contains("Proposal validation failed", ex.Message);
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<WorldState>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task CommitWorldStateVersionAsync_NullProposal_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CommitWorldStateVersionAsync(null!));
    }

    [Fact]
    public async Task CommitWorldStateVersionAsync_SetsLlmAssistedFlagToTrue()
    {
        // Arrange
        var proposal = new WorldState
        {
            WorldStateId = "world-new",
            PhysicalLaws = "Gravity works normally",
            SocialStructures = "Democratic",
            HistoricalContext = "Modern era",
            MagicOrTechnology = "Technology"
        };

        _repositoryMock
            .Setup(r => r.ListVersionsAsync(TestConstants.TestUserId))
            .ReturnsAsync(new List<VersionMetadata>());
        _repositoryMock
            .Setup(r => r.SaveNewVersionAsync(
                It.IsAny<WorldState>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .ReturnsAsync(TestConstants.TestVersionId);

        // Act
        await _service.CommitWorldStateVersionAsync(proposal);

        // Assert - Verify llmAssisted is set to true
        _repositoryMock.Verify(r => r.SaveNewVersionAsync(
            It.IsAny<WorldState>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            true), Times.Once);
    }

    #endregion

    #region Error Handling and Fallback Tests (Mandatory Coverage)

    [Fact]
    public async Task ProposeWorldStateMergeAsync_InvalidJsonStructure_ThrowsWithContext()
    {
        // Arrange
        var rawInput = "Some input";
        _llmServiceMock
            .Setup(s => s.ProposeWorldStateMergeAsync(It.IsAny<string>(), "1.0"))
            .ReturnsAsync(LlmResponseFixtures.ValidJsonWrongStructure);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProposeWorldStateMergeAsync(rawInput));
        Assert.Contains("Proposal validation failed", ex.Message);
        // Error should include the response for debugging
        Assert.Contains("Response:", ex.Message);
    }

    #endregion
}
