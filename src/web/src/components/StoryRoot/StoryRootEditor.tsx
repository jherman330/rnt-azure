import { FC, ReactElement, useState, useEffect } from 'react';
import { Stack, TextField, Text } from '@fluentui/react';
import { StoryRoot } from '../../models';

interface StoryRootEditorProps {
    storyRoot: StoryRoot;
    onChange: (storyRoot: StoryRoot) => void;
    title?: string;
}

export const StoryRootEditor: FC<StoryRootEditorProps> = ({ storyRoot, onChange, title = 'Edit Story Root' }): ReactElement => {
    const [genre, setGenre] = useState(storyRoot.genre);
    const [tone, setTone] = useState(storyRoot.tone);
    const [thematicPillars, setThematicPillars] = useState(storyRoot.thematic_pillars);
    const [notes, setNotes] = useState(storyRoot.notes || '');

    useEffect(() => {
        setGenre(storyRoot.genre);
        setTone(storyRoot.tone);
        setThematicPillars(storyRoot.thematic_pillars);
        setNotes(storyRoot.notes || '');
    }, [storyRoot]);

    const handleChange = (field: keyof StoryRoot, value: string) => {
        const updated: StoryRoot = {
            ...storyRoot,
            [field]: value,
        };
        onChange(updated);
    };

    return (
        <Stack tokens={{ childrenGap: 16 }}>
            <Text variant="xLarge">{title}</Text>
            <TextField
                label="Genre"
                value={genre}
                onChange={(_, value) => {
                    setGenre(value || '');
                    handleChange('genre', value || '');
                }}
                multiline
                rows={2}
            />
            <TextField
                label="Tone"
                value={tone}
                onChange={(_, value) => {
                    setTone(value || '');
                    handleChange('tone', value || '');
                }}
                multiline
                rows={2}
            />
            <TextField
                label="Thematic Pillars"
                value={thematicPillars}
                onChange={(_, value) => {
                    setThematicPillars(value || '');
                    handleChange('thematic_pillars', value || '');
                }}
                multiline
                rows={4}
            />
            <TextField
                label="Notes"
                value={notes}
                onChange={(_, value) => {
                    setNotes(value || '');
                    handleChange('notes', value || '');
                }}
                multiline
                rows={4}
            />
        </Stack>
    );
};

export default StoryRootEditor;
