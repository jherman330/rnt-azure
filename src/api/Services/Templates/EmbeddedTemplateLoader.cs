using System.Reflection;
using System.Text;
using System.Linq;

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
        // e.g., "story-root-merge" -> "SimpleTodo.Api.Templates.StoryRootMerge.txt"
        var resourceFileName = ConvertTemplateIdToResourceName(templateId);
        var resourceName = $"{_resourcePrefix}{resourceFileName}.txt";

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

    /// <summary>
    /// Converts a kebab-case template ID to PascalCase for embedded resource lookup.
    /// If the template ID contains hyphens, converts kebab-case to PascalCase.
    /// If it doesn't contain hyphens, assumes it's already PascalCase and returns it unchanged.
    /// e.g., "story-root-create" -> "StoryRootCreate"
    ///       "StoryRootMerge" -> "StoryRootMerge" (unchanged)
    /// </summary>
    /// <param name="templateId">The template ID (kebab-case or PascalCase)</param>
    /// <returns>The template ID in PascalCase format for resource lookup</returns>
    private static string ConvertTemplateIdToResourceName(string templateId)
    {
        // If no hyphens, assume already in PascalCase format (for backward compatibility with tests)
        if (!templateId.Contains('-'))
        {
            return templateId;
        }

        // Split by hyphens and capitalize each part
        var parts = templateId.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", parts.Select(part => 
            char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));
    }
}
