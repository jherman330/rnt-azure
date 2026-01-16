import { FontIcon, getTheme, IStackStyles, mergeStyles, Stack, Text, Nav } from '@fluentui/react';
import { FC, ReactElement } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useNavigation } from '../contexts/NavigationContext';
import { WorkspaceType } from '../types/navigation';

const theme = getTheme();

const logoStyles: IStackStyles = {
    root: {
        width: '250px',
        background: theme.palette.themePrimary,
        alignItems: 'center',
        padding: '0 20px'
    }
}

const logoIconClass = mergeStyles({
    fontSize: 20,
    paddingRight: 10
});

const navStyles: IStackStyles = {
    root: {
        alignItems: 'center',
        padding: '0 20px',
        height: 48
    }
}

const Header: FC = (): ReactElement => {
    const navigate = useNavigate();
    const location = useLocation();
    const { setActiveWorkspace } = useNavigation();

    const navLinkGroups = [
        {
            links: [
                {
                    name: 'Home',
                    key: 'home',
                    url: '/',
                    icon: 'Home',
                },
                {
                    name: 'Story Root',
                    key: 'story-root',
                    url: '/story-root',
                    icon: 'DocumentSet',
                },
                {
                    name: 'World State',
                    key: 'world-state',
                    url: '/world-state',
                    icon: 'Globe',
                },
            ],
        },
    ];

    const onLinkClick = (ev?: React.MouseEvent<HTMLElement>, item?: any) => {
        if (ev) {
            ev.preventDefault();
        }
        if (item) {
            navigate(item.url);
            setActiveWorkspace(item.key as WorkspaceType);
        }
    };

    // Determine selected key based on current location
    const getSelectedKey = () => {
        if (location.pathname === '/') return 'home';
        if (location.pathname === '/story-root') return 'story-root';
        if (location.pathname === '/world-state') return 'world-state';
        return 'home';
    };

    return (
        <Stack horizontal>
            <Stack horizontal styles={logoStyles}>
                <FontIcon aria-label="Narrative" iconName="Edit" className={logoIconClass} />
                <Text variant="xLarge">Narrative Workspace</Text>
            </Stack>
            <Stack.Item grow={1} styles={navStyles}>
                <Nav
                    groups={navLinkGroups}
                    selectedKey={getSelectedKey()}
                    onLinkClick={onLinkClick}
                    styles={{
                        root: {
                            height: 48,
                        },
                        link: {
                            height: 48,
                            lineHeight: '48px',
                            color: theme.palette.white,
                            selectors: {
                                ':hover': {
                                    backgroundColor: theme.palette.themeDark,
                                },
                            },
                        },
                        linkText: {
                            fontSize: 14,
                        },
                    }}
                />
            </Stack.Item>
        </Stack>
    );
}

export default Header;
