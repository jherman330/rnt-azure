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
        SetResponse("story_root", LlmResponseFixtures.ValidStoryRoot);
        SetResponse("world_state", LlmResponseFixtures.ValidWorldState);
    }

    /// <summary>
    /// Clears all configured responses.
    /// </summary>
    public void Clear()
    {
        _responseMap.Clear();
    }

    /// <summary>
    /// Checks if a response is configured for a given scenario.
    /// </summary>
    /// <param name="scenarioKey">Key identifying the scenario</param>
    /// <returns>True if a response is configured, false otherwise</returns>
    public bool HasResponse(string scenarioKey)
    {
        return _responseMap.ContainsKey(scenarioKey);
    }
}
