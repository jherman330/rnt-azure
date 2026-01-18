namespace SimpleTodo.Api.Services;

/// <summary>
/// Interface for LLM service interactions.
/// This interface enables mocking of LLM service calls for testing purposes,
/// ensuring zero live LLM calls in test scenarios.
/// This is a generic, domain-agnostic interface that provides the technical capability
/// of making LLM API calls without any domain-specific knowledge.
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Invokes the LLM with a complete prompt string.
    /// </summary>
    /// <param name="prompt">The complete prompt to send to the LLM</param>
    /// <returns>The raw LLM response as a string</returns>
    /// <remarks>
    /// - Calls are synchronous and one-shot only
    /// - No retry logic; failures are surfaced to the caller
    /// - Caller is responsible for parsing and validating the response
    /// - LLM output is treated as raw data, no interpretation performed
    /// </remarks>
    Task<string> InvokeAsync(string prompt);
}
