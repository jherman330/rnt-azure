import { FC, ReactElement, useContext, ReactNode } from 'react';
import { ChatProvider } from './ChatContext';
import { StoryRootContext } from '../components/storyRootContext';
import { ChatService } from '../../services/chatService';

export interface ChatProviderWrapperProps {
    children: ReactNode;
    chatService?: ChatService;
}

const ChatProviderWrapper: FC<ChatProviderWrapperProps> = ({ 
    children, 
    chatService 
}): ReactElement => {
    // Try to get Story Root context if available (it's optional)
    let currentStoryRoot = null;
    try {
        const storyRootContext = useContext(StoryRootContext);
        currentStoryRoot = storyRootContext?.currentStoryRoot || null;
    } catch {
        // StoryRootContext not available, continue without it
        currentStoryRoot = null;
    }

    return (
        <ChatProvider chatService={chatService} storyRootContext={currentStoryRoot}>
            {children}
        </ChatProvider>
    );
};

export default ChatProviderWrapper;
