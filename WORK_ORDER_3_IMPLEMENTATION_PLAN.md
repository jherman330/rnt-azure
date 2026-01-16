# Work Order 3 Implementation Plan - Corrected for Codebase

## Overview
Implement REST API endpoints for Story Root and World State management, enabling frontend applications to retrieve current artifacts, propose LLM-assisted updates, and commit approved changes.

## Current Codebase Status

### ✅ Already Implemented (from prior work orders)
- **Services**: `StoryRootService`, `WorldStateService` fully implemented
- **Repositories**: `IStoryRootRepository`, `IWorldStateRepository`, `BlobArtifactRepository`
- **Models**: `StoryRoot`, `WorldState`, `VersionMetadata` in `src/api/Models/`
- **Validation**: `StoryRootValidator`, `WorldStateValidator` in `src/api/Validation/`
- **User Context**: `IUserContextService`, `UserContextService` in `src/api/Services/`
- **LLM Integration**: `ILlmService`, `IPromptTemplateService` implemented
- **Testing**: Service layer unit tests with LLM fixture library

### ❌ Missing for Work Order 3
- API endpoints (`/api/story-root` and `/api/world-state`)
- Correlation ID middleware
- Request/Response DTOs
- Endpoint integration tests

## Implementation Plan

### Phase 1: Request/Response DTOs

**Files to Create:**
- `src/api/Models/Requests.cs` - Request DTOs for propose-merge and commit operations
- `src/api/Models/Responses.cs` - Response DTOs for proposals, commits, and errors

**Key Changes:**
- `ProposeStoryRootMergeRequest` - `{ "raw_input": "string" }`
- `ProposeWorldStateMergeRequest` - `{ "raw_input": "string" }`
- `CommitStoryRootRequest` - `{ "story_root": StoryRoot }`
- `CommitWorldStateRequest` - `{ "world_state": WorldState }`
- `ProposalResponse<T>` - `{ "proposal": T, "current": T }`
- `CommitResponse<T>` - `{ "version_id": "string", "artifact": T }`
- `ErrorResponse` - `{ "error": "string", "correlation_id": "string" }`

### Phase 2: Correlation ID Middleware

**Files to Create:**
- `src/api/Middleware/CorrelationIdMiddleware.cs`

**Key Changes:**
- Generate correlation ID for each request (GUID if not present in header)
- Store in `HttpContext.Items` for access throughout request pipeline
- Add to response headers (`X-Correlation-ID`)
- Must be first middleware in pipeline (before all other middleware)

### Phase 3: Story Root API Endpoints

**Files to Create:**
- `src/api/StoryRootEndpointsExtensions.cs` - Extension methods following `TodoEndpointsExtensions.cs` pattern

**Endpoints to Implement:**
- `GET /api/story-root` - Returns current Story Root (200 or 404)
- `GET /api/story-root/versions/{versionId}` - Returns specific version (200 or 404)
- `GET /api/story-root/versions` - Returns version metadata list (200)
- `POST /api/story-root/propose-merge` - Accepts raw input, returns proposal + current (200, 400, 500)
- `POST /api/story-root/commit` - Accepts approved Story Root, creates version (200, 400, 500)

**Key Implementation Details:**
- Use `IStoryRootService` injected via dependency injection
- Handle errors with correlation IDs in error responses
- Return proper HTTP status codes
- Validate request bodies
- Use extension method pattern: `.MapStoryRootApi()`

### Phase 4: World State API Endpoints

**Files to Create:**
- `src/api/WorldStateEndpointsExtensions.cs` - Extension methods following same pattern

**Endpoints to Implement:**
- `GET /api/world-state` - Returns current World State (200 or 404)
- `GET /api/world-state/versions/{versionId}` - Returns specific version (200 or 404)
- `GET /api/world-state/versions` - Returns version metadata list (200)
- `POST /api/world-state/propose-merge` - Accepts raw input, returns proposal + current (200, 400, 500)
- `POST /api/world-state/commit` - Accepts approved World State, creates version (200, 400, 500)

**Key Implementation Details:**
- Maintain complete independence from Story Root endpoints
- Follow same patterns as Story Root endpoints
- Use `IWorldStateService` injected via dependency injection

### Phase 5: Program.cs Updates

**File to Modify:**
- `src/api/Program.cs`

**Changes Required:**
1. Register CorrelationIdMiddleware (must be first middleware)
2. Map Story Root endpoints: `app.MapGroup("/api/story-root").MapStoryRootApi().WithOpenApi();`
3. Map World State endpoints: `app.MapGroup("/api/world-state").MapWorldStateApi().WithOpenApi();`

### Phase 6: Integration Tests

**Files to Create:**
- `tests/Todo.Api.Tests/Endpoints/StoryRootEndpointsTests.cs`
- `tests/Todo.Api.Tests/Endpoints/WorldStateEndpointsTests.cs`

**Test Coverage:**
- All HTTP status code scenarios (200, 400, 404, 500)
- Request/response serialization validation
- Correlation ID verification in error responses
- Authentication/user scoping (using existing UserContextService)
- Error handling with service layer failures
- Mock all service dependencies using existing fixture library

### Phase 7: OpenAPI Documentation

**File to Modify:**
- `src/api/wwwroot/openapi.yaml` or update via SwaggerGen configuration

**Key Changes:**
- Add schemas for all request/response DTOs
- Document all new endpoints
- Include error response examples
- Document correlation ID header usage

## Implementation Order

1. **Create Request/Response DTOs** (`src/api/Models/Requests.cs`, `Responses.cs`)
2. **Create CorrelationIdMiddleware** (`src/api/Middleware/CorrelationIdMiddleware.cs`)
3. **Create StoryRootEndpointsExtensions** (`src/api/StoryRootEndpointsExtensions.cs`)
4. **Create WorldStateEndpointsExtensions** (`src/api/WorldStateEndpointsExtensions.cs`)
5. **Update Program.cs** (register middleware and map endpoints)
6. **Create integration tests** (`tests/Todo.Api.Tests/Endpoints/`)
7. **Update OpenAPI documentation** (optional - can be auto-generated by SwaggerGen)

## Key Design Decisions

1. **Extension Method Pattern**: Follow existing `TodoEndpointsExtensions.cs` pattern for consistency
2. **Error Handling**: All errors return `ErrorResponse` with correlation ID
3. **User Scoping**: Services already handle user scoping via `IUserContextService`
4. **Testing**: Use existing `LlmResponseFixtures` and `MockLlmService` patterns
5. **DI Registration**: Services already registered, only need to register middleware
6. **Correlation IDs**: Store in `HttpContext.Items["CorrelationId"]` for access throughout request

## Acceptance Criteria Checklist

- ✅ All 10 endpoints implemented (5 Story Root, 5 World State)
- ✅ Proper HTTP status codes (200, 400, 404, 500)
- ✅ Correlation IDs in all error responses
- ✅ Request/response DTOs match specification
- ✅ User scoping via existing UserContextService
- ✅ Error handling with descriptive messages
- ✅ Integration tests covering all scenarios
- ✅ Zero live LLM calls in tests (using fixtures)
- ✅ Endpoints follow existing code patterns