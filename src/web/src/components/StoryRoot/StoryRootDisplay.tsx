import { FC, ReactElement } from 'react';
import { Stack, Text, TextField } from '@fluentui/react';
import { StoryRoot } from '../../models';

interface StoryRootDisplayProps {
    storyRoot: StoryRoot;
    title?: string;
}

export const StoryRootDisplay: FC<StoryRootDisplayProps> = ({ storyRoot, title = 'Story Root' }): ReactElement => {
    return (
        <Stack tokens={{ childrenGap: 16 }}>
            <Text variant="xLarge">{title}</Text>
            <TextField
                label="Genre"
                value={storyRoot.genre}
                readOnly
                multiline
                rows={2}
            />
            <TextField
                label="Tone"
                value={storyRoot.tone}
                readOnly
                multiline
                rows={2}
            />
            <TextField
                label="Thematic Pillars"
                value={storyRoot.thematic_pillars}
                readOnly
                multiline
                rows={4}
            />
            {storyRoot.notes && (
                <TextField
                    label="Notes"
                    value={storyRoot.notes}
                    readOnly
                    multiline
                    rows={4}
                />
            )}
        </Stack>
    );
};

export default StoryRootDisplay;
