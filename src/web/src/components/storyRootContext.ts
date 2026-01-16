import { createContext, useContext, useState, useCallback, useEffect, ReactNode } from 'react';
import { StoryRoot, ProposalResponse, CommitResponse, ErrorResponse } from '../models';
import { StoryRootService } from '../services/storyRootService';

export interface StoryRootContextType {
    currentStoryRoot: StoryRoot | null;
    proposal: StoryRoot | null;
    loading: boolean;
    error: ErrorResponse | null;
    loadCurrentStoryRoot: () => Promise<void>;
    proposeMerge: (rawInput: string) => Promise<ProposalResponse<StoryRoot>>;
    commitProposal: (storyRoot: StoryRoot) => Promise<CommitResponse<StoryRoot>>;
    discardProposal: () => void;
    clearError: () => void;
}

const defaultContext: StoryRootContextType = {
    currentStoryRoot: null,
    proposal: null,
    loading: false,
    error: null,
    loadCurrentStoryRoot: async () => {},
    proposeMerge: async () => { throw new Error('Context not initialized'); },
    commitProposal: async () => { throw new Error('Context not initialized'); },
    discardProposal: () => {},
    clearError: () => {},
};

export const StoryRootContext = createContext<StoryRootContextType>(defaultContext);

export interface StoryRootProviderProps {
    children: ReactNode;
    service?: StoryRootService;
}

export function StoryRootProvider({ children, service = new StoryRootService() }: StoryRootProviderProps) {
    const [currentStoryRoot, setCurrentStoryRoot] = useState<StoryRoot | null>(null);
    const [proposal, setProposal] = useState<StoryRoot | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<ErrorResponse | null>(null);

    const loadCurrentStoryRoot = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const storyRoot = await service.getCurrentStoryRoot();
            setCurrentStoryRoot(storyRoot);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to load Story Root';
            // Extract correlation ID from error message if present
            const correlationIdMatch = errorMessage.match(/Correlation ID: ([^\s)]+)/);
            setError({
                error: errorMessage,
                correlation_id: correlationIdMatch ? correlationIdMatch[1] : '',
            });
        } finally {
            setLoading(false);
        }
    }, [service]);

    const proposeMerge = useCallback(async (rawInput: string): Promise<ProposalResponse<StoryRoot>> => {
        setLoading(true);
        setError(null);
        try {
            const response = await service.proposeStoryRootMerge(rawInput);
            setProposal(response.proposal);
            if (response.current) {
                setCurrentStoryRoot(response.current);
            }
            return response;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to propose merge';
            const correlationIdMatch = errorMessage.match(/Correlation ID: ([^\s)]+)/);
            setError({
                error: errorMessage,
                correlation_id: correlationIdMatch ? correlationIdMatch[1] : '',
            });
            throw err;
        } finally {
            setLoading(false);
        }
    }, [service]);

    const commitProposal = useCallback(async (storyRoot: StoryRoot): Promise<CommitResponse<StoryRoot>> => {
        setLoading(true);
        setError(null);
        try {
            const response = await service.commitStoryRootVersion(storyRoot);
            setCurrentStoryRoot(response.artifact);
            setProposal(null);
            return response;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to commit Story Root';
            const correlationIdMatch = errorMessage.match(/Correlation ID: ([^\s)]+)/);
            setError({
                error: errorMessage,
                correlation_id: correlationIdMatch ? correlationIdMatch[1] : '',
            });
            throw err;
        } finally {
            setLoading(false);
        }
    }, [service]);

    const discardProposal = useCallback(() => {
        setProposal(null);
        setError(null);
    }, []);

    const clearError = useCallback(() => {
        setError(null);
    }, []);

    // Load current Story Root on mount
    useEffect(() => {
        loadCurrentStoryRoot();
    }, [loadCurrentStoryRoot]);

    const value: StoryRootContextType = {
        currentStoryRoot,
        proposal,
        loading,
        error,
        loadCurrentStoryRoot,
        proposeMerge,
        commitProposal,
        discardProposal,
        clearError,
    };

    return (
        <StoryRootContext.Provider value={value}>
            {children}
        </StoryRootContext.Provider>
    );
}

export function useStoryRoot(): StoryRootContextType {
    const context = useContext(StoryRootContext);
    if (!context) {
        throw new Error('useStoryRoot must be used within a StoryRootProvider');
    }
    return context;
}
