export type WorkspaceType = 'home' | 'story-root' | 'world-state';

export interface WorkspaceConfig {
    id: WorkspaceType;
    name: string;
    description: string;
    route: string;
    icon?: string;
}
