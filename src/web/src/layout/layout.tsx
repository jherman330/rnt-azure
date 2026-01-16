import { FC, ReactElement, useEffect } from 'react';
import { Routes, Route, useLocation } from 'react-router-dom';
import { Stack } from '@fluentui/react';
import Header from './header';
import LeftPanel from './LeftPanel';
import CenterWorkspace from './CenterWorkspace';
import RightPanel from './RightPanel';
import { rootStackStyles, headerStackStyles, sidebarStackStyles, mainStackStyles } from '../ux/styles';
import WorkspaceHomePage from '../pages/WorkspaceHomePage';
import StoryRootPage from '../pages/storyRootPage';
import WorldStatePage from '../pages/worldStatePage';
import { NavigationProvider, useNavigation } from '../contexts/NavigationContext';
import { WorkspaceType } from '../types/navigation';

const LayoutContent: FC = (): ReactElement => {
    const location = useLocation();
    const { setActiveWorkspace } = useNavigation();

    // Update active workspace based on route
    useEffect(() => {
        if (location.pathname === '/') {
            setActiveWorkspace('home');
        } else if (location.pathname === '/story-root') {
            setActiveWorkspace('story-root');
        } else if (location.pathname === '/world-state') {
            setActiveWorkspace('world-state');
        }
    }, [location.pathname, setActiveWorkspace]);

    return (
        <Stack styles={rootStackStyles}>
            <Stack.Item styles={headerStackStyles}>
                <Header />
            </Stack.Item>
            <Stack horizontal grow={1}>
                <Stack.Item styles={sidebarStackStyles}>
                    <LeftPanel />
                </Stack.Item>
                <Stack.Item grow={1} styles={mainStackStyles}>
                    <CenterWorkspace>
                        <Routes>
                            <Route path="/" element={<WorkspaceHomePage />} />
                            <Route path="/story-root" element={<StoryRootPage />} />
                            <Route path="/world-state" element={<WorldStatePage />} />
                        </Routes>
                    </CenterWorkspace>
                </Stack.Item>
                <Stack.Item styles={sidebarStackStyles}>
                    <RightPanel />
                </Stack.Item>
            </Stack>
        </Stack>
    );
};

const Layout: FC = (): ReactElement => {
    return (
        <NavigationProvider>
            <LayoutContent />
        </NavigationProvider>
    );
};

export default Layout;
