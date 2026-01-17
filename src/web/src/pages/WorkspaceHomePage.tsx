import { ComponentType, ReactElement } from 'react';
import { Stack, Text, PrimaryButton, IStackStyles, getTheme } from '@fluentui/react';
import { useNavigate } from 'react-router-dom';
import { useNavigation } from '../contexts/NavigationContext';
import { workspaceConfigs } from '../data/workspaceConfig';
import { stackPadding, stackGaps } from '../ux/styles';
import WithApplicationInsights from '../components/telemetryWithAppInsights';

const theme = getTheme();

const cardStyles: IStackStyles = {
    root: {
        padding: '20px',
        background: theme.palette.neutralLighterAlt,
        border: `1px solid ${theme.palette.neutralLight}`,
        boxShadow: theme.effects.elevation4,
        borderRadius: '4px',
    }
};

const WorkspaceHomePage: ComponentType<unknown> = (): ReactElement => {
    const navigate = useNavigate();
    const { setActiveWorkspace } = useNavigation();

    const handleWorkspaceSelect = (workspaceId: 'story-root' | 'world-state', route: string) => {
        setActiveWorkspace(workspaceId);
        navigate(route);
    };

    return (
        <Stack tokens={{ ...stackPadding, childrenGap: 30 }} style={{ maxWidth: '1200px', margin: '0 auto', padding: '40px 20px' }}>
            <Stack tokens={stackGaps}>
                <Text variant="xxLarge">Narrative Workspace</Text>
                <Text variant="large">
                    A workspace-oriented environment for managing narrative artifacts and story foundations.
                </Text>
                <Text variant="medium" style={{ marginTop: '10px' }}>
                    Select a workspace below to begin editing your narrative elements.
                </Text>
            </Stack>

            <Stack horizontal tokens={{ ...stackGaps, childrenGap: 20 }} wrap>
                {workspaceConfigs.map((workspace) => (
                    <Stack key={workspace.id} styles={cardStyles} tokens={stackGaps} style={{ minWidth: '300px', flex: 1 }}>
                        <Text variant="xLarge">{workspace.name}</Text>
                        <Text variant="medium">{workspace.description}</Text>
                        <PrimaryButton
                            text={`Open ${workspace.name}`}
                            onClick={() => handleWorkspaceSelect(workspace.id as 'story-root' | 'world-state', workspace.route)}
                            style={{ marginTop: '10px' }}
                        />
                    </Stack>
                ))}
            </Stack>

            <Stack tokens={stackGaps} style={{ marginTop: '20px' }}>
                <Text variant="medium" style={{ fontStyle: 'italic', opacity: 0.7 }}>
                    This workspace environment provides dedicated areas for different aspects of your narrative project.
                    Additional features and workspaces will be added over time.
                </Text>
            </Stack>
        </Stack>
    );
};

const WorkspaceHomePageWithTelemetry = WithApplicationInsights(WorkspaceHomePage, 'WorkspaceHomePage');

export default WorkspaceHomePageWithTelemetry;
