using SimpleTodo.Api.Services;
using Todo.Api.Tests.Fixtures;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Unit tests for SimpleVariableSubstitutor to verify variable substitution functionality.
/// </summary>
public class VariableSubstitutorTests
{
    private readonly SimpleVariableSubstitutor _substitutor;

    public VariableSubstitutorTests()
    {
        _substitutor = new SimpleVariableSubstitutor();
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_SimpleTemplate_ReplacesAllVariables()
    {
        // Arrange
        var template = PromptFactoryFixtures.SimpleTemplate;
        var variables = PromptFactoryFixtures.SimpleVariables;

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Equal(PromptFactoryFixtures.SimpleTemplateExpectedResult, result);
        Assert.DoesNotContain("{name}", result);
        Assert.DoesNotContain("{location}", result);
        Assert.Contains("Alice", result);
        Assert.Contains("Wonderland", result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_SingleVariable_ReplacesVariable()
    {
        // Arrange
        var template = PromptFactoryFixtures.SingleVariableTemplate;
        var variables = PromptFactoryFixtures.SingleVariableDictionary;

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Equal(PromptFactoryFixtures.SingleVariableExpectedResult, result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_NoVariables_ReturnsOriginalTemplate()
    {
        // Arrange
        var template = PromptFactoryFixtures.NoVariablesTemplate;
        var variables = PromptFactoryFixtures.EmptyVariables;

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Equal(PromptFactoryFixtures.NoVariablesExpectedResult, result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_RepeatedVariable_ReplacesAllOccurrences()
    {
        // Arrange
        var template = PromptFactoryFixtures.RepeatedVariableTemplate;
        var variables = PromptFactoryFixtures.RepeatedVariableDictionary;

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Equal(PromptFactoryFixtures.RepeatedVariableExpectedResult, result);
        Assert.DoesNotContain("{name}", result);
        Assert.Equal(2, result.Split("Charlie").Length - 1); // Should appear twice
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_ComplexTemplate_ReplacesAllVariables()
    {
        // Arrange
        var template = PromptFactoryFixtures.ComplexTemplate;
        var variables = PromptFactoryFixtures.ComplexVariables;

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Contains("Story Creation", result);
        Assert.Contains("A science fiction narrative", result);
        Assert.Contains("Create an engaging story", result);
        Assert.DoesNotContain("{task_name}", result);
        Assert.DoesNotContain("{context}", result);
        Assert.DoesNotContain("{instructions}", result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_ExtraVariables_IgnoresUnusedVariables()
    {
        // Arrange
        var template = PromptFactoryFixtures.SimpleTemplate;
        var variables = PromptFactoryFixtures.VariablesWithExtraKeys;

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Contains("Bob", result);
        Assert.Contains("City", result);
        // Extra variable should be ignored without error
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_MissingVariable_ThrowsVariableSubstitutionException()
    {
        // Arrange
        var template = PromptFactoryFixtures.SimpleTemplate;
        var variables = PromptFactoryFixtures.IncompleteVariables;

        // Act & Assert
        var ex = Assert.Throws<VariableSubstitutionException>(() =>
            _substitutor.Substitute(template, variables));
        Assert.Contains("location", ex.Message);
        Assert.Contains("location", ex.MissingVariables);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_MultipleMissingVariables_ThrowsExceptionWithAllMissing()
    {
        // Arrange
        var template = "{var1} and {var2} and {var3}";
        var variables = new Dictionary<string, string> { { "var1", "value1" } };

        // Act & Assert
        var ex = Assert.Throws<VariableSubstitutionException>(() =>
            _substitutor.Substitute(template, variables));
        Assert.Contains("var2", ex.MissingVariables);
        Assert.Contains("var3", ex.MissingVariables);
        Assert.Equal(2, ex.MissingVariables.Length);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_EmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var template = "";
        var variables = PromptFactoryFixtures.SimpleVariables;

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_NullTemplate_ReturnsNull()
    {
        // Arrange
        string? template = null;
        var variables = PromptFactoryFixtures.SimpleVariables;

        // Act
        var result = _substitutor.Substitute(template!, variables);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_NullVariables_ThrowsArgumentNullException()
    {
        // Arrange
        var template = PromptFactoryFixtures.SimpleTemplate;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _substitutor.Substitute(template, null!));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_VariableWithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var template = "Value: {value}";
        var variables = new Dictionary<string, string>
        {
            { "value", "Special chars: { } [ ] ( )" }
        };

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Contains("Special chars: { } [ ] ( )", result);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public void Substitute_VariableWithNewlines_PreservesNewlines()
    {
        // Arrange
        var template = "Multi-line:\n{content}\nEnd";
        var variables = new Dictionary<string, string>
        {
            { "content", "Line 1\nLine 2\nLine 3" }
        };

        // Act
        var result = _substitutor.Substitute(template, variables);

        // Assert
        Assert.Contains("Line 1\nLine 2\nLine 3", result);
    }
}
