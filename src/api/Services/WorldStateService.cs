using SimpleTodo.Api.Models;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Validation;
using System.Text.Json;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Service layer implementation for World State operations.
/// Orchestrates calls to repositories, LLM services, and validators,
/// handling business logic, parsing, validation, and merge operations.
/// This service is completely independent from StoryRootService.
/// </summary>
public class WorldStateService : IWorldStateService
{
    private readonly IWorldStateRepository _repository;
    private readonly ILlmService _llmService;
    private readonly IPromptTemplateService _promptTemplateService;
    private readonly IUserContextService _userContextService;
    private readonly WorldStateValidator _validator;
    private readonly JsonSerializerOptions _jsonOptions;

    public WorldStateService(
        IWorldStateRepository repository,
        ILlmService llmService,
        IPromptTemplateService promptTemplateService,
        IUserContextService userContextService,
        WorldStateValidator validator)
    {
        _repository = repository;
        _llmService = llmService;
        _promptTemplateService = promptTemplateService;
        _userContextService = userContextService;
        _validator = validator;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<WorldState?> GetCurrentWorldStateAsync()
    {
        var userId = _userContextService.GetCurrentUserId();
        return await _repository.GetCurrentVersionAsync(userId);
    }

    public async Task<WorldState?> GetWorldStateVersionAsync(string versionId)
    {
        if (string.IsNullOrWhiteSpace(versionId))
        {
            throw new ArgumentException("Version ID cannot be null or empty", nameof(versionId));
        }

        var userId = _userContextService.GetCurrentUserId();
        return await _repository.GetVersionAsync(userId, versionId);
    }

    public async Task<List<VersionMetadata>> ListWorldStateVersionsAsync()
    {
        var userId = _userContextService.GetCurrentUserId();
        return await _repository.ListVersionsAsync(userId);
    }

    public async Task<WorldState> ProposeWorldStateMergeAsync(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            throw new ArgumentException("Raw input cannot be null or empty", nameof(rawInput));
        }

        // Get current World State (if exists)
        var currentWorldState = await GetCurrentWorldStateAsync();
        var currentWorldStateJson = currentWorldState != null
            ? JsonSerializer.Serialize(currentWorldState, _jsonOptions)
            : "null";

        // Get prompt template (using version "1.0" for Phase-0)
        var promptTemplate = await _promptTemplateService.GetPromptTemplateAsync("world_state_merge", "1.0");

        // Replace template placeholders
        var prompt = promptTemplate
            .Replace("{current_world_state}", currentWorldStateJson)
            .Replace("{user_input}", rawInput);

        // Call LLM service
        string llmResponse;
        try
        {
            llmResponse = await _llmService.InvokeAsync(prompt);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"LLM service call failed: {ex.Message}", ex);
        }

        // Parse LLM response
        if (string.IsNullOrWhiteSpace(llmResponse))
        {
            throw new InvalidOperationException("LLM service returned empty response");
        }

        WorldState? proposal;
        try
        {
            // Try to parse the JSON response
            proposal = JsonSerializer.Deserialize<WorldState>(llmResponse, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse LLM response as JSON: {ex.Message}. Response: {llmResponse}", ex);
        }

        if (proposal == null)
        {
            throw new InvalidOperationException($"LLM response deserialized to null. Response: {llmResponse}");
        }

        // Validate the proposal
        var (isValid, errorMessage) = _validator.Validate(proposal);
        if (!isValid)
        {
            throw new InvalidOperationException($"Proposal validation failed: {errorMessage}. Response: {llmResponse}");
        }

        return proposal;
    }

    public async Task<string> CommitWorldStateVersionAsync(WorldState proposal, string? identity = null)
    {
        if (proposal == null)
        {
            throw new ArgumentNullException(nameof(proposal));
        }

        // Validate the proposal before committing
        var (isValid, errorMessage) = _validator.Validate(proposal);
        if (!isValid)
        {
            throw new ArgumentException($"Proposal validation failed: {errorMessage}", nameof(proposal));
        }

        // Get current version to establish prior version link
        // The repository returns versions ordered by timestamp descending (newest first)
        var versions = await ListWorldStateVersionsAsync();
        var priorVersionId = versions.FirstOrDefault()?.VersionId;

        // Generate source request ID for provenance
        var sourceRequestId = Guid.NewGuid().ToString("N");

        // Get environment (could be from configuration in future)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Save new version with llm_assisted flag set to true
        var versionId = await _repository.SaveNewVersionAsync(
            proposal,
            priorVersionId: priorVersionId,
            sourceRequestId: sourceRequestId,
            environment: environment,
            llmAssisted: true);

        return versionId;
    }
}
