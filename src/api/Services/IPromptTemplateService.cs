namespace SimpleTodo.Api.Services;

/// <summary>
/// Interface for retrieving version-controlled prompt templates.
/// </summary>
public interface IPromptTemplateService
{
    /// <summary>
    /// Retrieves a prompt template by name and version.
    /// </summary>
    /// <param name="templateName">Name of the template (e.g., "story_root_merge", "world_state_merge")</param>
    /// <param name="version">Version identifier for the template</param>
    /// <returns>The prompt template string</returns>
    Task<string> GetPromptTemplateAsync(string templateName, string version);
}
