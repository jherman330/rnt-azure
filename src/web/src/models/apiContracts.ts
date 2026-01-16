import { StoryRoot } from './storyRoot';

export interface ProposalResponse<T> {
    proposal: T;
    current: T | null;
}

export interface CommitResponse<T> {
    version_id: string;
    artifact: T;
}

export interface ErrorResponse {
    error: string;
    correlation_id: string;
}

export interface ProposeStoryRootMergeRequest {
    raw_input: string;
}

export interface CommitStoryRootRequest {
    story_root: StoryRoot;
}
