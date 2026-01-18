# Work Order 12 Implementation Plan - Generic LLM Backend Service

## Overview
Implement a generic, reusable LLM backend service that provides a clean abstraction for invoking live LLM providers. This service is completely domain-agnostic and focuses solely on the technical capability of making LLM API calls with proper configuration, error handling, and testing support.

## Current Codebase Status

### ✅ Already Implemented (from prior work orders)
- **LLM Service Interface**: `ILlmService` exists with domain-specific methods (`ProposeStoryRootMergeAsync`, `ProposeWorldStateMergeAsync`)
- **Placeholder Implementation**: `PlaceholderLlmService` throws `NotImplementedException`
- **Mock Implementation**: `MockLlmService` in test project implements current interface
- **Service Dependencies**: `StoryRootService` and `WorldStateService` use `ILlmService` via dependency injection
- **Configuration**: `.NET` configuration system with `appsettings.json` files
- **Logging**: Application Insights integration in `Program.cs`
- **Correlation IDs**: `CorrelationIdMiddleware` exists for request tracking
- **Dependency Injection**: Services registered in `Program.cs`

### ❌ Missing for Work Order 12
- Generic `InvokeAsync(string prompt)` method in `ILlmService`
- `OpenAILlmService` implementation for production use
- `MockLlmService` in source code (currently only in test project)
- Configuration classes for LLM settings
- Custom exception types for LLM errors
- `IHttpClientFactory` registration
- Unit and integration tests for generic LLM service

## Implementation Plan

### Phase 1: Update ILlmService Interface

**Files to Modify:**
- `src/api/Services/ILlmService.cs`

**Key Changes:**
- Replace domain-specific methods (`ProposeStoryRootMergeAsync`, `ProposeWorldStateMergeAsync`) with single generic method:
  ```csharp
  Task<string> InvokeAsync(string prompt);
  ```
- Update XML documentation to reflect generic, domain-agnostic nature
- Remove references to "Story Root" and "World State" from interface documentation

**Rationale:** This aligns the interface with the blueprint for a generic LLM service that is completely domain-agnostic.

---

### Phase 2: Update Service Layer to Use New Interface

**Files to Modify:**
- `src/api/Services/StoryRootService.cs` (line 89)
- `src/api/Services/WorldStateService.cs` (line 90)

**Key Changes:**

1. **StoryRootService.cs** - Update line 89:
   - **Before:** `llmResponse = await _llmService.ProposeStoryRootMergeAsync(prompt, "1.0");`
   - **After:** `llmResponse = await _llmService.InvokeAsync(prompt);`
   - Remove the hardcoded `"1.0"` version parameter (no longer needed by generic interface)

2. **WorldStateService.cs** - Update line 90:
   - **Before:** `llmResponse = await _llmService.ProposeWorldStateMergeAsync(prompt, "1.0");`
   - **After:** `llmResponse = await _llmService.InvokeAsync(prompt);`
   - Remove the hardcoded `"1.0"` version parameter (no longer needed by generic interface)

**Note:** Both services already construct the complete prompt using `IPromptTemplateService` and pass it to the LLM service. This change only updates the method call - no workflow or responsibility changes.

**Rationale:** These services must be updated to maintain compilation after the interface change. The change is minimal and preserves existing behavior.

---

### Phase 3: Remove PlaceholderLlmService

**Files to Modify:**
- `src/api/Services/PlaceholderLlmService.cs` - Delete this file

**Rationale:** This placeholder implementation is no longer needed as it will be replaced by `MockLlmService` (for testing) and `OpenAILlmService` (for production).

---

### Phase 4: Create Configuration Classes and Constants

**Files to Create:**
- `src/api/Models/LlmServiceConfiguration.cs`
- `src/api/LlmServiceConstants.cs`

**Key Changes:**

1. **LlmServiceConfiguration.cs:**
   ```csharp
   public class LlmServiceConfiguration
   {
       public string ApiKey { get; set; } = string.Empty;
       public string Endpoint { get; set; } = string.Empty;
       public string Model { get; set; } = string.Empty;
   }
   ```

