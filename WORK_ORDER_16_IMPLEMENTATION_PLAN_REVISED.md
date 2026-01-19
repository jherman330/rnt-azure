# Work Order 16 - Implementation Plan (Revised)

## Overview

**Purpose:** Refactor `StoryRootService.ProposeStoryRootMergeAsync` to route prompt construction through the proper architectural layers: `StoryRootPromptBuilder` → `PromptFactory` → `ILlmService`.

**Scope:** 
- ✅ Backend service refactoring only
- ❌ No API contract changes
- ❌ No frontend changes
- ❌ No new components or services

## Current Implementation

### StoryRootService.ProposeStoryRootMergeAsync (Current - Lines 64-126)

**Current Architecture (Incorrect):**
```
StoryRootService → IPromptTemplateService → Manual String Replacement → ILlmService
```

**Current Code:**
```csharp
// Get prompt template (using version "1.0" for Phase-0)
var promptTemplate = await _promptTemplateService.GetPromptTemplateAsync("story_root_merge", "1.0");

// Replace template placeholders
var prompt = promptTemplate
    .Replace("{current_story_root}", currentStoryRootJson)
    .Replace("{user_input}", rawInput);

// Call LLM service
var llmResponse = await _llmService.InvokeAsync(prompt);
```

**Issues:**
- Bypasses `StoryRootPromptBuilder` (domain logic layer)
- Bypasses `PromptFactory` (generic template assembly)
- Uses manual string replacement instead of structured variable substitution
- Directly couples service to template names and versions

## Target Implementation

### Target Architecture (Correct)
```
StoryRootService → StoryRootPromptBuilder → PromptFactory → ILlmService
```

**Target Flow:**
1. Load current Story Root (if exists)
2. **Call `StoryRootPromptBuilder.PrepareStoryRootMergeAsync()`** to get `PromptInput`
3. **Call `PromptFactory.AssemblePromptAsync()`** to assemble prompt string
4. Call `ILlmService.InvokeAsync()` with assembled prompt
5. Parse and validate LLM response (unchanged)

## Required Changes

### 1. StoryRootService.cs

**File:** `src/api/Services/StoryRootService.cs`

**Changes:**

#### a) Update Constructor (Lines 22-39)

**Remove:**
```csharp
private readonly IPromptTemplateService _promptTemplateService;
```

**Add:**
```csharp
private readonly IStoryRootPromptBuilder _storyRootPromptBuilder;
private readonly IPromptFactory _promptFactory;
```

**Update constructor parameters:**
```csharp
// OLD:
public StoryRootService(
    IStoryRootRepository repository,
    ILlmService llmService,
    IPromptTemplateService promptTemplateService,  // REMOVE
    IUserContextService userContextService,
    StoryRootValidator validator)

// NEW:
public StoryRootService(
    IStoryRootRepository repository,
    ILlmService llmService,
    IStoryRootPromptBuilder storyRootPromptBuilder,  // ADD
    IPromptFactory promptFactory,                     // ADD
    IUserContextService userContextService,
    StoryRootValidator validator)
```

#### b) Update Constructor Body (Lines 29-31)

**Remove:**
```csharp
_promptTemplateService = promptTemplateService;
```

**Add:**
```csharp
_storyRootPromptBuilder = storyRootPromptBuilder;
_promptFactory = promptFactory;
```

#### c) Refactor ProposeStoryRootMergeAsync (Lines 64-126)

**Replace lines 71-84 with:**
```csharp
// Get current Story Root (if exists)
var currentStoryRoot = await GetCurrentStoryRootAsync();

// Prepare prompt inputs using domain logic
var promptInput = await _storyRootPromptBuilder.PrepareStoryRootMergeAsync(currentStoryRoot, rawInput);

// Assemble prompt string using Prompt Factory
var prompt = await _promptFactory.AssemblePromptAsync(promptInput.TemplateId, promptInput.Variables);
```

**Keep everything else unchanged** (LLM call, parsing, validation remain the same).

### 2. StoryRootServiceTests.cs

**File:** `tests/Todo.Api.Tests/Services/StoryRootServiceTests.cs`

**Changes:**

