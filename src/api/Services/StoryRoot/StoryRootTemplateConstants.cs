namespace SimpleTodo.Api.Services.StoryRoot;

/// <summary>
/// Centralized template ID constants for Story Root operations.
/// Template IDs use hyphenated naming convention (e.g., "story-root-merge").
/// </summary>
internal static class StoryRootTemplateConstants
{
    /// <summary>
    /// Template ID for creating a new Story Root.
    /// Maps to: Templates/StoryRootCreate.txt
    /// </summary>
    public const string StoryRootCreate = "story-root-create";

    /// <summary>
    /// Template ID for merging new input into an existing Story Root.
    /// Maps to: Templates/StoryRootMerge.txt
    /// </summary>
    public const string StoryRootMerge = "story-root-merge";

    /// <summary>
    /// Template ID for updating an existing Story Root.
    /// Maps to: Templates/StoryRootUpdate.txt (if needed in future)
    /// </summary>
    public const string StoryRootUpdate = "story-root-update";
}
