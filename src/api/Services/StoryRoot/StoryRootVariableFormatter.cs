using SimpleTodo.Api.Models;

namespace SimpleTodo.Api.Services.StoryRoot;

/// <summary>
/// Helper class for formatting Story Root domain data into template-ready variables.
/// Transforms domain models and user input into key-value dictionaries for template substitution.
/// </summary>
internal static class StoryRootVariableFormatter
{
    /// <summary>
    /// Prepares variables dictionary for Story Root merge operation.
    /// </summary>
    /// <param name="currentStoryRoot">Existing Story Root, may be null</param>
    /// <param name="userInput">New user input to merge</param>
    /// <returns>Dictionary of variable names to values for template substitution</returns>
    public static Dictionary<string, string> PrepareMergeVariables(Models.StoryRoot? currentStoryRoot, string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            throw new InvalidUserInputException("User input cannot be null or empty");
        }

        return new Dictionary<string, string>
        {
            ["current_story_root"] = StoryRootContextGatherer.FormatStoryRootForTemplate(currentStoryRoot),
            ["user_input"] = userInput
        };
    }

    /// <summary>
    /// Prepares variables dictionary for Story Root creation operation.
    /// </summary>
    /// <param name="userInput">Initial user input for creation</param>
    /// <returns>Dictionary of variable names to values for template substitution</returns>
    public static Dictionary<string, string> PrepareCreationVariables(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            throw new InvalidUserInputException("User input cannot be null or empty");
        }

        return new Dictionary<string, string>
        {
            ["user_input"] = userInput
        };
    }
}
