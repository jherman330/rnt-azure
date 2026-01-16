/**
 * Unit tests for StoryRootService
 * 
 * Note: These tests require test infrastructure setup (Jest, axios-mock-adapter)
 * Run: npm test -- storyRootService.test.ts
 */

import { StoryRootService } from '../storyRootService';
import { StoryRoot, VersionMetadata, ProposalResponse, CommitResponse } from '../../models';
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';

describe('StoryRootService', () => {
    let service: StoryRootService;
    let mockAxios: MockAdapter;

    beforeEach(() => {
        service = new StoryRootService();
        mockAxios = new MockAdapter(axios);
    });

    afterEach(() => {
        mockAxios.restore();
    });

    describe('getCurrentStoryRoot', () => {
        it('should return current Story Root', async () => {
            const mockStoryRoot: StoryRoot = {
                story_root_id: 'test-id',
                genre: 'Fantasy',
                tone: 'Epic',
                thematic_pillars: 'Test pillars',
            };

            mockAxios.onGet('/api/story-root').reply(200, mockStoryRoot);

            const result = await service.getCurrentStoryRoot();

            expect(result).toEqual(mockStoryRoot);
        });

        it('should throw error with correlation ID when request fails', async () => {
            mockAxios.onGet('/api/story-root').reply(500, {
                error: 'Server error',
                correlation_id: 'corr-123',
            });

            await expect(service.getCurrentStoryRoot()).rejects.toThrow('Correlation ID: corr-123');
        });
    });

    describe('proposeStoryRootMerge', () => {
        it('should return proposal response', async () => {
            const mockProposal: StoryRoot = {
                story_root_id: 'proposal-id',
                genre: 'Updated Genre',
                tone: 'Updated Tone',
                thematic_pillars: 'Updated Pillars',
            };

            const mockResponse: ProposalResponse<StoryRoot> = {
                proposal: mockProposal,
                current: null,
            };

            mockAxios.onPost('/api/story-root/propose-merge').reply(200, mockResponse);

            const result = await service.proposeStoryRootMerge('test input');

            expect(result).toEqual(mockResponse);
        });

        it('should throw error when raw input is empty', async () => {
            await expect(service.proposeStoryRootMerge('')).rejects.toThrow('Raw input is required');
        });
    });

    describe('commitStoryRootVersion', () => {
        it('should return commit response', async () => {
            const mockStoryRoot: StoryRoot = {
                story_root_id: 'test-id',
                genre: 'Fantasy',
                tone: 'Epic',
                thematic_pillars: 'Test pillars',
            };

            const mockResponse: CommitResponse<StoryRoot> = {
                version_id: 'version-123',
                artifact: mockStoryRoot,
            };

            mockAxios.onPost('/api/story-root/commit').reply(200, mockResponse);

            const result = await service.commitStoryRootVersion(mockStoryRoot);

            expect(result).toEqual(mockResponse);
        });

        it('should throw error when Story Root is null', async () => {
            await expect(service.commitStoryRootVersion(null as any)).rejects.toThrow('Story Root is required');
        });
    });
});
