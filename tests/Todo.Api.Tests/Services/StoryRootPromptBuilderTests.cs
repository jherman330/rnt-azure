using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services;
using SimpleTodo.Api.Services.StoryRoot;
using Todo.Api.Tests.Fixtures;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Unit tests for StoryRootPromptBuilder to verify prompt preparation for Story Root operations.
/// </summary>
public class StoryRootPromptBuilderTests
{
    private readonly StoryRootPromptBuilder _builder;

    public StoryRootPromptBuilderTests()
    {
        _builder = new StoryRootPromptBuilder();
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_WithExistingStoryRoot_ReturnsMergePromptInput()
    {
        // Arrange
        var currentStoryRoot = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var result = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StoryRootPromptBuilderFixtures.ExpectedMergeTemplateId, result.TemplateId);
        Assert.NotNull(result.Variables);
        Assert.True(result.Variables.ContainsKey("current_story_root"));
        Assert.True(result.Variables.ContainsKey("user_input"));
        Assert.Equal(userInput, result.Variables["user_input"]);
        Assert.Contains("test-story-root-123", result.Variables["current_story_root"]);
        Assert.Contains("Science Fiction", result.Variables["current_story_root"]);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_WithNullStoryRoot_ReturnsMergePromptInput()
    {
        // Arrange
        StoryRoot? currentStoryRoot = null;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var result = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StoryRootPromptBuilderFixtures.ExpectedMergeTemplateId, result.TemplateId);
        Assert.NotNull(result.Variables);
        Assert.True(result.Variables.ContainsKey("current_story_root"));
        Assert.True(result.Variables.ContainsKey("user_input"));
        Assert.Equal("null", result.Variables["current_story_root"]);
        Assert.Equal(userInput, result.Variables["user_input"]);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootCreationAsync_WithUserInput_ReturnsCreatePromptInput()
    {
        // Arrange
        var userInput = StoryRootPromptBuilderFixtures.CreationUserInput;

        // Act
        var result = await _builder.PrepareStoryRootCreationAsync(userInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StoryRootPromptBuilderFixtures.ExpectedCreateTemplateId, result.TemplateId);
        Assert.NotNull(result.Variables);
        Assert.True(result.Variables.ContainsKey("user_input"));
        Assert.Equal(userInput, result.Variables["user_input"]);
        Assert.False(result.Variables.ContainsKey("current_story_root"));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_WithEmptyUserInput_ThrowsException()
    {
        // Arrange
        var currentStoryRoot = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var userInput = "";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidUserInputException>(() =>
            _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_WithWhitespaceUserInput_ThrowsException()
    {
        // Arrange
        var currentStoryRoot = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var userInput = "   ";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidUserInputException>(() =>
            _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootCreationAsync_WithEmptyUserInput_ThrowsException()
    {
        // Arrange
        var userInput = "";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidUserInputException>(() =>
            _builder.PrepareStoryRootCreationAsync(userInput));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_DeterministicOutput_ReturnsIdenticalResultsForSameInput()
    {
        // Arrange
        var currentStoryRoot = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var result1 = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);
        var result2 = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);

        // Assert
        Assert.Equal(result1.TemplateId, result2.TemplateId);
        Assert.Equal(result1.Variables.Count, result2.Variables.Count);
        Assert.Equal(result1.Variables["current_story_root"], result2.Variables["current_story_root"]);
        Assert.Equal(result1.Variables["user_input"], result2.Variables["user_input"]);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootCreationAsync_DeterministicOutput_ReturnsIdenticalResultsForSameInput()
    {
        // Arrange
        var userInput = StoryRootPromptBuilderFixtures.CreationUserInput;

        // Act
        var result1 = await _builder.PrepareStoryRootCreationAsync(userInput);
        var result2 = await _builder.PrepareStoryRootCreationAsync(userInput);

        // Assert
        Assert.Equal(result1.TemplateId, result2.TemplateId);
        Assert.Equal(result1.Variables.Count, result2.Variables.Count);
        Assert.Equal(result1.Variables["user_input"], result2.Variables["user_input"]);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_WithDifferentStoryRoots_ReturnsDifferentCurrentStoryRootValues()
    {
        // Arrange
        var storyRoot1 = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var storyRoot2 = StoryRootPromptBuilderFixtures.AlternativeStoryRoot;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var result1 = await _builder.PrepareStoryRootMergeAsync(storyRoot1, userInput);
        var result2 = await _builder.PrepareStoryRootMergeAsync(storyRoot2, userInput);

        // Assert
        Assert.NotEqual(result1.Variables["current_story_root"], result2.Variables["current_story_root"]);
        Assert.Contains("Science Fiction", result1.Variables["current_story_root"]);
        Assert.Contains("Fantasy", result2.Variables["current_story_root"]);
    }
}
