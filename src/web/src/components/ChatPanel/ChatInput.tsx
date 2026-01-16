import { FC, ReactElement, useState, KeyboardEvent } from 'react';
import { Stack, TextField, IconButton } from '@fluentui/react';
import { useChat } from './ChatContext';

const ChatInput: FC = (): ReactElement => {
    const { sendMessage, isLoading } = useChat();
    const [inputValue, setInputValue] = useState('');

    const handleSend = async () => {
        if (inputValue.trim() && !isLoading) {
            const message = inputValue.trim();
            setInputValue('');
            await sendMessage(message);
        }
    };

    const handleKeyPress = (e: KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    };

    return (
        <Stack
            horizontal
            tokens={{ childrenGap: 8 }}
            styles={{
                root: {
                    padding: '12px 16px',
                    borderTop: '1px solid #edebe9',
                    backgroundColor: '#faf9f8',
                },
            }}
        >
            <TextField
                multiline
                autoAdjustHeight
                value={inputValue}
                onChange={(_, newValue) => setInputValue(newValue || '')}
                onKeyPress={handleKeyPress}
                placeholder="Type your message... (Press Enter to send, Shift+Enter for new line)"
                disabled={isLoading}
                styles={{
                    root: {
                        flex: 1,
                    },
                    field: {
                        maxHeight: '100px',
                        overflowY: 'auto',
                    },
                }}
            />
            <IconButton
                iconProps={{ iconName: 'Send' }}
                onClick={handleSend}
                disabled={!inputValue.trim() || isLoading}
                styles={{
                    root: {
                        alignSelf: 'flex-end',
                        width: '40px',
                        height: '40px',
                    },
                    rootDisabled: {
                        opacity: 0.5,
                    },
                }}
            />
        </Stack>
    );
};

export default ChatInput;
