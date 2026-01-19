using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services.StoryRoot;
using StoryRootModel = SimpleTodo.Api.Models.StoryRoot;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Domain-specific prompt builder for Story Root operations.
/// Gathers relevant context, selects appropriate templates, and produces structured inputs for the Prompt Factory.
/// </summary>
public class StoryRootPromptBuilder : IStoryRootPromptBuilder
{
    /// <summary>
    /// Prepares prompt inputs for Story Root merge operations.
    /// </summary>
    /// <param name="currentStoryRoot">Existing Story Root data, null if creating new</param>
    /// <param name="userInput">New input from the user</param>
    /// <returns>Template ID and variables ready for Prompt Factory</returns>
    public Task<PromptInput> PrepareStoryRootMergeAsync(StoryRootModel? currentStoryRoot, string userInput)
    {
        // Infer operation type from input
        var operationType = InferOperationType(currentStoryRoot);

        // Select appropriate template
        var templateId = StoryRootTemplateSelector.SelectTemplateId(operationType);

        // Prepare variables for template
        var variables = StoryRootVariableFormatter.PrepareMergeVariables(currentStoryRoot, userInput);

        return Task.FromResult(new PromptInput
        {
            TemplateId = templateId,
            Variables = variables
        });
    }

    /// <summary>
    /// Prepares prompt inputs for Story Root creation operations.
    /// </summary>
    /// <param name="userInput">Initial Story Root input from user</param>
    /// <returns>Template ID and variables ready for Prompt Factory</returns>
    public Task<PromptInput> PrepareStoryRootCreationAsync(string userInput)
    {
        // Select create template
        var templateId = StoryRootTemplateSelector.SelectTemplateId(StoryRootOperationType.Create);

        // Prepare variables for template
        var variables = StoryRootVariableFormatter.PrepareCreationVariables(userInput);

        return Task.FromResult(new PromptInput
        {
            TemplateId = templateId,
            Variables = variables
        });
    }

    /// <summary>
    /// Infers the operation type from the current Story Root presence.
    /// </summary>
    /// <param name="currentStoryRoot">Existing Story Root, null if creating new</param>
    /// <returns>Operation type (Create if null, Merge otherwise)</returns>
    private static StoryRootOperationType InferOperationType(StoryRootModel? currentStoryRoot)
    {
        return currentStoryRoot == null ? StoryRootOperationType.Create : StoryRootOperationType.Merge;
    }
}
