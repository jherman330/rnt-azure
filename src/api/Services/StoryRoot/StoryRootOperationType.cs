namespace SimpleTodo.Api.Services.StoryRoot;

/// <summary>
/// Internal enumeration for Story Root operation types.
/// Used for template selection logic and decision-making within the prompt builder.
/// </summary>
internal enum StoryRootOperationType
{
    /// <summary>
    /// Create a new Story Root from initial user input.
    /// </summary>
    Create,

    /// <summary>
    /// Merge new input into an existing Story Root.
    /// </summary>
    Merge,

    /// <summary>
    /// Update an existing Story Root with new information.
    /// </summary>
    Update
}
