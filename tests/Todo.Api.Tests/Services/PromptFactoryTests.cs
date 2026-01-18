using Moq;
using SimpleTodo.Api.Services;
using Todo.Api.Tests.Fixtures;
using Xunit;

namespace Todo.Api.Tests.Services;

/// <summary>
/// Unit tests for PromptFactory to verify prompt assembly from templates and variables.
/// </summary>
public class PromptFactoryTests
{
    private readonly Mock<ITemplateLoader> _templateLoaderMock;
    private readonly Mock<IVariableSubstitutor> _variableSubstitutorMock;
    private readonly PromptFactory _factory;

    public PromptFactoryTests()
    {
        _templateLoaderMock = new Mock<ITemplateLoader>();
        _variableSubstitutorMock = new Mock<IVariableSubstitutor>();
        _factory = new PromptFactory(_templateLoaderMock.Object, _variableSubstitutorMock.Object);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task AssemblePromptAsync_ValidInput_ReturnsSubstitutedPrompt()
    {
        // Arrange
        var templateId = "test-template";
        var template = PromptFactoryFixtures.SimpleTemplate;
        var variables = PromptFactoryFixtures.SimpleVariables;
        var expectedPrompt = PromptFactoryFixtures.SimpleTemplateExpectedResult;

        _templateLoaderMock
            .Setup(l => l.LoadTemplateAsync(templateId))
            .ReturnsAsync(template);

        _variableSubstitutorMock
            .Setup(s => s.Substitute(template, variables))
            .Returns(expectedPrompt);

        // Act
        var result = await _factory.AssemblePromptAsync(templateId, variables);

        // Assert
        Assert.Equal(expectedPrompt, result);
        _templateLoaderMock.Verify(l => l.LoadTemplateAsync(templateId), Times.Once);
        _variableSubstitutorMock.Verify(s => s.Substitute(template, variables), Times.Once);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task AssemblePromptAsync_EmptyTemplateId_ThrowsArgumentException()
    {
        // Arrange
        var variables = PromptFactoryFixtures.SimpleVariables;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factory.AssemblePromptAsync("", variables));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task AssemblePromptAsync_WhitespaceTemplateId_ThrowsArgumentException()
    {
        // Arrange
        var variables = PromptFactoryFixtures.SimpleVariables;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _factory.AssemblePromptAsync("   ", variables));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task AssemblePromptAsync_NullVariables_ThrowsArgumentNullException()
    {
        // Arrange
        var templateId = "test-template";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _factory.AssemblePromptAsync(templateId, null!));
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task AssemblePromptAsync_TemplateNotFound_PropagatesException()
    {
        // Arrange
        var templateId = "nonexistent-template";
        var variables = PromptFactoryFixtures.SimpleVariables;

        _templateLoaderMock
            .Setup(l => l.LoadTemplateAsync(templateId))
            .ThrowsAsync(new TemplateNotFoundException(templateId));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            _factory.AssemblePromptAsync(templateId, variables));
        Assert.Equal(templateId, ex.TemplateId);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task AssemblePromptAsync_MissingVariables_PropagatesException()
    {
        // Arrange
        var templateId = "test-template";
        var template = PromptFactoryFixtures.SimpleTemplate;
        var variables = PromptFactoryFixtures.IncompleteVariables;

        _templateLoaderMock
            .Setup(l => l.LoadTemplateAsync(templateId))
            .ReturnsAsync(template);

        _variableSubstitutorMock
            .Setup(s => s.Substitute(template, variables))
            .Throws(new VariableSubstitutionException("Missing variables: location", new[] { "location" }));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<VariableSubstitutionException>(() =>
            _factory.AssemblePromptAsync(templateId, variables));
        Assert.Contains("location", ex.MissingVariables);
    }

    [Fact]
    [Trait("Category", "FastLocal")]
    public async Task AssemblePromptAsync_EmptyVariablesDictionary_WorksWithNoVariableTemplate()
    {
        // Arrange
        var templateId = "no-vars-template";
        var template = PromptFactoryFixtures.NoVariablesTemplate;
        var variables = PromptFactoryFixtures.EmptyVariables;

        _templateLoaderMock
            .Setup(l => l.LoadTemplateAsync(templateId))
            .ReturnsAsync(template);

        _variableSubstitutorMock
            .Setup(s => s.Substitute(template, variables))
            .Returns(PromptFactoryFixtures.NoVariablesExpectedResult);

        // Act
        var result = await _factory.AssemblePromptAsync(templateId, variables);

        // Assert
        Assert.Equal(PromptFactoryFixtures.NoVariablesExpectedResult, result);
    }
}
