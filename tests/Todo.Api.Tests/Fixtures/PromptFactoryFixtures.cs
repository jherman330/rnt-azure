namespace Todo.Api.Tests.Fixtures;

/// <summary>
/// Test fixtures for Prompt Factory testing, including template content and variable dictionaries.
/// </summary>
public static class PromptFactoryFixtures
{
    /// <summary>
    /// Simple test template with two variables.
    /// </summary>
    public const string SimpleTemplate = "Hello {name}, welcome to {location}!";

    /// <summary>
    /// Template with multiple variables and newlines.
    /// </summary>
    public const string ComplexTemplate = @"You are helping with {task_name}.

Context:
{context}

Instructions:
{instructions}

Please provide a response.";

    /// <summary>
    /// Template with single variable.
    /// </summary>
    public const string SingleVariableTemplate = "The value is: {value}";

    /// <summary>
    /// Template with no variables (plain text).
    /// </summary>
    public const string NoVariablesTemplate = "This template has no variables.";

    /// <summary>
    /// Template with repeated variable occurrences.
    /// </summary>
    public const string RepeatedVariableTemplate = "{name} says hello to {name}";

    /// <summary>
    /// Variables for SimpleTemplate.
    /// </summary>
    public static Dictionary<string, string> SimpleVariables => new()
    {
        { "name", "Alice" },
        { "location", "Wonderland" }
    };

    /// <summary>
    /// Variables for ComplexTemplate.
    /// </summary>
    public static Dictionary<string, string> ComplexVariables => new()
    {
        { "task_name", "Story Creation" },
        { "context", "A science fiction narrative" },
        { "instructions", "Create an engaging story" }
    };

    /// <summary>
    /// Variables for SingleVariableTemplate.
    /// </summary>
    public static Dictionary<string, string> SingleVariableDictionary => new()
    {
        { "value", "42" }
    };

    /// <summary>
    /// Empty variables dictionary.
    /// </summary>
    public static Dictionary<string, string> EmptyVariables => new();

    /// <summary>
    /// Variables with extra keys not used in template.
    /// </summary>
    public static Dictionary<string, string> VariablesWithExtraKeys => new()
    {
        { "name", "Bob" },
        { "location", "City" },
        { "unused_key", "ignored" }
    };

    /// <summary>
    /// Variables for RepeatedVariableTemplate.
    /// </summary>
    public static Dictionary<string, string> RepeatedVariableDictionary => new()
    {
        { "name", "Charlie" }
    };

    /// <summary>
    /// Variables with missing required keys.
    /// </summary>
    public static Dictionary<string, string> IncompleteVariables => new()
    {
        { "name", "David" }
        // Missing "location"
    };

    /// <summary>
    /// Expected result for SimpleTemplate with SimpleVariables.
    /// </summary>
    public const string SimpleTemplateExpectedResult = "Hello Alice, welcome to Wonderland!";

    /// <summary>
    /// Expected result for SingleVariableTemplate with SingleVariableDictionary.
    /// </summary>
    public const string SingleVariableExpectedResult = "The value is: 42";

    /// <summary>
    /// Expected result for NoVariablesTemplate.
    /// </summary>
    public const string NoVariablesExpectedResult = "This template has no variables.";

    /// <summary>
    /// Expected result for RepeatedVariableTemplate with RepeatedVariableDictionary.
    /// </summary>
    public const string RepeatedVariableExpectedResult = "Charlie says hello to Charlie";
}
