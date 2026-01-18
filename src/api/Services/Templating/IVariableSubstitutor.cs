namespace SimpleTodo.Api.Services;

/// <summary>
/// Interface for performing variable substitution in templates.
/// </summary>
public interface IVariableSubstitutor
{
    /// <summary>
    /// Substitutes variables in a template string.
    /// </summary>
    /// <param name="template">The template string containing variable placeholders</param>
    /// <param name="variables">Key-value pairs for variable substitution</param>
    /// <returns>The template with variables substituted</returns>
    /// <exception cref="VariableSubstitutionException">Thrown when required variables are missing</exception>
    string Substitute(string template, Dictionary<string, string> variables);
}
