import { FC, ReactElement, ReactNode } from 'react';
import { Stack } from '@fluentui/react';

interface CenterWorkspaceProps {
    children: ReactNode;
}

const CenterWorkspace: FC<CenterWorkspaceProps> = ({ children }): ReactElement => {
    return (
        <Stack style={{ height: '100%', overflow: 'auto' }}>
            {children}
        </Stack>
    );
};

export default CenterWorkspace;
