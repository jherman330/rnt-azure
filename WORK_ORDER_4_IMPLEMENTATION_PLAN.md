# Work Order 4 Implementation Plan - Aligned with Existing Frontend Conventions

## Overview
Implement React components for Story Root management, providing authors with a complete workflow to input raw narrative ideas, review LLM proposals, edit proposals, and approve changes. This implementation follows existing frontend patterns and conventions.

## Current Codebase Status

### ✅ Already Implemented (from prior work orders)
- **Backend API**: All Story Root endpoints implemented in `StoryRootEndpointsExtensions.cs`
  - `GET /api/story-root` - Current Story Root
  - `GET /api/story-root/versions/{versionId}` - Specific version
  - `GET /api/story-root/versions` - Version list
  - `POST /api/story-root/propose-merge` - Generate proposal
  - `POST /api/story-root/commit` - Commit approved version
- **Backend Models**: `StoryRoot`, `VersionMetadata`, `ProposalResponse<T>`, `CommitResponse<T>`, `ErrorResponse`
- **Error Handling**: Correlation IDs in all error responses
- **Frontend Infrastructure**: React + TypeScript, Fluent UI, Axios, React Router

### ❌ Missing for Work Order 4
- Frontend TypeScript models/interfaces for Story Root
- API service client using RestService pattern
- React components for Story Root workflow
- State management hooks and context
- Component tests

## Key Design Decisions (Aligned with Existing Patterns)

1. **API Client**: Use `RestService` base class pattern with Axios (like `ListService`, `ItemService`)
2. **UI Components**: Use Fluent UI components (`Stack`, `TextField`, `PrimaryButton`, etc.) like `TodoItemDetailPane`
3. **Directory Structure**: 
   - `src/models/` for TypeScript interfaces (not `src/types/`)
   - `src/services/` for API clients (not `src/api/`)
   - `src/components/` for React components
   - Context files in `src/components/` (like `todoContext.ts`)
4. **State Management**: React Context API with hooks (like `TodoContext`)
5. **Styling**: Fluent UI component styling (no custom CSS modules needed)

## Implementation Plan

### Phase 1: TypeScript Models and Interfaces

**Files to Create:**
- `src/web/src/models/storyRoot.ts` - Story Root interface matching backend model
- `src/web/src/models/versionMetadata.ts` - Version metadata interface
- `src/web/src/models/apiContracts.ts` - Request/response interfaces
- Update `src/web/src/models/index.ts` - Export new models

**Key Changes:**
- Define `StoryRoot` interface with `story_root_id`, `genre`, `tone`, `thematic_pillars`, `notes`
- Create `VersionMetadata` interface matching backend model
- Create `ProposalResponse<T>` and `CommitResponse<T>` generic interfaces
- Create `ErrorResponse` interface with `error` and `correlation_id`
- Ensure exact alignment with backend C# models (camelCase JSON properties)

**Example Structure:**
```typescript
// src/web/src/models/storyRoot.ts
export interface StoryRoot {
    story_root_id: string;
    genre: string;
    tone: string;
    thematic_pillars: string;
    notes?: string;
}
```

### Phase 2: API Service Client

**Files to Create:**
- `src/web/src/services/storyRootService.ts` - Story Root API service extending RestService pattern

**Key Changes:**
- Create `StoryRootService` class (not extending RestService, but using similar Axios pattern)
- Implement typed methods for each Story Root endpoint:
  - `getCurrentStoryRoot(): Promise<StoryRoot>`
  - `getStoryRootVersion(versionId: string): Promise<StoryRoot>`
  - `listStoryRootVersions(): Promise<VersionMetadata[]>`
  - `proposeStoryRootMerge(rawInput: string): Promise<ProposalResponse<StoryRoot>>`
  - `commitStoryRootVersion(storyRoot: StoryRoot): Promise<CommitResponse<StoryRoot>>`
- Use Axios instance with base URL from config
- Handle error responses and extract correlation IDs
- Use proper TypeScript typing throughout

**Note**: Story Root endpoints don't follow standard REST CRUD pattern, so we'll create a custom service class using Axios directly (similar to how RestService uses Axios internally).

### Phase 3: React Context and State Management

**Files to Create:**
- `src/web/src/components/storyRootContext.ts` - Story Root context provider
- `src/web/src/hooks/useStoryRoot.ts` - Custom hook for Story Root operations
- `src/web/src/hooks/useApiCall.ts` - Generic hook for API calls with loading/error states

**Key Changes:**
- Create `StoryRootContext` following `TodoContext` pattern
- Implement context provider with state for:
  - Current Story Root
  - Proposal (if any)
  - Loading states
  - Error states
- Create `useStoryRoot` hook for accessing context and operations
- Create `useApiCall` hook for standardized API call handling with loading/error states
- Use `useState` and `useContext` only (no external state libraries)

### Phase 4: Display Components

