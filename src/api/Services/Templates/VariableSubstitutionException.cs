namespace SimpleTodo.Api.Services;

/// <summary>
/// Exception thrown when variable substitution fails (e.g., missing required variables).
/// </summary>
public class VariableSubstitutionException : Exception
{
    public string[] MissingVariables { get; }

    public VariableSubstitutionException(string message) : base(message)
    {
        MissingVariables = Array.Empty<string>();
    }

    public VariableSubstitutionException(string message, string[] missingVariables) : base(message)
    {
        MissingVariables = missingVariables;
    }

    public VariableSubstitutionException(string message, Exception innerException) : base(message, innerException)
    {
        MissingVariables = Array.Empty<string>();
    }
}
