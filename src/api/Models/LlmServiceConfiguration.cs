namespace SimpleTodo.Api.Models;

/// <summary>
/// Configuration class for LLM service settings.
/// Maps to the "OpenAI" configuration section in appsettings.json.
/// </summary>
public class LlmServiceConfiguration
{
    /// <summary>
    /// OpenAI API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI model identifier to use for API calls.
    /// </summary>
    public string Model { get; set; } = string.Empty;
}
