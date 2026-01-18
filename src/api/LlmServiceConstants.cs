namespace SimpleTodo.Api;

/// <summary>
/// Constants related to LLM service configuration keys and section names.
/// Centralizes magic strings to improve code maintainability.
/// </summary>
public static class LlmServiceConstants
{
    /// <summary>
    /// Configuration section name for OpenAI settings.
    /// </summary>
    public const string ConfigurationSection = "OpenAI";

    /// <summary>
    /// Configuration key for OpenAI API key.
    /// </summary>
    public const string ApiKeyKey = "OpenAI:ApiKey";

    /// <summary>
    /// Configuration key for OpenAI API endpoint.
    /// </summary>
    public const string EndpointKey = "OpenAI:Endpoint";

    /// <summary>
    /// Configuration key for OpenAI model identifier.
    /// </summary>
    public const string ModelKey = "OpenAI:Model";
}
