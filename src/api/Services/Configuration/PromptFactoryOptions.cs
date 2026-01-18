namespace SimpleTodo.Api.Services;

/// <summary>
/// Configuration options for Prompt Factory.
/// Maps to the "PromptFactory" configuration section in appsettings.json.
/// </summary>
public class PromptFactoryOptions
{
    /// <summary>
    /// Location where templates are stored. Defaults to "Templates".
    /// For embedded resources, this is the folder name within the project.
    /// </summary>
    public string TemplateLocation { get; set; } = "Templates";

    /// <summary>
    /// Whether to validate templates on service startup. Defaults to false.
    /// When true, attempts to load all available templates during initialization.
    /// </summary>
    public bool ValidateOnStartup { get; set; } = false;
}
