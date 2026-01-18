using System.Text.RegularExpressions;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Simple variable substitutor that replaces {variableName} placeholders with provided values.
/// </summary>
public class SimpleVariableSubstitutor : IVariableSubstitutor
{
    /// <summary>
    /// Pattern to match variable placeholders: {variableName}
    /// </summary>
    private static readonly Regex VariablePattern = new(@"\{([a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

    public string Substitute(string template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        if (variables == null || variables.Count == 0)
        {
            return template;
        }

        var missingVariables = new List<string>();
        var result = VariablePattern.Replace(template, match =>
        {
            var variableName = match.Groups[1].Value;
            
            if (variables.TryGetValue(variableName, out var value))
            {
                return value;
            }

            missingVariables.Add(variableName);
            return match.Value; // Leave placeholder unchanged if variable is missing
        });

        if (missingVariables.Count > 0)
        {
            var missingList = string.Join(", ", missingVariables);
            throw new VariableSubstitutionException(
                $"Missing required variables: {missingList}. Provided variables: {string.Join(", ", variables.Keys)}",
                missingVariables.ToArray());
        }

        return result;
    }
}
