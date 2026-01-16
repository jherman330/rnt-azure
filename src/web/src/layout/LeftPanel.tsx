import { FC, ReactElement } from 'react';
import { Stack, Text } from '@fluentui/react';
import { useNavigation } from '../contexts/NavigationContext';
import { stackPadding } from '../ux/styles';

const LeftPanel: FC = (): ReactElement => {
    const { activeWorkspace } = useNavigation();

    return (
        <Stack tokens={stackPadding}>
            <Text variant="medium">Workspace Navigation</Text>
            <Text variant="small" style={{ marginTop: '10px' }}>
                {activeWorkspace === 'home' && 'Select a workspace to begin'}
                {activeWorkspace === 'story-root' && 'Story Root workspace'}
                {activeWorkspace === 'world-state' && 'World State workspace'}
            </Text>
        </Stack>
    );
};

export default LeftPanel;
