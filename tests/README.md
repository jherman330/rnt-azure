# ToDo Application Tests

This directory contains C# unit tests for the API service layer.

## Run Tests

To run all C# unit tests:

```bash
cd tests/Todo.Api.Tests
dotnet test
```

To run only FastLocal tests (for pre-commit):

```bash
cd tests/Todo.Api.Tests
dotnet test --filter "Category=FastLocal"
```

## Test Structure

- **Services/** - Service layer unit tests
- **Fixtures/** - Test data fixtures
- **Mocks/** - Mock implementations for testing
- **TestUtilities/** - Test helper utilities
