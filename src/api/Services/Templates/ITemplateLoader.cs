namespace SimpleTodo.Api.Services;

/// <summary>
/// Interface for loading prompt templates from storage.
/// </summary>
public interface ITemplateLoader
{
    /// <summary>
    /// Loads a template by its identifier.
    /// </summary>
    /// <param name="templateId">The identifier of the template to load</param>
    /// <returns>The template content as a string</returns>
    /// <exception cref="TemplateNotFoundException">Thrown when the template is not found</exception>
    Task<string> LoadTemplateAsync(string templateId);
}
