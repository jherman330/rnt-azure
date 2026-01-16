import { createContext, useContext, useState, useCallback, ReactNode, FC, ReactElement } from 'react';
import { ChatMessage, ChatContextType } from './chatTypes';
import { ChatService } from '../../services/chatService';
import { StoryRoot } from '../../models';

const defaultContext: ChatContextType = {
    messages: [],
    isLoading: false,
    error: null,
    sendMessage: async () => {},
    clearChat: () => {},
};

export const ChatContext = createContext<ChatContextType>(defaultContext);

export interface ChatProviderProps {
    children: ReactNode;
    chatService?: ChatService;
    storyRootContext?: StoryRoot | null;
}

export const ChatProvider: FC<ChatProviderProps> = ({ 
    children, 
    chatService = new ChatService(),
    storyRootContext = null
}): ReactElement => {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const sendMessage = useCallback(async (content: string) => {
        if (!content || content.trim() === '') {
            return;
        }

        // Create user message
        const userMessage: ChatMessage = {
            id: `user-${Date.now()}`,
            role: 'user',
            content: content.trim(),
            timestamp: new Date(),
        };

        // Add user message immediately
        setMessages(prev => [...prev, userMessage]);
        setIsLoading(true);
        setError(null);

        try {
            // Send to chat service with Story Root context (if available)
            const response = await chatService.sendChatMessage({
                message: content,
                context: storyRootContext || undefined,
            });

            // Create assistant message
            const assistantMessage: ChatMessage = {
                id: `assistant-${Date.now()}`,
                role: 'assistant',
                content: response.response,
                timestamp: new Date(),
                correlationId: response.correlationId,
            };

            setMessages(prev => [...prev, assistantMessage]);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to send message';
            const correlationIdMatch = errorMessage.match(/Correlation ID: ([^\s)]+)/);
            
            // Create error message
            const errorMsg: ChatMessage = {
                id: `error-${Date.now()}`,
                role: 'system',
                content: `Error: ${errorMessage}`,
                timestamp: new Date(),
                error: errorMessage,
                correlationId: correlationIdMatch ? correlationIdMatch[1] : undefined,
            };

            setMessages(prev => [...prev, errorMsg]);
            setError(errorMessage);
        } finally {
            setIsLoading(false);
        }
    }, [chatService, storyRootContext]);

    const clearChat = useCallback(() => {
        setMessages([]);
        setError(null);
    }, []);

    const value: ChatContextType = {
        messages,
        isLoading,
        error,
        sendMessage,
        clearChat,
    };

    return (
        <ChatContext.Provider value={value}>
            {children}
        </ChatContext.Provider>
    );
};

export function useChat(): ChatContextType {
    const context = useContext(ChatContext);
    if (!context) {
        throw new Error('useChat must be used within a ChatProvider');
    }
    return context;
}