#### a) Update Test Fixtures (Lines 19-44)

**Remove:**
```csharp
private readonly Mock<IPromptTemplateService> _promptTemplateServiceMock;
```

**Add:**
```csharp
private readonly Mock<IStoryRootPromptBuilder> _storyRootPromptBuilderMock;
private readonly Mock<IPromptFactory> _promptFactoryMock;
```

**Update constructor initialization:**
```csharp
// OLD:
_promptTemplateServiceMock = new Mock<IPromptTemplateService>();
_promptTemplateServiceMock
    .Setup(s => s.GetPromptTemplateAsync("story_root_merge", "1.0"))
    .ReturnsAsync("Template with {current_story_root} and {user_input}");

// NEW:
_storyRootPromptBuilderMock = new Mock<IStoryRootPromptBuilder>();
_promptFactoryMock = new Mock<IPromptFactory>();
```

#### b) Update Service Construction (Lines 34-39)

**Change:**
```csharp
// OLD:
_service = new StoryRootService(
    _repositoryMock.Object,
    _llmServiceMock.Object,
    _promptTemplateServiceMock.Object,  // REMOVE
    _userContextServiceMock.Object,
    _validator);

// NEW:
_service = new StoryRootService(
    _repositoryMock.Object,
    _llmServiceMock.Object,
    _storyRootPromptBuilderMock.Object,  // ADD
    _promptFactoryMock.Object,            // ADD
    _userContextServiceMock.Object,
    _validator);
```

#### c) Update Test Setup for ProposeStoryRootMergeAsync Tests

For tests that need prompt setup, add mocks:

```csharp
// Setup StoryRootPromptBuilder to return PromptInput
_storyRootPromptBuilderMock
    .Setup(b => b.PrepareStoryRootMergeAsync(It.IsAny<StoryRoot?>(), It.IsAny<string>()))
    .ReturnsAsync(new PromptInput
    {
        TemplateId = "story_root_merge",  // Or actual template ID from StoryRootTemplateSelector
        Variables = new Dictionary<string, string>
        {
            { "current_story_root", "..." },  // Serialized JSON or null
            { "user_input", rawInput }
        }
    });

// Setup PromptFactory to return assembled prompt
_promptFactoryMock
    .Setup(f => f.AssemblePromptAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
    .ReturnsAsync("Assembled prompt string");
```

**Note:** Most tests only need to verify the LLM response is parsed correctly. The prompt construction verification can be minimal (just verify the mocks are called).

#### d) Update Prompt Content Verification Tests

**Tests affected:**
- `ProposeStoryRootMergeAsync_WithCurrentStoryRoot_IncludesInPrompt` (Lines 194-219)
- `ProposeStoryRootMergeAsync_NoCurrentStoryRoot_UsesNullInPrompt` (Lines 223-241)

**Change from:**
```csharp
// OLD - Verifies LLM service received string containing content
_llmServiceMock.Verify(s => s.InvokeAsync(
    It.Is<string>(p => p.Contains("story-existing"))), Times.Once);
```

**Change to:**
```csharp
// NEW - Verifies StoryRootPromptBuilder was called with correct StoryRoot
_storyRootPromptBuilderMock.Verify(b => b.PrepareStoryRootMergeAsync(
    It.Is<StoryRoot?>(sr => sr != null && sr.StoryRootId == "story-existing"),
    It.Is<string>(input => input == rawInput)), Times.Once);
```

Or verify that `PromptFactory` received variables containing the StoryRoot data:

```csharp
// Alternative: Verify PromptFactory received correct variables
_promptFactoryMock.Verify(f => f.AssemblePromptAsync(
    It.IsAny<string>(),
    It.Is<Dictionary<string, string>>(vars => 
        vars.ContainsKey("current_story_root") && 
        vars["current_story_root"].Contains("story-existing"))), Times.Once);
```

## Testing Strategy

### Unit Tests

**Keep existing test coverage:**
- ✅ LLM response parsing tests (unchanged - still verify JSON parsing)
- ✅ Validation tests (unchanged - still verify schema validation)
- ✅ Error handling tests (unchanged - still verify exceptions)

