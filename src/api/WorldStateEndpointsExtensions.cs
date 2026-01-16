using Microsoft.AspNetCore.Http.HttpResults;
using SimpleTodo.Api.Middleware;
using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services;

namespace SimpleTodo.Api;

/// <summary>
/// Extension methods for mapping World State API endpoints following the minimal APIs pattern.
/// This endpoint group is completely independent from Story Root endpoints.
/// </summary>
public static class WorldStateEndpointsExtensions
{
    public static RouteGroupBuilder MapWorldStateApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetCurrentWorldState);
        group.MapGet("/versions/{versionId}", GetWorldStateVersion);
        group.MapGet("/versions", GetWorldStateVersions);
        group.MapPost("/propose-merge", ProposeWorldStateMerge);
        group.MapPost("/commit", CommitWorldState);
        return group;
    }

    /// <summary>
    /// GET /api/world-state - Returns the current World State for the authenticated user.
    /// </summary>
    public static async Task<IResult> GetCurrentWorldState(
        IWorldStateService worldStateService,
        HttpContext context)
    {
        try
        {
            var worldState = await worldStateService.GetCurrentWorldStateAsync();

            if (worldState == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(worldState);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while retrieving the current World State: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// GET /api/world-state/versions/{versionId} - Returns a specific version of the World State.
    /// </summary>
    public static async Task<IResult> GetWorldStateVersion(
        IWorldStateService worldStateService,
        string versionId,
        HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(versionId))
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = "Version ID is required.",
                    CorrelationId = correlationId
                },
                statusCode: 400);
        }

        try
        {
            var worldState = await worldStateService.GetWorldStateVersionAsync(versionId);

            if (worldState == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(worldState);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while retrieving World State version: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// GET /api/world-state/versions - Returns a list of all World State versions for the authenticated user.
    /// </summary>
    public static async Task<IResult> GetWorldStateVersions(
        IWorldStateService worldStateService,
        HttpContext context)
    {
        try
        {
            var versions = await worldStateService.ListWorldStateVersionsAsync();
            return TypedResults.Ok(versions);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while retrieving World State versions: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// POST /api/world-state/propose-merge - Generates an LLM proposal for merging raw input into the World State.
    /// </summary>
    public static async Task<IResult> ProposeWorldStateMerge(
        IWorldStateService worldStateService,
        ProposeWorldStateMergeRequest request,
        HttpContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RawInput))
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = "Request body is required and must include a non-empty 'raw_input' field.",
                    CorrelationId = correlationId
                },
                statusCode: 400);
        }

        try
        {
            var currentWorldState = await worldStateService.GetCurrentWorldStateAsync();
            var proposal = await worldStateService.ProposeWorldStateMergeAsync(request.RawInput);

            var response = new ProposalResponse<WorldState>
            {
                Proposal = proposal,
                Current = currentWorldState
            };

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while proposing World State merge: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// POST /api/world-state/commit - Commits a World State proposal as a new version.
    /// </summary>
    public static async Task<IResult> CommitWorldState(
        IWorldStateService worldStateService,
        CommitWorldStateRequest request,
        HttpContext context)
    {
        if (request == null || request.WorldState == null)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = "Request body is required and must include a 'world_state' field.",
                    CorrelationId = correlationId
                },
                statusCode: 400);
        }

        try
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            var versionId = await worldStateService.CommitWorldStateVersionAsync(request.WorldState, correlationId);

            var response = new CommitResponse<WorldState>
            {
                VersionId = versionId,
                Artifact = request.WorldState
            };

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while committing World State: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }
}
