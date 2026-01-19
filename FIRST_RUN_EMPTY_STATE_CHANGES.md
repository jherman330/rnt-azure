# First-Run / Empty Story Root Handling Changes

## Summary

Updated the application to correctly handle first-run scenarios where no Story Root exists. The backend correctly returns 404 when `current.json` doesn't exist, and the frontend now treats this as an expected empty state rather than an error.

## Backend Changes

**No changes needed** - Backend already correctly implements the contract:
- `GET /api/story-root` returns `404 NotFound` when no Story Root exists
- This is the correct HTTP contract for a missing resource
- Tests already verify this behavior (`GetCurrentStoryRoot_Returns404_WhenStoryRootDoesNotExist`)

## Frontend Changes

### 1. StoryRootService (`src/web/src/services/storyRootService.ts`)

**Change:** Updated `getCurrentStoryRoot()` to return `null` for 404 responses instead of throwing an error.

```typescript
// Before: Threw error on 404
async getCurrentStoryRoot(): Promise<StoryRoot>

// After: Returns null for 404 (expected first-run scenario)
async getCurrentStoryRoot(): Promise<StoryRoot | null>
```

**Rationale:** 404 is an expected response when no Story Root exists (first-run), not an error condition.

### 2. StoryRootContext (`src/web/src/components/storyRootContext.tsx`)

**Change:** Updated `loadCurrentStoryRoot()` to handle `null` as empty state, not an error.

- When service returns `null` (404), sets `currentStoryRoot` to `null` without setting error state
- Only sets error state for actual errors (non-404)

**Rationale:** Distinguishes between "not found" (expected) and actual errors (unexpected).

### 3. StoryRootEditorContainer (`src/web/src/components/StoryRoot/StoryRootEditorContainer.tsx`)

**Change:** Added empty state UI that displays when no Story Root exists and no proposal is active.

- Shows welcome message explaining what a Story Root is
- Provides guidance on how to create the first Story Root
- The existing raw input section serves as the CTA (users can immediately start typing)

**Rationale:** Provides clear guidance for first-time users instead of showing an error or blank screen.

### 4. Mock Service (`src/web/src/__mocks__/storyRootService.ts`)

**Change:** Updated mock to return `null` instead of throwing when no Story Root is set.

**Rationale:** Aligns mock behavior with real service behavior.

### 5. Tests (`src/web/src/services/__tests__/storyRootService.test.ts`)

**Change:** Added test case for 404 handling.

```typescript
it('should return null when Story Root does not exist (404)', async () => {
    mockAxios.onGet('/api/story-root').reply(404);
    const result = await service.getCurrentStoryRoot();
    expect(result).toBeNull();
});
```

**Rationale:** Ensures 404 is correctly handled as empty state.

## Behavior Summary

### Before
1. User visits Story Root page for first time
2. Backend returns 404 (correct)
3. Frontend service throws error
4. Context sets error state
5. UI shows error message ❌

### After
1. User visits Story Root page for first time
2. Backend returns 404 (correct)
3. Frontend service returns `null` (expected)
4. Context sets `currentStoryRoot` to `null` (no error)
5. UI shows empty state with welcome message and guidance ✅

## Test Coverage

### Backend Tests
- ✅ `GetCurrentStoryRoot_Returns404_WhenStoryRootDoesNotExist` - Verifies 404 response
- ✅ `GetCurrentStoryRoot_Returns200_WhenStoryRootExists` - Verifies normal flow
- ✅ `GetCurrentStoryRoot_Returns500_WhenServiceThrowsException` - Verifies error handling

### Frontend Tests
- ✅ `getCurrentStoryRoot should return current Story Root` - Normal flow
- ✅ `getCurrentStoryRoot should return null when Story Root does not exist (404)` - **NEW** - Empty state
- ✅ `getCurrentStoryRoot should throw error with correlation ID when request fails with non-404 error` - Error handling

## User Experience

### First-Run Scenario
1. User navigates to Story Root page
2. Sees welcome message: "Welcome to Story Root"
3. Sees explanation: "You don't have a Story Root yet. Create your first Story Root by entering your narrative ideas below."
4. Sees guidance: "A Story Root defines your story's genre, tone, and thematic pillars..."
5. Can immediately start typing in the raw input field
6. After proposing and committing, sees the Story Root display

### Existing Story Root Scenario
1. User navigates to Story Root page
2. Sees current Story Root displayed
3. Can propose updates using raw input
4. Normal workflow continues

### Error Scenario
1. If a real error occurs (network failure, 500, etc.)
2. Error message is displayed with correlation ID
3. User can retry or contact support

## Files Modified

1. `src/web/src/services/storyRootService.ts` - Handle 404 as null
2. `src/web/src/components/storyRootContext.tsx` - Handle null as empty state
3. `src/web/src/components/StoryRoot/StoryRootEditorContainer.tsx` - Add empty state UI
4. `src/web/src/__mocks__/storyRootService.ts` - Update mock return type
5. `src/web/src/services/__tests__/storyRootService.test.ts` - Add 404 test case

## No Changes Required

- Backend endpoints (already correct)
- Backend service layer (already returns null correctly)
- Backend repository layer (already returns null correctly)
- Backend tests (already cover 404 correctly)
