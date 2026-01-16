/**
 * Mock implementation of StoryRootService for testing
 */

import { StoryRoot, VersionMetadata, ProposalResponse, CommitResponse } from '../models';

export class MockStoryRootService {
    private mockCurrentStoryRoot: StoryRoot | null = null;
    private mockVersions: VersionMetadata[] = [];

    setCurrentStoryRoot(storyRoot: StoryRoot | null) {
        this.mockCurrentStoryRoot = storyRoot;
    }

    setVersions(versions: VersionMetadata[]) {
        this.mockVersions = versions;
    }

    async getCurrentStoryRoot(): Promise<StoryRoot> {
        if (!this.mockCurrentStoryRoot) {
            throw new Error('No current Story Root');
        }
        return this.mockCurrentStoryRoot;
    }

    async getStoryRootVersion(versionId: string): Promise<StoryRoot> {
        // Mock implementation
        return {
            story_root_id: versionId,
            genre: 'Mock Genre',
            tone: 'Mock Tone',
            thematic_pillars: 'Mock Pillars',
        };
    }

    async listStoryRootVersions(): Promise<VersionMetadata[]> {
        return this.mockVersions;
    }

    async proposeStoryRootMerge(_rawInput: string): Promise<ProposalResponse<StoryRoot>> {
        return {
            proposal: {
                story_root_id: 'proposal-id',
                genre: 'Proposed Genre',
                tone: 'Proposed Tone',
                thematic_pillars: 'Proposed Pillars',
            },
            current: this.mockCurrentStoryRoot,
        };
    }

    async commitStoryRootVersion(storyRoot: StoryRoot): Promise<CommitResponse<StoryRoot>> {
        return {
            version_id: 'new-version-id',
            artifact: storyRoot,
        };
    }
}
