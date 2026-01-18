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

    /// <summary>
    /// Invokes the mock LLM service with a prompt.
    /// Returns predefined responses based on configured mappings.
    /// </summary>
    /// <param name="prompt">The prompt to send (used to detect scenario if configured)</param>
    /// <returns>The configured mock response</returns>
    public Task<string> InvokeAsync(string prompt)
    {
        // Try to detect scenario from prompt content (backward compatibility)
        // Check if prompt contains hints about story_root or world_state
        bool isStoryRoot = prompt.Contains("story_root", StringComparison.OrdinalIgnoreCase) ||
                          prompt.Contains("StoryRoot", StringComparison.OrdinalIgnoreCase);
        bool isWorldState = prompt.Contains("world_state", StringComparison.OrdinalIgnoreCase) ||
                           prompt.Contains("WorldState", StringComparison.OrdinalIgnoreCase);

        // Priority: default > scenario-specific > first available
        if (_responseMap.TryGetValue("default", out var defaultResponse))
        {
            return Task.FromResult(defaultResponse);
        }

        if (isStoryRoot)
        {
            // Check for factory first (allows dynamic responses)
            if (_responseFactoryMap.TryGetValue("story_root", out var factory))
            {
                // Factory signature changed - it now receives (prompt) instead of (rawInput, promptVersion)
                // For backward compatibility, pass prompt as first arg, empty string as second
                return Task.FromResult(factory(prompt, string.Empty));
            }

            // Fall back to static response
            if (_responseMap.TryGetValue("story_root", out var storyRootResponse))
            {
                return Task.FromResult(storyRootResponse);
            }
        }

        if (isWorldState)
        {
            // Check for factory first (allows dynamic responses)
            if (_responseFactoryMap.TryGetValue("world_state", out var factory))
            {
                // Factory signature changed - it now receives (prompt) instead of (rawInput, promptVersion)
                // For backward compatibility, pass prompt as first arg, empty string as second
                return Task.FromResult(factory(prompt, string.Empty));
            }

            // Fall back to static response
            if (_responseMap.TryGetValue("world_state", out var worldStateResponse))
            {
                return Task.FromResult(worldStateResponse);
            }
        }

        // Fall back to first available response or default fixture
        if (_responseMap.Count > 0)
        {
            return Task.FromResult(_responseMap.Values.First());
        }

        // Ultimate fallback: return story root fixture as default
        return Task.FromResult(LlmResponseFixtures.ValidStoryRoot);
    }
}
