using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleTodo.Api.Models;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Production implementation of ILlmService that integrates with OpenAI API.
/// Uses HttpClient for thread-safe HTTP requests and handles error mapping.
/// </summary>
public class OpenAILlmService : ILlmService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LlmServiceConfiguration _config;
    private readonly ILogger<OpenAILlmService> _logger;

    public OpenAILlmService(
        IHttpClientFactory httpClientFactory,
        IOptions<LlmServiceConfiguration> config,
        ILogger<OpenAILlmService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the OpenAI API with the provided prompt.
    /// </summary>
    /// <param name="prompt">The complete prompt to send to the LLM</param>
    /// <returns>The raw LLM response as a string</returns>
    /// <exception cref="LlmAuthenticationException">When authentication fails (401)</exception>
    /// <exception cref="LlmRateLimitException">When rate limits are exceeded (429)</exception>
    /// <exception cref="LlmNetworkException">When network or HTTP errors occur</exception>
    public async Task<string> InvokeAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
        }

        ValidateConfiguration();

        var httpClient = _httpClientFactory.CreateClient();

        // Prepare request
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

        var requestBody = new
        {
            model = _config.Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        _logger.LogInformation("Invoking OpenAI LLM with model {Model}. Prompt length: {PromptLength} characters",
            _config.Model, prompt.Length);

        try
        {
            var startTime = DateTime.UtcNow;

            var response = await httpClient.PostAsJsonAsync(_config.Endpoint, requestBody);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("OpenAI API call completed in {DurationMs}ms with status {StatusCode}",
                duration.TotalMilliseconds, (int)response.StatusCode);

            // Handle different HTTP status codes
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("OpenAI API authentication failed - invalid API key");
                throw new LlmAuthenticationException("OpenAI API authentication failed. Please check your API key.");
            }

            if (response.StatusCode == (HttpStatusCode)429)
            {
                _logger.LogWarning("OpenAI API rate limit exceeded");
                throw new LlmRateLimitException("OpenAI API rate limit exceeded. Please try again later.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API call failed with status {StatusCode}: {ErrorContent}",
                    (int)response.StatusCode, errorContent);
                throw new LlmNetworkException($"OpenAI API call failed with status {(int)response.StatusCode}: {errorContent}");
            }

            // Parse response
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);

            // Extract content from OpenAI response structure
            // OpenAI API returns: { "choices": [{ "message": { "content": "..." } }] }
            var content = responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("OpenAI API returned empty content");
                return string.Empty;
            }

            _logger.LogInformation("OpenAI API returned response with length {ResponseLength} characters",
                content.Length);

            return content;
        }
        catch (LlmServiceException)
        {
            // Re-throw our custom exceptions as-is
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during OpenAI API call");
            throw new LlmNetworkException("Network error during OpenAI API call", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OpenAI API call timed out");
            throw new LlmNetworkException("OpenAI API call timed out", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI API response as JSON");
            throw new LlmNetworkException("Failed to parse OpenAI API response", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OpenAI API call");
            throw new LlmNetworkException("Unexpected error during OpenAI API call", ex);
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured. Please set OpenAI:ApiKey in configuration.");
        }

        if (string.IsNullOrWhiteSpace(_config.Endpoint))
        {
            throw new InvalidOperationException("OpenAI endpoint is not configured. Please set OpenAI:Endpoint in configuration.");
        }

        if (string.IsNullOrWhiteSpace(_config.Model))
        {
            throw new InvalidOperationException("OpenAI model is not configured. Please set OpenAI:Model in configuration.");
        }
    }
}
