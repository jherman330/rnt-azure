import { FC, ReactElement } from 'react';
import { Stack, Text } from '@fluentui/react';
import { stackPadding } from '../ux/styles';

const RightPanel: FC = (): ReactElement => {
    return (
        <Stack tokens={stackPadding} style={{ height: '100%', alignItems: 'center', justifyContent: 'center' }}>
            <Text variant="medium" style={{ textAlign: 'center', opacity: 0.6 }}>
                Chat Coming Soon
            </Text>
            <Text variant="small" style={{ textAlign: 'center', opacity: 0.5, marginTop: '10px' }}>
                Future chat functionality will appear here
            </Text>
        </Stack>
    );
};

export default RightPanel;
