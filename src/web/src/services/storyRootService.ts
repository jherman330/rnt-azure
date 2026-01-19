import axios, { AxiosInstance, AxiosError } from 'axios';
import config from '../config';
import { StoryRoot, VersionMetadata, ProposalResponse, CommitResponse, ErrorResponse } from '../models';
import { withRetry } from '../utils/retryLogic';
import { classifyApiError, extractErrorDetails, ErrorType } from '../utils/apiErrorHandler';

export class StoryRootService {
    private client: AxiosInstance;
    private readonly baseUrl: string;

    constructor() {
        this.baseUrl = `${config.api.baseUrl}/api/story-root`;
        this.client = axios.create({
            baseURL: this.baseUrl,
            headers: {
                'Content-Type': 'application/json',
            },
        });
    }

    /**
     * Checks if a StoryRoot is an empty default object (all fields empty)
     */
    private isEmptyStoryRoot(storyRoot: StoryRoot | null): boolean {
        if (!storyRoot) return true;
        return (
            !storyRoot.story_root_id &&
            !storyRoot.genre &&
            !storyRoot.tone &&
            !storyRoot.thematic_pillars &&
            (!storyRoot.notes || !storyRoot.notes.trim())
        );
    }

    /**
     * Creates an error message with optional correlation ID
     */
    private createErrorMessage(_baseMessage: string, error: unknown): string {
        const { message, correlationId } = extractErrorDetails(error);
        return correlationId ? `${message} (Correlation ID: ${correlationId})` : message;
    }

    /**
     * GET /api/story-root - Returns the current Story Root for the authenticated user.
     * Returns a default empty StoryRoot object if none exists (treats missing as valid empty initial state).
     * Uses retry logic for transient failures and request de-duplication to prevent redundant calls.
     */
    async getCurrentStoryRoot(): Promise<StoryRoot | null> {
        try {
            const response = await withRetry(
                () => this.client.get<StoryRoot>('/'),
                `${this.baseUrl}/`,
                { maxRetries: 3, initialDelayMs: 500, maxDelayMs: 5000 }
            );

            const storyRoot = response.data;

            // Treat empty default object as null for consistency with existing code
            return this.isEmptyStoryRoot(storyRoot) ? null : storyRoot;
        } catch (error) {
            const classification = classifyApiError(error, `${this.baseUrl}/`);
            
            // Non-actionable errors (like empty state) should not throw
            if (classification.type === ErrorType.NonActionable) {
                return null;
            }
            
            // Throw actionable or transient errors (after retries exhausted)
            throw new Error(this.createErrorMessage('Failed to retrieve current Story Root', error));
        }
    }

    /**
     * GET /api/story-root/versions/{versionId} - Returns a specific version of the Story Root.
     * Uses retry logic for transient failures.
     */
    async getStoryRootVersion(versionId: string): Promise<StoryRoot> {
        if (!versionId || versionId.trim() === '') {
            throw new Error('Version ID is required');
        }

        try {
            const response = await withRetry(
                () => this.client.get<StoryRoot>(`/versions/${versionId}`),
                `${this.baseUrl}/versions/${versionId}`,
                { maxRetries: 3, initialDelayMs: 500, maxDelayMs: 5000 }
            );
            return response.data;
        } catch (error) {
            throw new Error(this.createErrorMessage('Failed to retrieve Story Root version', error));
        }
    }

    /**
     * GET /api/story-root/versions - Returns a list of all Story Root versions for the authenticated user.
     * Uses retry logic for transient failures.
     */
    async listStoryRootVersions(): Promise<VersionMetadata[]> {
        try {
            const response = await withRetry(
                () => this.client.get<VersionMetadata[]>('/versions'),
                `${this.baseUrl}/versions`,
                { maxRetries: 3, initialDelayMs: 500, maxDelayMs: 5000 }
            );
            return response.data;
        } catch (error) {
            throw new Error(this.createErrorMessage('Failed to retrieve Story Root versions', error));
        }
    }

    /**
     * POST /api/story-root/propose-merge - Generates an LLM proposal for merging raw input into the Story Root.
     * Uses retry logic for transient failures.
     */
    async proposeStoryRootMerge(rawInput: string): Promise<ProposalResponse<StoryRoot>> {
        if (!rawInput || rawInput.trim() === '') {
            throw new Error('Raw input is required');
        }

        try {
            const response = await withRetry(
                () => this.client.post<ProposalResponse<StoryRoot>>('/propose-merge', {
                    raw_input: rawInput,
                }),
                `${this.baseUrl}/propose-merge`,
                { maxRetries: 3, initialDelayMs: 500, maxDelayMs: 5000 }
            );
            return response.data;
        } catch (error) {
            throw new Error(this.createErrorMessage('Failed to propose Story Root merge', error));
        }
    }

    /**
     * POST /api/story-root/commit - Commits a Story Root proposal as a new version.
     * Uses retry logic for transient failures only (not for 409 conflicts).
     * @param storyRoot The Story Root to commit
     * @param expectedVersionId Optional expected current version ID for optimistic concurrency control
     */
    async commitStoryRootVersion(storyRoot: StoryRoot, expectedVersionId?: string): Promise<CommitResponse<StoryRoot>> {
        if (!storyRoot) {
            throw new Error('Story Root is required');
        }

        const requestBody: { story_root: StoryRoot; expected_version_id?: string } = {
            story_root: storyRoot,
        };

        if (expectedVersionId) {
            requestBody.expected_version_id = expectedVersionId;
        }

        try {
            // For commit operations, we want to retry transient failures but NOT retry version conflicts (409)
            // The withRetry function checks error classification, and 409 is classified as actionable (not transient)
            const response = await withRetry(
                () => this.client.post<CommitResponse<StoryRoot>>('/commit', requestBody),
                `${this.baseUrl}/commit`,
                { maxRetries: 3, initialDelayMs: 500, maxDelayMs: 5000 }
            );
            return response.data;
        } catch (error) {
            // Error classification already handles 409 as actionable (not retryable)
            // Just format the error message appropriately
            const axiosError = error as AxiosError<ErrorResponse>;
            if (axiosError.response?.status === 409) {
                const errorMessage = axiosError.response?.data?.error || 'Version conflict detected. The Story Root has been updated by another user.';
                throw new Error(this.createErrorMessage(errorMessage, error));
            }
            
            throw new Error(this.createErrorMessage('Failed to commit Story Root', error));
        }
    }
}
