namespace SimpleTodo.Api.Services;

/// <summary>
/// Generic, domain-agnostic Prompt Factory implementation.
/// Orchestrates template loading and variable substitution to assemble prompt strings.
/// </summary>
public class PromptFactory : IPromptFactory
{
    private readonly ITemplateLoader _templateLoader;
    private readonly IVariableSubstitutor _variableSubstitutor;

    public PromptFactory(
        ITemplateLoader templateLoader,
        IVariableSubstitutor variableSubstitutor)
    {
        _templateLoader = templateLoader ?? throw new ArgumentNullException(nameof(templateLoader));
        _variableSubstitutor = variableSubstitutor ?? throw new ArgumentNullException(nameof(variableSubstitutor));
    }

    public async Task<string> AssemblePromptAsync(string templateId, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            throw new ArgumentException("Template ID cannot be null or empty", nameof(templateId));
        }

        if (variables == null)
        {
            throw new ArgumentNullException(nameof(variables));
        }

        // Load template
        var template = await _templateLoader.LoadTemplateAsync(templateId);

        // Substitute variables
        var prompt = _variableSubstitutor.Substitute(template, variables);

        return prompt;
    }
}
