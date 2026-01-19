using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services;
using SimpleTodo.Api.Services.Templates;
using SimpleTodo.Api.Services.Templating;
using Todo.Api.Tests.Fixtures;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Integration tests for StoryRootPromptBuilder with real PromptFactory and template infrastructure.
/// Verifies end-to-end integration with template loading and variable substitution.
/// </summary>
public class StoryRootPromptBuilderIntegrationTests
{
    private readonly StoryRootPromptBuilder _builder;
    private readonly IPromptFactory _promptFactory;

    public StoryRootPromptBuilderIntegrationTests()
    {
        _builder = new StoryRootPromptBuilder();
        
        // Use real template loader and variable substitutor for integration testing
        var templateLoader = new EmbeddedTemplateLoader();
        var variableSubstitutor = new SimpleVariableSubstitutor();
        _promptFactory = new PromptFactory(templateLoader, variableSubstitutor);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_WithPromptFactory_ProducesValidPrompt()
    {
        // Arrange
        var currentStoryRoot = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var promptInput = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);
        var assembledPrompt = await _promptFactory.AssemblePromptAsync(promptInput.TemplateId, promptInput.Variables);

        // Assert
        Assert.NotNull(promptInput);
        Assert.NotNull(assembledPrompt);
        Assert.NotEmpty(assembledPrompt);
        
        // Verify the prompt contains user input
        Assert.Contains(userInput, assembledPrompt);
        
        // Verify the prompt contains Story Root content
        Assert.Contains("Science Fiction", assembledPrompt);
        Assert.Contains("test-story-root-123", assembledPrompt);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootCreationAsync_WithPromptFactory_ProducesValidPrompt()
    {
        // Arrange
        var userInput = StoryRootPromptBuilderFixtures.CreationUserInput;

        // Act
        var promptInput = await _builder.PrepareStoryRootCreationAsync(userInput);
        var assembledPrompt = await _promptFactory.AssemblePromptAsync(promptInput.TemplateId, promptInput.Variables);

        // Assert
        Assert.NotNull(promptInput);
        Assert.NotNull(assembledPrompt);
        Assert.NotEmpty(assembledPrompt);
        
        // Verify the prompt contains user input
        Assert.Contains(userInput, assembledPrompt);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_WithNullStoryRoot_ProducesValidPromptWithNull()
    {
        // Arrange
        StoryRoot? currentStoryRoot = null;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var promptInput = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);
        var assembledPrompt = await _promptFactory.AssemblePromptAsync(promptInput.TemplateId, promptInput.Variables);

        // Assert
        Assert.NotNull(promptInput);
        Assert.NotNull(assembledPrompt);
        Assert.NotEmpty(assembledPrompt);
        
        // Verify the prompt contains user input
        Assert.Contains(userInput, assembledPrompt);
        
        // Verify the prompt contains "null" for current story root
        Assert.Contains("null", assembledPrompt);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_DifferentStoryRoots_ProduceDifferentPrompts()
    {
        // Arrange
        var storyRoot1 = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var storyRoot2 = StoryRootPromptBuilderFixtures.AlternativeStoryRoot;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var promptInput1 = await _builder.PrepareStoryRootMergeAsync(storyRoot1, userInput);
        var assembledPrompt1 = await _promptFactory.AssemblePromptAsync(promptInput1.TemplateId, promptInput1.Variables);
        
        var promptInput2 = await _builder.PrepareStoryRootMergeAsync(storyRoot2, userInput);
        var assembledPrompt2 = await _promptFactory.AssemblePromptAsync(promptInput2.TemplateId, promptInput2.Variables);

        // Assert
        Assert.NotEqual(assembledPrompt1, assembledPrompt2);
        Assert.Contains("Science Fiction", assembledPrompt1);
        Assert.Contains("Fantasy", assembledPrompt2);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task PrepareStoryRootMergeAsync_DeterministicIntegration_ProducesIdenticalPromptsForSameInput()
    {
        // Arrange
        var currentStoryRoot = StoryRootPromptBuilderFixtures.SampleStoryRoot;
        var userInput = StoryRootPromptBuilderFixtures.SampleUserInput;

        // Act
        var promptInput1 = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);
        var assembledPrompt1 = await _promptFactory.AssemblePromptAsync(promptInput1.TemplateId, promptInput1.Variables);
        
        var promptInput2 = await _builder.PrepareStoryRootMergeAsync(currentStoryRoot, userInput);
        var assembledPrompt2 = await _promptFactory.AssemblePromptAsync(promptInput2.TemplateId, promptInput2.Variables);

        // Assert
        Assert.Equal(assembledPrompt1, assembledPrompt2);
    }
}
