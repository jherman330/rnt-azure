namespace SimpleTodo.Api.Services.StoryRoot;

/// <summary>
/// Helper class for selecting appropriate template IDs based on Story Root operation type.
/// Maps domain operations to template identifiers.
/// </summary>
internal static class StoryRootTemplateSelector
{
    /// <summary>
    /// Selects the appropriate template ID based on operation type.
    /// </summary>
    /// <param name="operationType">The Story Root operation type</param>
    /// <returns>Template ID string for the operation</returns>
    /// <exception cref="UnsupportedStoryRootOperationException">Thrown if operation type is not supported</exception>
    public static string SelectTemplateId(StoryRootOperationType operationType)
    {
        return operationType switch
        {
            StoryRootOperationType.Create => StoryRootTemplateConstants.StoryRootCreate,
            StoryRootOperationType.Merge => StoryRootTemplateConstants.StoryRootMerge,
            StoryRootOperationType.Update => StoryRootTemplateConstants.StoryRootUpdate,
            _ => throw new UnsupportedStoryRootOperationException(operationType.ToString())
        };
    }
}
