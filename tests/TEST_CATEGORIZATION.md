# Test Categorization Summary

This document describes the two-tier test execution strategy for this repository.

## Categories

### FastLocal
- **Purpose**: Fast, deterministic unit tests that run in pre-commit hooks
- **Characteristics**:
  - Unit tests only
  - All dependencies mocked
  - Deterministic (no external dependencies)
  - Fast execution (< 1 second per test)
  - No I/O operations
  - No network calls

### FullCI
- **Purpose**: All tests including slower integration/E2E tests
- **Characteristics**:
  - Integration tests
  - End-to-end tests
  - Tests requiring external dependencies
  - Tests that may be slower
  - Browser-based tests (Playwright)

## Test Breakdown

### FastLocal Tests (55 total)
All C# unit tests using xUnit with `[Trait("Category", "FastLocal")]`:

- **PromptTemplateServiceTests**: 7 tests
  - All unit tests for prompt template retrieval
  - No external dependencies, fully mocked

- **StoryRootServiceTests**: 24 tests
  - All unit tests for Story Root service operations
  - All dependencies mocked (repository, LLM service, etc.)

- **WorldStateServiceTests**: 24 tests
  - All unit tests for World State service operations
  - All dependencies mocked (repository, LLM service, etc.)

### FullCI Tests (1 total)
Playwright E2E test with tag `@FullCI`:

- **todo.spec.ts**: 1 test
  - End-to-end browser test
  - Requires running application
  - Requires browser automation

## Running Tests

### FastLocal Only (Pre-commit)
```bash
# C# tests only
cd tests/Todo.Api.Tests
dotnet test --filter "Category=FastLocal"
```

### All Tests (CI)
```bash
# C# tests
cd tests/Todo.Api.Tests
dotnet test

# Playwright tests
cd tests
npx playwright test
```

## Verification

To verify all tests are categorized:
```bash
# Count FastLocal tests
cd tests/Todo.Api.Tests
dotnet test --filter "Category=FastLocal" --list-tests | findstr /C:"test"

# Count all C# tests (should match FastLocal count)
cd tests/Todo.Api.Tests
dotnet test --list-tests | findstr /C:"test"
```

## Notes

- All 55 C# unit tests are categorized as FastLocal
- The 1 Playwright E2E test is categorized as FullCI
- No tests are uncategorized
- Test categorization is enforced via xUnit traits and Playwright tags