**Update these tests:**
- Update prompt construction verification tests to verify `StoryRootPromptBuilder` and `PromptFactory` calls instead of `IPromptTemplateService` calls
- Remove tests that specifically verify template string format (that's now in `PromptFactory` tests)

**Add new test mocks:**
- Mock `IStoryRootPromptBuilder` to return `PromptInput` objects
- Mock `IPromptFactory` to return assembled prompt strings

### Integration Tests

If integration tests exist for `StoryRootService`, they should continue to work unchanged since:
- The service method signature is unchanged
- The API contract is unchanged
- Only internal implementation changes

## Dependency Injection

### Program.cs

**Verify these registrations exist (should already be present):**
```csharp
// Should already exist (Line 54):
builder.Services.AddPromptFactory(builder.Configuration);

// Should already exist (Line 57):
builder.Services.AddStoryRootPromptBuilder();
```

**No changes needed** - registrations already exist.

## Verification Checklist

### Before Refactoring

- [ ] Verify `IStoryRootPromptBuilder` is registered in DI (`Program.cs` line 57)
- [ ] Verify `IPromptFactory` is registered in DI (`Program.cs` line 54)
- [ ] Review existing `StoryRootService` tests to understand test patterns
- [ ] Review `StoryRootPromptBuilder` implementation to understand what it returns

### After Refactoring

- [ ] `StoryRootService` constructor updated (removed `IPromptTemplateService`, added `IStoryRootPromptBuilder` and `IPromptFactory`)
- [ ] `ProposeStoryRootMergeAsync` uses `StoryRootPromptBuilder` → `PromptFactory` → `ILlmService` flow
- [ ] All existing `StoryRootServiceTests` pass (after mock updates)
- [ ] No compilation errors
- [ ] Manual test: Story Root proposal still works end-to-end via API
- [ ] Verify `WorldStateService` still works (it still uses `IPromptTemplateService` directly - that's OK, out of scope)

## Expected Behavior

**Behavior should be identical:**
- Same API endpoint: `POST /api/story-root/propose-merge`
- Same request format: `{ "raw_input": "..." }`
- Same response format: `{ "proposal": {...}, "current": {...} }`
- Same error handling
- Same validation

**Only internal wiring changes:**
- Prompt construction now goes through proper architectural layers
- Better separation of concerns
- Domain logic (template selection, variable preparation) is in `StoryRootPromptBuilder`
- Generic template assembly is in `PromptFactory`

## Files Modified

1. **`src/api/Services/StoryRootService.cs`**
   - Constructor: Remove `IPromptTemplateService`, Add `IStoryRootPromptBuilder` and `IPromptFactory`
   - `ProposeStoryRootMergeAsync`: Replace template loading with builder/factory chain

2. **`tests/Todo.Api.Tests/Services/StoryRootServiceTests.cs`**
   - Update test fixtures to mock `IStoryRootPromptBuilder` and `IPromptFactory` instead of `IPromptTemplateService`
   - Update prompt verification tests to verify builder/factory calls

## Files NOT Modified

- `src/api/StoryRootEndpointsExtensions.cs` - Endpoint unchanged
- `src/api/Models/Requests.cs` - Request model unchanged
- `src/api/Models/Responses.cs` - Response model unchanged
- `src/web/...` - All frontend code unchanged
- `src/api/Program.cs` - DI registrations already correct
- `src/api/Services/WorldStateService.cs` - Out of scope (still uses `IPromptTemplateService`)

## Risk Assessment

**Low Risk:**
- ✅ Service method signature unchanged (internal only)
- ✅ API contract unchanged
- ✅ Existing tests provide safety net (after mock updates)
- ✅ `StoryRootPromptBuilder` and `PromptFactory` already exist and are tested
- ✅ DI registrations already in place

**Mitigation:**
- Update tests alongside service refactoring
- Run full test suite after changes
- Verify end-to-end API call still works

## Summary

This is a **focused refactoring** that improves architectural layering without changing behavior. The refactoring ensures prompt construction follows the intended design pattern:

**Before:** Service directly loads templates and does string replacement  
**After:** Service uses domain-specific builder and generic factory for proper separation of concerns

No new functionality. No API changes. Just cleaner internal wiring.
