using SimpleTodo.Api.Services;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Unit tests for PromptTemplateService to verify correct retrieval of versioned prompt templates.
/// </summary>
public class PromptTemplateServiceTests
{
    private readonly PromptTemplateService _service;

    public PromptTemplateServiceTests()
    {
        _service = new PromptTemplateService();
    }

    [Fact]
    public async Task GetPromptTemplateAsync_StoryRootMerge_ReturnsValidTemplate()
    {
        // Act
        var template = await _service.GetPromptTemplateAsync("story_root_merge", "1.0");

        // Assert
        Assert.NotNull(template);
        Assert.Contains("Story Root", template);
        Assert.Contains("{current_story_root}", template);
        Assert.Contains("{user_input}", template);
    }

    [Fact]
    public async Task GetPromptTemplateAsync_StoryRootMerge_DefaultVersion_ReturnsValidTemplate()
    {
        // Act
        var template = await _service.GetPromptTemplateAsync("story_root_merge", "");

        // Assert
        Assert.NotNull(template);
        Assert.Contains("Story Root", template);
    }

    [Fact]
    public async Task GetPromptTemplateAsync_WorldStateMerge_ReturnsValidTemplate()
    {
        // Act
        var template = await _service.GetPromptTemplateAsync("world_state_merge", "1.0");

        // Assert
        Assert.NotNull(template);
        Assert.Contains("World State", template);
        Assert.Contains("{current_world_state}", template);
        Assert.Contains("{user_input}", template);
    }

    [Fact]
    public async Task GetPromptTemplateAsync_WorldStateMerge_DefaultVersion_ReturnsValidTemplate()
    {
        // Act
        var template = await _service.GetPromptTemplateAsync("world_state_merge", "");

        // Assert
        Assert.NotNull(template);
        Assert.Contains("World State", template);
    }

    [Fact]
    public async Task GetPromptTemplateAsync_UnknownTemplateName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetPromptTemplateAsync("unknown_template", "1.0"));
    }

    [Fact]
    public async Task GetPromptTemplateAsync_UnknownVersion_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetPromptTemplateAsync("story_root_merge", "2.0"));
    }

    [Fact]
    public async Task GetPromptTemplateAsync_CaseInsensitiveTemplateName_Works()
    {
        // Act
        var template1 = await _service.GetPromptTemplateAsync("STORY_ROOT_MERGE", "1.0");
        var template2 = await _service.GetPromptTemplateAsync("story_root_merge", "1.0");

        // Assert
        Assert.Equal(template1, template2);
    }
}
