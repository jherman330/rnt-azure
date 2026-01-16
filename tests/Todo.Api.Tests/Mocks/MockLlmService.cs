using SimpleTodo.Api.Services;
using Todo.Api.Tests.Fixtures;

namespace Todo.Api.Tests.Mocks;

/// <summary>
/// Mock implementation of ILlmService that returns predefined responses
/// from LlmResponseFixtures for deterministic test execution.
/// </summary>
public class MockLlmService : ILlmService
{
    private readonly Dictionary<string, string> _responseMap = new();
    private readonly Dictionary<string, Func<string, string, string>> _responseFactoryMap = new();

    /// <summary>
    /// Initializes a new instance with default fixture mappings.
    /// </summary>
    public MockLlmService()
    {
        // Default to valid responses
        SetResponse("story_root", LlmResponseFixtures.ValidStoryRoot);
        SetResponse("world_state", LlmResponseFixtures.ValidWorldState);
    }

    /// <summary>
    /// Sets a response for a specific scenario key.
    /// </summary>
    /// <param name="scenarioKey">Key identifying the scenario (e.g., "story_root", "story_root_missing_field")</param>
    /// <param name="response">The response string to return for this scenario</param>
    public void SetResponse(string scenarioKey, string? response)
    {
        _responseMap[scenarioKey] = response ?? string.Empty;
    }

    /// <summary>
    /// Sets a response factory function for a specific scenario key.
    /// The factory receives (rawInput, promptVersion) and returns the response string.
    /// </summary>
    /// <param name="scenarioKey">Key identifying the scenario</param>
    /// <param name="factory">Function that generates the response based on input and version</param>
    public void SetResponseFactory(string scenarioKey, Func<string, string, string> factory)
    {
        _responseFactoryMap[scenarioKey] = factory;
    }

    /// <summary>
    /// Gets the configured response for a scenario, or null if not configured.
    /// </summary>
    /// <param name="scenarioKey">Key identifying the scenario</param>
    /// <returns>The response string, or null if not found</returns>
    public string? GetResponse(string scenarioKey)
    {
        return _responseMap.TryGetValue(scenarioKey, out var response) ? response : null;
    }

    /// <summary>
    /// Resets all responses to default valid fixtures.
    /// </summary>
    public void ResetToDefaults()
    {
        _responseMap.Clear();
        _responseFactoryMap.Clear();
        SetResponse("story_root", LlmResponseFixtures.ValidStoryRoot);
        SetResponse("world_state", LlmResponseFixtures.ValidWorldState);
    }

    /// <summary>
    /// Clears all configured responses.
    /// </summary>
    public void Clear()
    {
        _responseMap.Clear();
        _responseFactoryMap.Clear();
    }

    /// <summary>
    /// Checks if a response is configured for a given scenario.
    /// </summary>
    /// <param name="scenarioKey">Key identifying the scenario</param>
    /// <returns>True if a response is configured, false otherwise</returns>
    public bool HasResponse(string scenarioKey)
    {
        return _responseMap.ContainsKey(scenarioKey) || _responseFactoryMap.ContainsKey(scenarioKey);
    }

    public Task<string> ProposeStoryRootMergeAsync(string rawInput, string promptVersion)
    {
        // Check for factory first (allows dynamic responses based on input)
        if (_responseFactoryMap.TryGetValue("story_root", out var factory))
        {
            return Task.FromResult(factory(rawInput, promptVersion));
        }

        // Fall back to static response
        var response = _responseMap.TryGetValue("story_root", out var staticResponse)
            ? staticResponse
            : LlmResponseFixtures.ValidStoryRoot;

        return Task.FromResult(response);
    }

    public Task<string> ProposeWorldStateMergeAsync(string rawInput, string promptVersion)
    {
        // Check for factory first (allows dynamic responses based on input)
        if (_responseFactoryMap.TryGetValue("world_state", out var factory))
        {
            return Task.FromResult(factory(rawInput, promptVersion));
        }

        // Fall back to static response
        var response = _responseMap.TryGetValue("world_state", out var staticResponse)
            ? staticResponse
            : LlmResponseFixtures.ValidWorldState;

        return Task.FromResult(response);
    }
}
