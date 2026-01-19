namespace SimpleTodo.Api.Models;

/// <summary>
/// Data transfer object for structured prompt inputs.
/// Contains template identifier and variables dictionary for Prompt Factory consumption.
/// </summary>
public class PromptInput
{
    /// <summary>
    /// Identifier of the template to use for prompt assembly.
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Key-value pairs for variable substitution in the template.
    /// Variable names should match template placeholders (e.g., {variable_name}).
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = new();
}
