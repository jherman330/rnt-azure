import axios, { AxiosInstance, AxiosError } from 'axios';
import config from '../config';
import { StoryRoot, VersionMetadata, ProposalResponse, CommitResponse, ErrorResponse } from '../models';

export class StoryRootService {
    private client: AxiosInstance;

    constructor() {
        this.client = axios.create({
            baseURL: `${config.api.baseUrl}/api/story-root`,
            headers: {
                'Content-Type': 'application/json',
            },
        });
    }

    /**
     * Extracts correlation ID from error response
     */
    private extractCorrelationId(error: AxiosError<ErrorResponse>): string | null {
        if (error.response?.data?.correlation_id) {
            return error.response.data.correlation_id;
        }
        return null;
    }

    /**
     * GET /api/story-root - Returns the current Story Root for the authenticated user.
     */
    async getCurrentStoryRoot(): Promise<StoryRoot> {
        try {
            const response = await this.client.get<StoryRoot>('/');
            return response.data;
        } catch (error) {
            const axiosError = error as AxiosError<ErrorResponse>;
            const correlationId = this.extractCorrelationId(axiosError);
            const errorMessage = axiosError.response?.data?.error || axiosError.message || 'Failed to retrieve current Story Root';
            throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
        }
    }

    /**
     * GET /api/story-root/versions/{versionId} - Returns a specific version of the Story Root.
     */
    async getStoryRootVersion(versionId: string): Promise<StoryRoot> {
        if (!versionId || versionId.trim() === '') {
            throw new Error('Version ID is required');
        }

        try {
            const response = await this.client.get<StoryRoot>(`/versions/${versionId}`);
            return response.data;
        } catch (error) {
            const axiosError = error as AxiosError<ErrorResponse>;
            const correlationId = this.extractCorrelationId(axiosError);
            const errorMessage = axiosError.response?.data?.error || axiosError.message || 'Failed to retrieve Story Root version';
            throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
        }
    }

    /**
     * GET /api/story-root/versions - Returns a list of all Story Root versions for the authenticated user.
     */
    async listStoryRootVersions(): Promise<VersionMetadata[]> {
        try {
            const response = await this.client.get<VersionMetadata[]>('/versions');
            return response.data;
        } catch (error) {
            const axiosError = error as AxiosError<ErrorResponse>;
            const correlationId = this.extractCorrelationId(axiosError);
            const errorMessage = axiosError.response?.data?.error || axiosError.message || 'Failed to retrieve Story Root versions';
            throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
        }
    }

    /**
     * POST /api/story-root/propose-merge - Generates an LLM proposal for merging raw input into the Story Root.
     */
    async proposeStoryRootMerge(rawInput: string): Promise<ProposalResponse<StoryRoot>> {
        if (!rawInput || rawInput.trim() === '') {
            throw new Error('Raw input is required');
        }

        try {
            const response = await this.client.post<ProposalResponse<StoryRoot>>('/propose-merge', {
                raw_input: rawInput,
            });
            return response.data;
        } catch (error) {
            const axiosError = error as AxiosError<ErrorResponse>;
            const correlationId = this.extractCorrelationId(axiosError);
            const errorMessage = axiosError.response?.data?.error || axiosError.message || 'Failed to propose Story Root merge';
            throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
        }
    }

    /**
     * POST /api/story-root/commit - Commits a Story Root proposal as a new version.
     * @param storyRoot The Story Root to commit
     * @param expectedVersionId Optional expected current version ID for optimistic concurrency control
     */
    async commitStoryRootVersion(storyRoot: StoryRoot, expectedVersionId?: string): Promise<CommitResponse<StoryRoot>> {
        if (!storyRoot) {
            throw new Error('Story Root is required');
        }

        try {
            const requestBody: { story_root: StoryRoot; expected_version_id?: string } = {
                story_root: storyRoot,
            };

            if (expectedVersionId) {
                requestBody.expected_version_id = expectedVersionId;
            }

            const response = await this.client.post<CommitResponse<StoryRoot>>('/commit', requestBody);
            return response.data;
        } catch (error) {
            const axiosError = error as AxiosError<ErrorResponse>;
            const correlationId = this.extractCorrelationId(axiosError);
            
            // Handle version conflict (409) specifically
            if (axiosError.response?.status === 409) {
                const errorMessage = axiosError.response?.data?.error || 'Version conflict detected. The Story Root has been updated by another user.';
                throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
            }
            
            const errorMessage = axiosError.response?.data?.error || axiosError.message || 'Failed to commit Story Root';
            throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
        }
    }
}
