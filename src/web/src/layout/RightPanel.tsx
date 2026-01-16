import { FC, ReactElement } from 'react';
import ChatProviderWrapper from '../components/ChatPanel/ChatProviderWrapper';
import ChatPanel from '../components/ChatPanel/ChatPanel';

const RightPanel: FC = (): ReactElement => {
    return (
        <ChatProviderWrapper>
            <ChatPanel />
        </ChatProviderWrapper>
    );
};

export default RightPanel;
