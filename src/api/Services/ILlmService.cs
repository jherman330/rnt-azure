namespace SimpleTodo.Api.Services;

/// <summary>
/// Interface for LLM service interactions.
/// This interface enables mocking of LLM service calls for testing purposes,
/// ensuring zero live LLM calls in test scenarios.
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Generates a Story Root merge proposal from raw input using the specified prompt template version.
    /// Returns raw JSON/string response from the LLM without parsing or validation.
    /// </summary>
    /// <param name="rawInput">The raw user input to merge into the Story Root</param>
    /// <param name="promptVersion">Version identifier for the prompt template to use</param>
    /// <returns>Raw JSON string response from the LLM</returns>
    Task<string> ProposeStoryRootMergeAsync(string rawInput, string promptVersion);

    /// <summary>
    /// Generates a World State merge proposal from raw input using the specified prompt template version.
    /// Returns raw JSON/string response from the LLM without parsing or validation.
    /// </summary>
    /// <param name="rawInput">The raw user input to merge into the World State</param>
    /// <param name="promptVersion">Version identifier for the prompt template to use</param>
    /// <returns>Raw JSON string response from the LLM</returns>
    Task<string> ProposeWorldStateMergeAsync(string rawInput, string promptVersion);
}
