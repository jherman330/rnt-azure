namespace SimpleTodo.Api.Services;

/// <summary>
/// Interface for assembling prompt strings from templates and variables.
/// This is a generic, domain-agnostic service that performs mechanical template assembly
/// without any domain-specific knowledge or logic.
/// </summary>
public interface IPromptFactory
{
    /// <summary>
    /// Assembles a prompt string from a template and variables.
    /// </summary>
    /// <param name="templateId">Identifier of the template to use</param>
    /// <param name="variables">Key-value pairs for variable substitution</param>
    /// <returns>Final prompt string with variables substituted</returns>
    /// <remarks>
    /// - Performs mechanical template assembly only
    /// - No domain logic or conditional processing
    /// - Caller responsible for providing all required variables
    /// - Template loading and variable substitution are deterministic
    /// </remarks>
    Task<string> AssemblePromptAsync(string templateId, Dictionary<string, string> variables);
}
