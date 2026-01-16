import { FC, ReactElement, useEffect, useRef } from 'react';
import { Stack, Spinner, Text } from '@fluentui/react';
import ChatMessageBubble from './ChatMessageBubble';
import { useChat } from './ChatContext';

const ChatMessageDisplay: FC = (): ReactElement => {
    const { messages, isLoading } = useChat();
    const messagesEndRef = useRef<HTMLDivElement>(null);

    // Auto-scroll to bottom when new messages arrive
    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages, isLoading]);

    return (
        <Stack
            styles={{
                root: {
                    height: '100%',
                    overflowY: 'auto',
                    padding: '16px',
                    backgroundColor: '#ffffff',
                },
            }}
        >
            {messages.length === 0 && !isLoading && (
                <Stack
                    horizontalAlign="center"
                    verticalAlign="center"
                    styles={{
                        root: {
                            height: '100%',
                            opacity: 0.5,
                        },
                    }}
                >
                    <Text variant="medium">Start a conversation about your Story Root</Text>
                    <Text variant="small" style={{ marginTop: '8px' }}>
                        Ask questions or request changes to genre, tone, thematic pillars, or notes
                    </Text>
                </Stack>
            )}

            {messages.map((message) => (
                <ChatMessageBubble key={message.id} message={message} />
            ))}

            {isLoading && (
                <Stack horizontal tokens={{ childrenGap: 8 }} styles={{ root: { padding: '8px 0' } }}>
                    <Spinner size={1} />
                    <Text variant="small" style={{ opacity: 0.7, alignSelf: 'center' }}>
                        Assistant is typing...
                    </Text>
                </Stack>
            )}

            <div ref={messagesEndRef} />
        </Stack>
    );
};

export default ChatMessageDisplay;
