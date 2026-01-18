namespace SimpleTodo.Api.Services;

/// <summary>
/// Mock implementation of ILlmService for development and testing environments.
/// Returns predefined responses for deterministic testing without live API calls.
/// </summary>
public class MockLlmService : ILlmService
{
    private string _defaultResponse;

    /// <summary>
    /// Initializes a new instance with a default response.
    /// </summary>
    /// <param name="defaultResponse">The default response to return for all invocations</param>
    public MockLlmService(string defaultResponse = "{}")
    {
        _defaultResponse = defaultResponse;
    }

    /// <summary>
    /// Sets the default response that will be returned by InvokeAsync.
    /// </summary>
    /// <param name="response">The response string to return</param>
    public void SetDefaultResponse(string response)
    {
        _defaultResponse = response;
    }

    /// <summary>
    /// Invokes the mock LLM service, returning the configured default response.
    /// </summary>
    /// <param name="prompt">The prompt to send (ignored in mock implementation)</param>
    /// <returns>The configured mock response</returns>
    public Task<string> InvokeAsync(string prompt)
    {
        return Task.FromResult(_defaultResponse);
    }
}
