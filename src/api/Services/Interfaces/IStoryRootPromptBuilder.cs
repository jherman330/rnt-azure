using SimpleTodo.Api.Models;
using StoryRootModel = SimpleTodo.Api.Models.StoryRoot;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Interface for Story Root domain-specific prompt builder.
/// Gathers relevant context, selects appropriate templates, and produces structured inputs for the Prompt Factory.
/// </summary>
public interface IStoryRootPromptBuilder
{
    /// <summary>
    /// Prepares prompt inputs for Story Root merge operations.
    /// </summary>
    /// <param name="currentStoryRoot">Existing Story Root data, null if creating new</param>
    /// <param name="userInput">New input from the user</param>
    /// <returns>Template ID and variables ready for Prompt Factory</returns>
    Task<PromptInput> PrepareStoryRootMergeAsync(StoryRootModel? currentStoryRoot, string userInput);

    /// <summary>
    /// Prepares prompt inputs for Story Root creation operations.
    /// </summary>
    /// <param name="userInput">Initial Story Root input from user</param>
    /// <returns>Template ID and variables ready for Prompt Factory</returns>
    Task<PromptInput> PrepareStoryRootCreationAsync(string userInput);
}
