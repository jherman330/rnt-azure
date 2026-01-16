import { FC, ReactElement } from 'react';
import { Stack, Text, MessageBar, MessageBarType } from '@fluentui/react';
import { stackPadding, stackGaps } from '../../ux/styles';

const WorldStateStubEditor: FC = (): ReactElement => {
    return (
        <Stack tokens={{ ...stackPadding, childrenGap: 20 }} style={{ maxWidth: '1400px', margin: '0 auto', padding: '20px' }}>
            <Text variant="xxLarge">World State Editor</Text>
            
            <MessageBar messageBarType={MessageBarType.info}>
                World State Editor is coming soon. This workspace will allow you to manage the current state of your narrative world.
            </MessageBar>

            <Stack tokens={stackGaps} style={{ marginTop: '40px', textAlign: 'center' }}>
                <Text variant="large">Feature in Development</Text>
                <Text variant="medium" style={{ opacity: 0.7 }}>
                    The World State editor will provide functionality for managing and tracking the current state of your narrative world.
                </Text>
            </Stack>
        </Stack>
    );
};

export default WorldStateStubEditor;
