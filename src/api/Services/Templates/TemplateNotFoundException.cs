namespace SimpleTodo.Api.Services;

/// <summary>
/// Exception thrown when a requested template is not found.
/// </summary>
public class TemplateNotFoundException : Exception
{
    public string TemplateId { get; }

    public TemplateNotFoundException(string templateId) 
        : base($"Template not found: {templateId}")
    {
        TemplateId = templateId;
    }

    public TemplateNotFoundException(string templateId, Exception innerException) 
        : base($"Template not found: {templateId}", innerException)
    {
        TemplateId = templateId;
    }
}
