export interface ChatMessage {
    id: string;
    role: 'user' | 'assistant' | 'system';
    content: string;
    timestamp: Date;
    error?: string;
    correlationId?: string;
}

export interface ChatState {
    messages: ChatMessage[];
    isLoading: boolean;
    error: string | null;
}

export interface ChatContextType extends ChatState {
    sendMessage: (content: string) => Promise<void>;
    clearChat: () => void;
}
