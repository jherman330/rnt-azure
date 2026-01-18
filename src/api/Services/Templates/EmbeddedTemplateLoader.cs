using System.Reflection;
using System.Text;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Template loader that loads templates from embedded resources.
/// Templates are expected to be in the Templates folder as embedded resources.
/// Template identifier is converted to a file path: Templates/{templateId}.txt
/// </summary>
public class EmbeddedTemplateLoader : ITemplateLoader
{
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;
    private readonly Dictionary<string, string> _templateCache;

    public EmbeddedTemplateLoader(Assembly? assembly = null)
    {
        _assembly = assembly ?? Assembly.GetExecutingAssembly();
        
        // Get the base namespace from the assembly (e.g., "SimpleTodo.Api")
        var assemblyName = _assembly.GetName().Name ?? string.Empty;
        _resourcePrefix = $"{assemblyName}.Templates.";
        
        // Thread-safe template cache using ConcurrentDictionary pattern
        // Note: For simplicity, using Dictionary with lock in production code
        // For higher concurrency, could use ConcurrentDictionary
        _templateCache = new Dictionary<string, string>();
    }

    public async Task<string> LoadTemplateAsync(string templateId)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            throw new ArgumentException("Template ID cannot be null or empty", nameof(templateId));
        }

        // Check cache first
        lock (_templateCache)
        {
            if (_templateCache.TryGetValue(templateId, out var cachedTemplate))
            {
                return cachedTemplate;
            }
        }

        // Convert template ID to resource path
        // e.g., "story-root-merge" -> "SimpleTodo.Api.Templates.story-root-merge.txt"
        var resourceName = $"{_resourcePrefix}{templateId}.txt";

        // Load from embedded resources
        var template = await LoadFromEmbeddedResourceAsync(resourceName);

        // Cache the template
        lock (_templateCache)
        {
            if (!_templateCache.ContainsKey(templateId))
            {
                _templateCache[templateId] = template;
            }
        }

        return template;
    }

    private async Task<string> LoadFromEmbeddedResourceAsync(string resourceName)
    {
        using var stream = _assembly.GetManifestResourceStream(resourceName);
        
        if (stream == null)
        {
            throw new TemplateNotFoundException(
                resourceName.Replace(_resourcePrefix, "").Replace(".txt", ""),
                new FileNotFoundException($"Embedded resource not found: {resourceName}"));
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
