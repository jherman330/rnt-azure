using SimpleTodo.Api.Models;

namespace Todo.Api.Tests.Fixtures;

/// <summary>
/// Test fixtures for Story Root Prompt Builder testing, including sample Story Root data and user inputs.
/// All fixtures use deterministic data for reproducible tests.
/// </summary>
public static class StoryRootPromptBuilderFixtures
{
    /// <summary>
    /// Sample Story Root for testing merge operations.
    /// </summary>
    public static StoryRoot SampleStoryRoot => new()
    {
        StoryRootId = "test-story-root-123",
        Genre = "Science Fiction",
        Tone = "Serious and contemplative",
        ThematicPillars = "Technology ethics, Human connection, Identity",
        Notes = "Focus on near-future scenarios"
    };

    /// <summary>
    /// Another sample Story Root with different values.
    /// </summary>
    public static StoryRoot AlternativeStoryRoot => new()
    {
        StoryRootId = "test-story-root-456",
        Genre = "Fantasy",
        Tone = "Whimsical and adventurous",
        ThematicPillars = "Courage, Friendship, Magic",
        Notes = "Young adult audience"
    };

    /// <summary>
    /// Sample user input for merge operations.
    /// </summary>
    public const string SampleUserInput = "Add more emphasis on the environmental impact of new technology.";

    /// <summary>
    /// Another sample user input.
    /// </summary>
    public const string AlternativeUserInput = "The story should explore themes of artificial intelligence consciousness.";

    /// <summary>
    /// Sample user input for creation operations.
    /// </summary>
    public const string CreationUserInput = "I want to create a cyberpunk story about corporate espionage in a near-future megacity.";

    /// <summary>
    /// Expected template ID for merge operations.
    /// </summary>
    public const string ExpectedMergeTemplateId = "story-root-merge";

    /// <summary>
    /// Expected template ID for create operations.
    /// </summary>
    public const string ExpectedCreateTemplateId = "story-root-create";
}
