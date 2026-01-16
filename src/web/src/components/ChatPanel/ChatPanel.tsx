import { FC, ReactElement } from 'react';
import { Stack, Text, IconButton } from '@fluentui/react';
import ChatMessageDisplay from './ChatMessageDisplay';
import ChatInput from './ChatInput';
import { useChat } from './ChatContext';

const ChatPanel: FC = (): ReactElement => {
    const { clearChat, messages } = useChat();

    const handleClearChat = () => {
        if (window.confirm('Are you sure you want to clear the chat history?')) {
            clearChat();
        }
    };

    return (
        <Stack
            styles={{
                root: {
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    backgroundColor: '#ffffff',
                },
            }}
        >
            {/* Chat Header */}
            <Stack
                horizontal
                horizontalAlign="space-between"
                verticalAlign="center"
                styles={{
                    root: {
                        padding: '12px 16px',
                        borderBottom: '1px solid #edebe9',
                        backgroundColor: '#faf9f8',
                    },
                }}
            >
                <Text variant="medium" style={{ fontWeight: 600 }}>
                    Story Root Assistant
                </Text>
                {messages.length > 0 && (
                    <IconButton
                        iconProps={{ iconName: 'Delete' }}
                        onClick={handleClearChat}
                        title="Clear chat history"
                        styles={{
                            root: {
                                width: '32px',
                                height: '32px',
                            },
                        }}
                    />
                )}
            </Stack>

            {/* Chat Messages */}
            <Stack grow={1} styles={{ root: { minHeight: 0, overflow: 'hidden' } }}>
                <ChatMessageDisplay />
            </Stack>

            {/* Chat Input */}
            <ChatInput />
        </Stack>
    );
};

export default ChatPanel;
