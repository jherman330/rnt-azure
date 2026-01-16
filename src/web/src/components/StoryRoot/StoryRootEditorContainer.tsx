import { FC, ReactElement, useState, useCallback, useEffect } from 'react';
import { Stack, TextField, PrimaryButton, DefaultButton, Text, MessageBar, MessageBarType } from '@fluentui/react';
import { useStoryRoot } from '../../hooks/useStoryRoot';
import { StoryRoot } from '../../models';
import StoryRootDisplay from './StoryRootDisplay';
import StoryRootEditor from './StoryRootEditor';
import LoadingSpinner from './LoadingSpinner';
import ErrorDisplay from './ErrorDisplay';

export const StoryRootEditorContainer: FC = (): ReactElement => {
    const {
        currentStoryRoot,
        proposal,
        loading,
        error,
        proposeMerge,
        commitProposal,
        discardProposal,
        clearError,
    } = useStoryRoot();

    const [rawInput, setRawInput] = useState('');
    const [editedProposal, setEditedProposal] = useState<StoryRoot | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);

    const handlePropose = useCallback(async () => {
        if (!rawInput.trim()) {
            return;
        }

        clearError();
        setSuccessMessage(null);
        try {
            await proposeMerge(rawInput);
            // Set edited proposal to the new proposal
            // This will be updated via the context
        } catch (err) {
            // Error is handled by context
        }
    }, [rawInput, proposeMerge, clearError]);

    const handleApprove = useCallback(async () => {
        if (!editedProposal) {
            return;
        }

        clearError();
        setSuccessMessage(null);
        try {
            await commitProposal(editedProposal);
            setRawInput('');
            setEditedProposal(null);
            setSuccessMessage('Story Root has been successfully updated!');
        } catch (err) {
            // Error is handled by context
        }
    }, [editedProposal, commitProposal, clearError]);

    const handleDiscard = useCallback(() => {
        discardProposal();
        setRawInput('');
        setEditedProposal(null);
        setSuccessMessage(null);
        clearError();
    }, [discardProposal, clearError]);

    // Update edited proposal when proposal from context changes
    useEffect(() => {
        if (proposal) {
            setEditedProposal(proposal);
        } else {
            setEditedProposal(null);
        }
    }, [proposal]);

    return (
        <Stack tokens={{ childrenGap: 20 }} style={{ padding: '20px', maxWidth: '1400px', margin: '0 auto' }}>
            <Text variant="xxLarge">Story Root Editor</Text>

            {successMessage && (
                <MessageBar messageBarType={MessageBarType.success} onDismiss={() => setSuccessMessage(null)}>
                    {successMessage}
                </MessageBar>
            )}

            <ErrorDisplay error={error} onDismiss={clearError} />

            {loading && <LoadingSpinner label="Processing..." />}

            {/* Raw Input Section */}
            <Stack tokens={{ childrenGap: 12 }}>
                <Text variant="large">Raw Input</Text>
                <TextField
                    label="Enter your narrative ideas, genre, tone, and thematic pillars"
                    placeholder="Example: A dark fantasy story with a somber tone, exploring themes of loss and redemption..."
                    value={rawInput}
                    onChange={(_, value) => setRawInput(value || '')}
                    multiline
                    rows={6}
                    disabled={loading}
                />
                <PrimaryButton
                    text="Propose Story Root Update"
                    onClick={handlePropose}
                    disabled={loading || !rawInput.trim()}
                />
            </Stack>

            {/* Side-by-Side Comparison */}
            {proposal && editedProposal && (
                <Stack horizontal tokens={{ childrenGap: 20 }} style={{ marginTop: '20px' }}>
                    {/* Current Story Root (Read-only) */}
                    <Stack.Item grow={1}>
                        {currentStoryRoot ? (
                            <StoryRootDisplay storyRoot={currentStoryRoot} title="Current Story Root" />
                        ) : (
                            <Stack>
                                <Text variant="large">Current Story Root</Text>
                                <Text>No current Story Root exists.</Text>
                            </Stack>
                        )}
                    </Stack.Item>

                    {/* Proposal (Editable) */}
                    <Stack.Item grow={1}>
                        <StoryRootEditor
                            storyRoot={editedProposal}
                            onChange={setEditedProposal}
                            title="Proposed Story Root"
                        />
                    </Stack.Item>
                </Stack>
            )}

            {/* Action Buttons */}
            {proposal && editedProposal && (
                <Stack horizontal tokens={{ childrenGap: 12 }}>
                    <PrimaryButton
                        text="Approve Story Root"
                        onClick={handleApprove}
                        disabled={loading}
                    />
                    <DefaultButton
                        text="Discard"
                        onClick={handleDiscard}
                        disabled={loading}
                    />
                </Stack>
            )}

            {/* Display Current Story Root if no proposal */}
            {!proposal && currentStoryRoot && !loading && (
                <Stack>
                    <Text variant="large">Current Story Root</Text>
                    <StoryRootDisplay storyRoot={currentStoryRoot} />
                </Stack>
            )}
        </Stack>
    );
};

export default StoryRootEditorContainer;
