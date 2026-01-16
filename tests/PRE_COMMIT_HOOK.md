# Pre-commit Hook Setup

This repository uses a two-tier validation strategy. The pre-commit hook runs fast, deterministic checks to provide quick feedback before commits.

## Hook Location

The pre-commit hook template is located at: `.git/hooks/pre-commit.sample`

## Setup Instructions

### Windows (PowerShell)

1. Navigate to the repository root
2. Copy the sample hook:
   ```powershell
   Copy-Item .git/hooks/pre-commit.sample .git/hooks/pre-commit
   ```
3. Make it executable (if needed):
   ```powershell
   # Git Bash or WSL
   chmod +x .git/hooks/pre-commit
   ```

### Linux/macOS

1. Navigate to the repository root
2. Copy the sample hook:
   ```bash
   cp .git/hooks/pre-commit.sample .git/hooks/pre-commit
   ```
3. Make it executable:
   ```bash
   chmod +x .git/hooks/pre-commit
   ```

## What It Does

The pre-commit hook runs the following checks in sequence:

### Backend Validation
```bash
cd tests/Todo.Api.Tests
dotnet test --filter "Category=FastLocal" --no-build
```
This executes only the FastLocal unit tests, which:
- Are fully mocked (no external dependencies)
- Run in < 1 second total
- Provide deterministic results
- Cover all service layer logic

### Frontend Type Checking
```bash
cd src/web
npm run typecheck
```
Runs TypeScript type checking (`tsc --noEmit`) to catch type errors without building.

### Frontend Linting
```bash
npm run lint
```
Runs ESLint to catch code quality and style issues.

**All checks must pass** for the commit to proceed. The hook fails fast on the first error.

## Disabling the Hook

To temporarily bypass the hook for a commit:
```bash
git commit --no-verify -m "your message"
```

**Warning**: Use `--no-verify` sparingly. The hook ensures code quality before commits.

## Troubleshooting

### Hook not running
- Ensure the file is named `pre-commit` (not `pre-commit.sample`)
- Verify it's executable: `ls -l .git/hooks/pre-commit`
- Check that you're in a git repository

### Backend tests failing
- Fix failing tests before committing
- Run tests manually: `cd tests/Todo.Api.Tests && dotnet test --filter "Category=FastLocal"`
- Ensure the project builds: `dotnet build tests/Todo.Api.Tests`

### Frontend type errors
- Fix TypeScript errors shown in the output
- Run manually: `cd src/web && npm run typecheck`
- Check `tsconfig.json` for configuration issues

### Frontend linting errors
- Fix ESLint errors shown in the output
- Run manually: `cd src/web && npm run lint`
- Most linting errors can be auto-fixed: `npm run lint -- --fix`

### Build errors
- The hook uses `--no-build` for backend tests (speed optimization)
- If build is needed, run: `dotnet build tests/Todo.Api.Tests` first