**Files to Create:**
- `src/web/src/components/StoryRoot/StoryRootDisplay.tsx` - Read-only Story Root display
- `src/web/src/components/StoryRoot/LoadingSpinner.tsx` - Reusable loading component
- `src/web/src/components/StoryRoot/ErrorDisplay.tsx` - Error display with correlation ID

**Key Changes:**
- Implement read-only display using Fluent UI `Stack` and `Text` components
- Display genre, tone, thematic_pillars, notes sections
- Handle optional fields gracefully (notes)
- Use semantic HTML structure with proper accessibility
- Clean, readable formatting for narrative blob content
- Loading spinner using Fluent UI `Spinner` component
- Error display showing error message and correlation ID using Fluent UI `MessageBar`

**Component Pattern:**
```typescript
// Following TodoItemDetailPane pattern
import { Stack, Text, MessageBar, MessageBarType, Spinner } from '@fluentui/react';
```

### Phase 5: Editable Components

**Files to Create:**
- `src/web/src/components/StoryRoot/StoryRootEditor.tsx` - Editable Story Root form
- `src/web/src/components/StoryRoot/StoryRootProposalEditor.tsx` - Editable proposal form

**Key Changes:**
- Implement editable form using Fluent UI `TextField` components (multiline for text areas)
- Add proper TypeScript typing for onChange handlers
- Implement field validation and error display
- Handle controlled component state properly
- Use Fluent UI `Stack` for layout
- Use Fluent UI `PrimaryButton` and `DefaultButton` for actions

**Component Pattern:**
```typescript
// Following TodoItemDetailPane pattern with TextField components
import { Stack, TextField, PrimaryButton, DefaultButton } from '@fluentui/react';
```

### Phase 6: Main Editor Workflow Component

**Files to Create:**
- `src/web/src/components/StoryRoot/StoryRootEditorContainer.tsx` - Main editor orchestrating workflow

**Key Changes:**
- Implement complete workflow: input → proposal → review → approval
- Add side-by-side comparison interface using Fluent UI `Stack` with horizontal layout
- Current Story Root display (read-only) on left
- LLM proposal display (editable) on right
- Implement loading states for all async operations using `Spinner`
- Add success feedback using Fluent UI `MessageBar`
- Error handling with correlation ID display
- "Propose Story Root Update" button to call propose-merge API
- "Approve Story Root" button to commit the proposal
- "Discard" button to cancel the proposal and clear input
- Proper state management with useState
- Automatic loading of current Story Root on component mount
- State cleanup after successful approval or discard actions

**Workflow States:**
1. **Initial**: Show current Story Root (if exists) and raw input textarea
2. **Loading Proposal**: Show spinner while LLM generates proposal
3. **Proposal Ready**: Show side-by-side comparison (current vs proposal)
4. **Loading Commit**: Show spinner while committing
5. **Success**: Show success message and refresh current Story Root
6. **Error**: Show error message with correlation ID

### Phase 7: Integration with App Routing

**Files to Modify:**
- `src/web/src/App.tsx` - Add route for Story Root editor
- `src/web/src/layout/sidebar.tsx` - Add navigation link (if needed)

**Key Changes:**
- Add route for Story Root editor page
- Integrate with existing routing structure
- Ensure proper context provider wrapping

### Phase 8: Testing Implementation

**Files to Create:**
- `src/web/src/components/StoryRoot/__tests__/StoryRootDisplay.test.tsx`
- `src/web/src/components/StoryRoot/__tests__/StoryRootEditor.test.tsx`
- `src/web/src/components/StoryRoot/__tests__/StoryRootEditorContainer.test.tsx`
- `src/web/src/services/__tests__/storyRootService.test.ts`
- `src/web/src/__mocks__/storyRootService.ts` - Mock service for tests

**Key Changes:**
- Test component mounting and unmounting behavior
- Validate prop passing and event handling
- Test conditional rendering based on state
- Mock API calls using Jest mocks
- Test error handling and loading state display
- Test TypeScript interface compliance
- Use React Testing Library (if available) or similar
- Test integration with TypeScript interfaces

**Note**: Since no existing test structure found, create test directory structure following common React testing patterns.

## Implementation Order

1. **Create TypeScript Models** (`src/web/src/models/storyRoot.ts`, `versionMetadata.ts`, `apiContracts.ts`)
2. **Create API Service** (`src/web/src/services/storyRootService.ts`)
3. **Create Context and Hooks** (`src/web/src/components/storyRootContext.ts`, `src/web/src/hooks/useStoryRoot.ts`, `useApiCall.ts`)
4. **Create Display Components** (`StoryRootDisplay.tsx`, `LoadingSpinner.tsx`, `ErrorDisplay.tsx`)
5. **Create Editable Components** (`StoryRootEditor.tsx`, `StoryRootProposalEditor.tsx`)
6. **Create Main Workflow Component** (`StoryRootEditorContainer.tsx`)
7. **Integrate with App Routing** (Update `App.tsx`)
8. **Create Tests** (Component and service tests)

## File Structure Summary