2. **LlmServiceConstants.cs:**
   ```csharp
   public static class LlmServiceConstants
   {
       public const string ConfigurationSection = "OpenAI";
       public const string ApiKeyKey = "OpenAI:ApiKey";
       public const string EndpointKey = "OpenAI:Endpoint";
       public const string ModelKey = "OpenAI:Model";
   }
   ```

**Rationale:** Centralizes configuration management and eliminates magic strings.

---

### Phase 5: Create Custom Exception Types

**Files to Create:**
- `src/api/LlmServiceException.cs` (or `Exceptions/LlmServiceException.cs`)

**Key Changes:**
- Create base exception: `LlmServiceException` (inherits from `Exception`)
- Create derived exceptions:
  - `LlmNetworkException` - For network/HTTP errors
  - `LlmAuthenticationException` - For authentication failures (401)
  - `LlmRateLimitException` - For rate limiting (429)
- Include correlation ID property in base exception
- Include meaningful error messages

**Rationale:** Provides specific exception types for common LLM failure modes, enabling targeted exception handling by callers.

---

### Phase 6: Create MockLlmService for Testing

**Files to Create:**
- `src/api/Services/MockLlmService.cs`

**Key Changes:**
- Implement `ILlmService` interface
- Return predefined string responses based on simple fixture patterns
- Configurable via constructor or properties
- Support deterministic testing without live API calls

**Note:** This is a production-code mock (for development environments). The test project's `MockLlmService` will be updated separately in Phase 12.

**Rationale:** Enables development and local testing without live API calls.

---

### Phase 7: Create OpenAILlmService Implementation

**Files to Create:**
- `src/api/Services/OpenAILlmService.cs`

**Key Changes:**
- Implement `ILlmService` interface
- Use `IHttpClientFactory` to get `HttpClient` instances
- Inject `LlmServiceConfiguration` for API settings
- Make HTTP POST requests to OpenAI API
- Serialize request payload (prompt) and deserialize response
- Map HTTP errors to custom exception types (`LlmNetworkException`, `LlmAuthenticationException`, `LlmRateLimitException`)
- Include structured logging with correlation IDs (if available via `IHttpContextAccessor` or other mechanism)
- Use Application Insights logging for performance metrics

**Implementation Notes:**
- Use `HttpClient` from `IHttpClientFactory` for thread-safe HTTP calls
- Handle OpenAI API response format (extract text from response JSON)
- Log request/response (sanitized - no API keys in logs)
- Propagate exceptions directly (no retry logic per blueprint)

**Rationale:** Provides the concrete production implementation for invoking OpenAI LLM API.

---

### Phase 8: Update Program.cs for Dependency Injection

**Files to Modify:**
- `src/api/Program.cs`

**Key Changes:**

1. Register `IHttpClientFactory`:
   ```csharp
   builder.Services.AddHttpClient();
   ```

2. Bind configuration section:
   ```csharp
   builder.Services.Configure<LlmServiceConfiguration>(
       builder.Configuration.GetSection(LlmServiceConstants.ConfigurationSection));
   ```

3. Conditionally register LLM service implementation:
   - For Development/Testing environments: Register `MockLlmService` as `ILlmService`
   - For Production environments: Register `OpenAILlmService` as `ILlmService`
   - Environment detection: `Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")`

4. Remove existing registration of `PlaceholderLlmService`

**Example Registration Logic:**
```csharp
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == "Development" || environment == "Testing")
{
    builder.Services.AddSingleton<ILlmService, MockLlmService>();
}
else
{
    builder.Services.AddSingleton<ILlmService>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var config = sp.GetRequiredService<IOptions<LlmServiceConfiguration>>().Value;
        return new OpenAILlmService(httpClientFactory, config);
    });
}
```

**Rationale:** Sets up dependency injection for the new LLM services with environment-specific implementations and proper HttpClient management.

---

### Phase 9: Update Configuration Files

**Files to Modify:**
- `src/api/appsettings.json`
- `src/api/appsettings.Development.json`
- `src/api/appsettings.Production.json`

**Key Changes:**

