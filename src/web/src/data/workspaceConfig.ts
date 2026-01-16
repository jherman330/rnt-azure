import { WorkspaceConfig } from '../types/navigation';

export const workspaceConfigs: WorkspaceConfig[] = [
    {
        id: 'story-root',
        name: 'Story Root',
        description: 'Define the core narrative foundations including genre, tone, and thematic pillars',
        route: '/story-root',
    },
    {
        id: 'world-state',
        name: 'World State',
        description: 'Manage the current state of your narrative world',
        route: '/world-state',
    },
];
