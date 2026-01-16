import { createContext, useContext, useState, useCallback, ReactNode, FC, ReactElement } from 'react';
import { WorkspaceType } from '../types/navigation';

export interface NavigationContextType {
    activeWorkspace: WorkspaceType;
    setActiveWorkspace: (workspace: WorkspaceType) => void;
}

const defaultContext: NavigationContextType = {
    activeWorkspace: 'home',
    setActiveWorkspace: () => {},
};

export const NavigationContext = createContext<NavigationContextType>(defaultContext);

export interface NavigationProviderProps {
    children: ReactNode;
}

export const NavigationProvider: FC<NavigationProviderProps> = ({ children }): ReactElement => {
    const [activeWorkspace, setActiveWorkspaceState] = useState<WorkspaceType>('home');

    const setActiveWorkspace = useCallback((workspace: WorkspaceType) => {
        setActiveWorkspaceState(workspace);
    }, []);

    const value: NavigationContextType = {
        activeWorkspace,
        setActiveWorkspace,
    };

    return (
        <NavigationContext.Provider value={value}>
            {children}
        </NavigationContext.Provider>
    );
};

export function useNavigation(): NavigationContextType {
    const context = useContext(NavigationContext);
    if (!context) {
        throw new Error('useNavigation must be used within a NavigationProvider');
    }
    return context;
}