1. **appsettings.json** - Add base configuration:
   ```json
   {
     "OpenAI": {
       "ApiKey": "",
       "Endpoint": "",
       "Model": ""
     }
   }
   ```

2. **appsettings.Development.json** - Add development settings:
   ```json
   {
     "OpenAI": {
       "ApiKey": "placeholder-for-local-dev",
       "Endpoint": "https://api.openai.com/v1/chat/completions",
       "Model": "gpt-4"
     }
   }
   ```
   **Note:** In Development, `MockLlmService` will be used, so these values may not be used, but they provide a template.

3. **appsettings.Production.json** - Add production settings:
   ```json
   {
     "OpenAI": {
       "ApiKey": "",  // Should come from environment variables or Azure Key Vault
       "Endpoint": "https://api.openai.com/v1/chat/completions",
       "Model": "gpt-4"
     }
   }
   ```
   **Note:** In production, API keys should be sourced from environment variables or Azure Key Vault references (not hardcoded).

**Rationale:** Provides structured configuration locations for OpenAI API settings across environments.

---

### Phase 10: Update Test Project MockLlmService

**Files to Modify:**
- `tests/Todo.Api.Tests/Mocks/MockLlmService.cs`

**Key Changes:**
- Update to implement the new `ILlmService.InvokeAsync(string prompt)` method
- Remove `ProposeStoryRootMergeAsync` and `ProposeWorldStateMergeAsync` methods
- Maintain existing fixture-based response mapping, but adapt to work with generic prompts
- Keep all existing test helper methods (`SetResponse`, `SetResponseFactory`, `ResetToDefaults`, etc.) but adapt them to work with prompt strings

**Implementation Note:** The mock may need to detect which scenario (story_root vs world_state) based on prompt content, or tests can be updated to set responses differently. The simplest approach is to have a default response that works for both, or use the first configured response.

**Rationale:** Adapts the existing test mock to the new generic interface while preserving deterministic test behavior.

---

### Phase 11: Update TestWebApplicationFactory

**Files to Modify:**
- `tests/Todo.Api.Tests/Endpoints/TestWebApplicationFactory.cs`

**Key Changes:**
- Ensure `MockLlmService` is registered as `ILlmService` for integration tests
- Remove any references to old `PlaceholderLlmService` or domain-specific methods
- Verify the mock service is correctly injected into the test application

**Rationale:** Ensures integration tests consistently use the mock LLM service and do not make live API calls.

---

### Phase 12: Update Service Layer Tests

**Files to Modify:**
- `tests/Todo.Api.Tests/Services/StoryRootServiceTests.cs`
- `tests/Todo.Api.Tests/Services/WorldStateServiceTests.cs`

**Key Changes:**

1. **StoryRootServiceTests.cs:**
   - Update all `_llmServiceMock.Setup(s => s.ProposeStoryRootMergeAsync(...))` calls to `_llmServiceMock.Setup(s => s.InvokeAsync(...))`
   - Remove the `"1.0"` version parameter from all mock setups
   - Verify calls using `_llmServiceMock.Verify(s => s.InvokeAsync(...), Times.Once)`
   - All test logic remains the same - only the mock setup/verification changes

2. **WorldStateServiceTests.cs:**
   - Update all `_llmServiceMock.Setup(s => s.ProposeWorldStateMergeAsync(...))` calls to `_llmServiceMock.Setup(s => s.InvokeAsync(...))`
   - Remove the `"1.0"` version parameter from all mock setups
   - Verify calls using `_llmServiceMock.Verify(s => s.InvokeAsync(...), Times.Once)`
   - All test logic remains the same - only the mock setup/verification changes

**Note:** There are approximately 15-20 test methods in each file that use LLM service mocks. All need to be updated to use `InvokeAsync` instead of the domain-specific methods.

**Rationale:** These tests must be updated to use the new interface, otherwise they will fail to compile. This is a low-risk change that preserves all test logic.

---

### Phase 13: Add Unit Tests for LLM Service

**Files to Create:**
- `tests/Todo.Api.Tests/Services/LlmServiceTests.cs`

