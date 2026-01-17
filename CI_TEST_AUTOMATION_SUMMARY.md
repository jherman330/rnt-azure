# CI Test Automation Summary

## CI Scope and Purpose

This document defines the **Continuous Integration (CI) quality gates** that run on push to `main`/`master` branches. CI is strictly limited to **code quality validation only** and does not include deployment concerns.

## CI Pipeline Execution

### Trigger
- Runs automatically on push to `main` or `master` branches
- GitHub Actions: `.github/workflows/azure-dev.yml`
- Azure DevOps: `.azdo/pipelines/azure-dev.yml`

### CI Steps (In Order)
1. **Restore dependencies** - Restores NuGet packages for all C# projects
2. **Build** - Builds all C# projects with `--no-restore`
3. **Run C# Tests** - Executes all backend C# unit tests using `dotnet test --no-build`

### What CI Does NOT Include
- ❌ **Azure authentication** - Out of scope for CI
- ❌ **Azure provisioning** - Out of scope for CI
- ❌ **Azure deployment** - Treated as separate CD concern, not part of push-based CI
- ❌ **Playwright / E2E tests** - Not part of CI pipeline
- ❌ **Frontend build or tests** - Backend C# tests only

## Test Execution Tiers

### Pre-commit (FastLocal)
- **55 C# unit tests** with `[Trait("Category", "FastLocal")]`
- Runs locally before commits (via `.git/hooks/pre-commit`)
- Runs in < 1 second
- Fully mocked, deterministic
- Blocks commits if tests fail

### CI (All Backend C# Tests)
- **55 C# unit tests** (all categories)
- Runs on every push to main/master
- Fails pipeline if any test fails
- **Only backend C# tests** - No frontend or E2E tests

## CI/CD Separation

**CI (Current Scope):**
- Code quality validation only
- Restore, build, and all backend C# tests
- Runs on push to main/master

**CD (Future Concern):**
- Azure authentication, provisioning, and deployment are explicitly separated from push-based CI
- Azure deployment will be handled as a separate Continuous Deployment pipeline
- No Azure infrastructure steps are included in the CI pipeline

## Verification

CI pipeline configuration:
- ✅ Minimal CI scope: restore, build, tests only
- ✅ No Azure authentication or deployment steps in CI
- ✅ No Playwright/E2E tests in CI
- ✅ All backend C# tests run (55 total)
- ✅ Pipeline fails on test failures

## Reference

- Pre-commit hook: `.git/hooks/pre-commit` (runs FastLocal tests only)
- Pre-commit documentation: `tests/PRE_COMMIT_HOOK.md`
- Test categorization: `tests/TEST_CATEGORIZATION.md`