```
src/web/src/
├── models/
│   ├── storyRoot.ts (NEW)
│   ├── versionMetadata.ts (NEW)
│   ├── apiContracts.ts (NEW)
│   └── index.ts (UPDATE - export new models)
├── services/
│   ├── storyRootService.ts (NEW)
│   └── __tests__/
│       └── storyRootService.test.ts (NEW)
├── components/
│   ├── storyRootContext.ts (NEW)
│   └── StoryRoot/
│       ├── StoryRootDisplay.tsx (NEW)
│       ├── StoryRootEditor.tsx (NEW)
│       ├── StoryRootProposalEditor.tsx (NEW)
│       ├── StoryRootEditorContainer.tsx (NEW)
│       ├── LoadingSpinner.tsx (NEW)
│       ├── ErrorDisplay.tsx (NEW)
│       └── __tests__/
│           ├── StoryRootDisplay.test.tsx (NEW)
│           ├── StoryRootEditor.test.tsx (NEW)
│           └── StoryRootEditorContainer.test.tsx (NEW)
├── hooks/
│   ├── useStoryRoot.ts (NEW)
│   └── useApiCall.ts (NEW)
├── __mocks__/
│   └── storyRootService.ts (NEW)
└── App.tsx (UPDATE - add route)
```

## Key Implementation Details

### API Service Pattern
```typescript
// src/web/src/services/storyRootService.ts
import axios, { AxiosInstance } from 'axios';
import config from '../config';
import { StoryRoot, VersionMetadata, ProposalResponse, CommitResponse } from '../models';

export class StoryRootService {
    private client: AxiosInstance;

    constructor() {
        this.client = axios.create({
            baseURL: `${config.api.baseUrl}/api/story-root`
        });
    }

    async getCurrentStoryRoot(): Promise<StoryRoot> {
        const response = await this.client.get<StoryRoot>('/');
        return response.data;
    }

    // ... other methods
}
```

### Component Pattern (Fluent UI)
```typescript
// Following TodoItemDetailPane pattern
import { Stack, TextField, PrimaryButton, DefaultButton, Text, Spinner, MessageBar } from '@fluentui/react';

export const StoryRootDisplay: FC<StoryRootDisplayProps> = (props) => {
    return (
        <Stack>
            <Text variant="large">{props.storyRoot.genre}</Text>
            <TextField label="Tone" value={props.storyRoot.tone} readOnly />
            {/* ... */}
        </Stack>
    );
};
```

### Context Pattern
```typescript
// Following TodoContext pattern
import { createContext } from 'react';

export interface StoryRootContextType {
    currentStoryRoot: StoryRoot | null;
    proposal: StoryRoot | null;
    loading: boolean;
    error: ErrorResponse | null;
    // ... methods
}

export const StoryRootContext = createContext<StoryRootContextType>({ /* ... */ });
```

## Acceptance Criteria Checklist

- ✅ Story Root Editor Component with text area for raw input
- ✅ "Propose Story Root Update" button calling propose-merge API
- ✅ Loading states during LLM proposal generation
- ✅ Error handling with correlation ID display
- ✅ Side-by-side comparison interface (current vs proposal)
- ✅ Editable text areas for proposal fields
- ✅ "Approve Story Root" button to commit proposal
- ✅ "Discard" button to cancel proposal
- ✅ Loading states during commit operations
- ✅ Success feedback when changes approved
- ✅ Read-only StoryRootDisplay component
- ✅ Editable StoryRootEditorComponent
- ✅ Proper handling of optional fields (notes)
- ✅ React state management using useState and useContext
- ✅ Automatic loading of current Story Root on mount
- ✅ State cleanup after successful operations
- ✅ TypeScript interfaces matching backend models exactly
- ✅ API client using Axios (following RestService pattern)
- ✅ Fluent UI components throughout
- ✅ Component tests for mounting, props, state, and API integration
- ✅ Error boundary implementation (optional but recommended)

## Out of Scope (Per Work Order)

- World State components (separate work order)
- Version history browsing interface
- Advanced text editing features (rich text, formatting)
- Visual regression testing
- Performance testing beyond basic rendering
- Comprehensive accessibility testing (basic semantic HTML only)
- UX testing and LLM-related behavior testing

## Notes

1. **Fluent UI Usage**: All components should use Fluent UI components for consistency with existing codebase (`TodoItemDetailPane` pattern)
2. **No Custom CSS**: Use Fluent UI component styling, no need for CSS modules
3. **Axios Pattern**: Follow the Axios usage pattern from `RestService`, but create custom service class since Story Root endpoints don't follow standard REST
4. **Context Location**: Place context file in `src/components/` following `todoContext.ts` pattern
5. **Models Location**: Use `src/models/` directory following existing pattern
6. **Services Location**: Use `src/services/` directory following existing pattern
7. **Error Handling**: Extract correlation IDs from error responses and display to user
8. **Authentication**: Azure Static Web Apps handles authentication token forwarding automatically