**Key Changes:**
- Create unit tests for `MockLlmService`:
  - Test `InvokeAsync` returns configured responses
  - Test response factory functions
  - Test default responses
- Create unit tests for `OpenAILlmService`:
  - Mock `HttpClient` to test HTTP request/response handling
  - Test successful API calls
  - Test error mapping (network errors → `LlmNetworkException`, 401 → `LlmAuthenticationException`, 429 → `LlmRateLimitException`)
  - Test configuration validation
- Use `Moq` for mocking dependencies

**Test Categories:**
- Core functionality (InvokeAsync)
- Configuration validation
- Error handling and exception propagation
- Logging (verify logs are written, sanitized)

**Rationale:** Ensures the core LLM service functionality, error handling, and configuration are correctly implemented and thoroughly tested in isolation.

---

### Phase 14: Add Integration Tests for LLM Service

**Files to Create:**
- `tests/Todo.Api.Tests/Endpoints/LlmServiceIntegrationTests.cs`

**Key Changes:**
- Use `TestWebApplicationFactory` to test `ILlmService` through the application's dependency injection container
- Verify that `MockLlmService` is resolved correctly (not `OpenAILlmService`) in test environment
- Test that `InvokeAsync` method behaves as expected within the application context
- Verify configuration binding works correctly
- Test error propagation through the DI container

**Rationale:** Verifies the end-to-end integration of the LLM service within the application context, using the mock to avoid external dependencies in automated tests.

---

### Phase 15: Verify Package Dependencies

**Files to Check/Modify:**
- `src/api/Todo.Api.csproj`

**Key Changes:**
- Verify if `System.Net.Http.Json` package reference is needed for .NET 8.0
- .NET 8.0 includes `System.Net.Http.Json` in the base class library, but explicit package reference may be desired for version pinning
- If adding, use appropriate version: `<PackageReference Include="System.Net.Http.Json" Version="8.0.0" />`

**Note:** `IHttpClientFactory` is included in `Microsoft.AspNetCore.App` framework reference, so no additional package needed.

**Rationale:** Ensures required external libraries for HTTP communication and JSON handling are available.

---

## Implementation Notes

### Thread Safety
- `OpenAILlmService` should use `IHttpClientFactory` for thread-safe `HttpClient` instances
- No shared mutable state in service implementations

### Error Handling
- Direct exception propagation (no retry logic per blueprint)
- Map OpenAI API errors to custom exception types
- Include correlation IDs in exceptions when available

### Logging
- Use structured logging with correlation IDs
- Sanitize logs (no API keys in log output)
- Log request/response summaries for observability
- Use Application Insights for performance metrics

### Security
- API keys must come from configuration, never hardcoded
- Support Azure Key Vault references in production configuration
- No API keys in source code or logs

### Testing Strategy
- Zero live LLM calls in automated test suite
- All tests use `MockLlmService`
- Tests verify behavior, not implementation details where possible

---

## Acceptance Criteria Verification

After implementation, verify:

✅ `ILlmService` interface has single `InvokeAsync(string prompt)` method  
✅ `OpenAILlmService` implements interface with HttpClient  
✅ `MockLlmService` implements interface for testing  
✅ Configuration bound from `appsettings.json`  
✅ Environment-based service registration (Mock in Dev/Test, OpenAI in Production)  
✅ Custom exception types exist and are used  
✅ Service layer (`StoryRootService`, `WorldStateService`) uses new interface  
✅ All existing tests pass (after updates)  
✅ New unit tests for LLM service pass  
✅ New integration tests for LLM service pass  
✅ Zero compilation errors  
✅ No API keys in code or logs  

---

## Out of Scope (Per Work Order Requirements)

- Domain-specific integrations (Story Root, World State, characters, etc.) - handled by service layer
- Prompt template management - remains in `IPromptTemplateService`
- Response parsing or validation - remains in service layer
- Business logic or UX concerns - handled by existing services
- Advanced retry logic or resilience patterns - direct error propagation only
- Multi-provider support beyond OpenAI - single provider focus
- End-to-end workflow integration - integration happens via service layer updates only
