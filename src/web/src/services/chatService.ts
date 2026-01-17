import axios, { AxiosInstance, AxiosError } from 'axios';
import config from '../config';
import { ErrorResponse, ProposeStoryRootMergeRequest, ProposalResponse, StoryRoot } from '../models';

export interface ChatRequest {
    message: string;
    context?: StoryRoot;
}

export interface ChatResponse {
    response: string;
    correlationId?: string;
}

export class ChatService {
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
     * Sends a chat message to the LLM service using the propose-merge endpoint
     * The message is treated as raw input that will be merged into the Story Root
     */
    async sendChatMessage(request: ChatRequest): Promise<ChatResponse> {
        if (!request.message || request.message.trim() === '') {
            throw new Error('Message is required');
        }

        try {
            // Use the existing propose-merge endpoint
            // We'll send the user's message as raw_input
            const payload: ProposeStoryRootMergeRequest = {
                raw_input: request.message,
            };

            const response = await this.client.post<ProposalResponse<StoryRoot>>('/propose-merge', payload);
            
            // Extract meaningful response from the proposal
            // The proposal contains the merged Story Root, so we'll format it as a readable response
            const proposal = response.data.proposal;
            const responseText = this.formatProposalAsResponse(proposal, request.message);
            
            // Try to extract correlation ID from response headers or use a placeholder
            const correlationId = response.headers['x-correlation-id'] || undefined;

            return {
                response: responseText,
                correlationId,
            };
        } catch (error) {
            const axiosError = error as AxiosError<ErrorResponse>;
            const correlationId = this.extractCorrelationId(axiosError);
            const errorMessage = axiosError.response?.data?.error || axiosError.message || 'Failed to send chat message';
            throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
        }
    }

    /**
     * Formats the Story Root proposal as a readable chat response
     */
    private formatProposalAsResponse(proposal: StoryRoot, _userMessage: string): string {
        // Extract the key changes or provide a summary of the proposal
        const parts: string[] = [];
        
        parts.push("Based on your input, here's an updated version of your Story Root:");
        
        if (proposal.genre) {
            parts.push(`\n**Genre:** ${proposal.genre}`);
        }
        if (proposal.tone) {
            parts.push(`**Tone:** ${proposal.tone}`);
        }
        if (proposal.thematic_pillars) {
            parts.push(`**Thematic Pillars:** ${proposal.thematic_pillars}`);
        }
        if (proposal.notes) {
            parts.push(`\n**Notes:** ${proposal.notes}`);
        }
        
        parts.push("\nYou can review this proposal in the Story Root editor and commit it if it looks good.");
        
        return parts.join('\n');
    }
}
