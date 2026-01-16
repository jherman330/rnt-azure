import { FC, ReactElement, useState } from 'react';
import { Stack, Text, IconButton, TooltipHost } from '@fluentui/react';
import { ChatMessage } from './chatTypes';

export interface ChatMessageBubbleProps {
    message: ChatMessage;
}

const ChatMessageBubble: FC<ChatMessageBubbleProps> = ({ message }): ReactElement => {
    const [copied, setCopied] = useState(false);

    const handleCopy = async () => {
        try {
            await navigator.clipboard.writeText(message.content);
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        } catch (err) {
            console.error('Failed to copy text:', err);
        }
    };

    const formatTimestamp = (date: Date): string => {
        return new Date(date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    const isUser = message.role === 'user';
    const isSystem = message.role === 'system';
    const isError = message.error !== undefined;

    const bubbleStyles: React.CSSProperties = {
        padding: '12px 16px',
        borderRadius: '8px',
        maxWidth: '85%',
        wordWrap: 'break-word' as const,
        backgroundColor: isUser 
            ? '#0078d4' 
            : isSystem && isError
            ? '#fef6e6'
            : '#f3f2f1',
        color: isUser ? '#ffffff' : isError ? '#8a2e00' : '#323130',
        alignSelf: isUser ? 'flex-end' : 'flex-start',
        border: isError ? '1px solid #ffaa44' : undefined,
    };

    const textStyles: React.CSSProperties = {
        whiteSpace: 'pre-wrap',
        margin: 0,
        fontSize: '14px',
        lineHeight: '1.5',
    };

    return (
        <Stack 
            horizontal 
            horizontalAlign={isUser ? 'end' : 'start'} 
            styles={{ root: { width: '100%', marginBottom: '12px' } }}
        >
            <Stack styles={{ root: { maxWidth: '85%' } }}>
                <Stack horizontal horizontalAlign="space-between" tokens={{ childrenGap: 8 }}>
                    <Stack 
                        styles={{ root: bubbleStyles }}
                        tokens={{ childrenGap: 4 }}
                    >
                        {!isUser && (
                            <Text variant="small" style={{ opacity: 0.7, fontSize: '11px', fontWeight: 600 }}>
                                {isSystem ? (isError ? 'Error' : 'System') : 'Assistant'}
                            </Text>
                        )}
                        <Text style={textStyles}>{message.content}</Text>
                        {message.correlationId && (
                            <Text variant="small" style={{ opacity: 0.6, fontSize: '10px', marginTop: '4px' }}>
                                Correlation ID: {message.correlationId}
                            </Text>
                        )}
                    </Stack>
                    {!isUser && (
                        <TooltipHost content={copied ? 'Copied!' : 'Copy'}>
                            <IconButton
                                iconProps={{ iconName: copied ? 'CheckMark' : 'Copy' }}
                                onClick={handleCopy}
                                styles={{
                                    root: {
                                        width: '32px',
                                        height: '32px',
                                        alignSelf: 'flex-start',
                                    },
                                }}
                            />
                        </TooltipHost>
                    )}
                </Stack>
                <Text 
                    variant="small" 
                    style={{ 
                        opacity: 0.5, 
                        fontSize: '11px', 
                        padding: '4px 4px 0 4px',
                        textAlign: isUser ? 'right' : 'left',
                    }}
                >
                    {formatTimestamp(message.timestamp)}
                </Text>
            </Stack>
        </Stack>
    );
};

export default ChatMessageBubble;
