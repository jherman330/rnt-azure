namespace SimpleTodo.Api.Services;

/// <summary>
/// Placeholder implementation of ILlmService for dependency injection registration.
/// This implementation will be replaced with actual LLM integration in future work orders.
/// </summary>
public class PlaceholderLlmService : ILlmService
{
    public Task<string> ProposeStoryRootMergeAsync(string rawInput, string promptVersion)
    {
        throw new NotImplementedException("LLM integration will be implemented in a future work order. Use MockLlmService for testing.");
    }

    public Task<string> ProposeWorldStateMergeAsync(string rawInput, string promptVersion)
    {
        throw new NotImplementedException("LLM integration will be implemented in a future work order. Use MockLlmService for testing.");
    }
}
