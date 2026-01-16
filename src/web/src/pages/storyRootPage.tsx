import { FC, ReactElement } from 'react';
import { StoryRootProvider } from '../components/storyRootContext';
import StoryRootEditorContainer from '../components/StoryRoot/StoryRootEditorContainer';
import WithApplicationInsights from '../components/telemetryWithAppInsights';

const StoryRootPage: FC = (): ReactElement => {
    return (
        <StoryRootProvider>
            <StoryRootEditorContainer />
        </StoryRootProvider>
    );
};

const StoryRootPageWithTelemetry = WithApplicationInsights(StoryRootPage, 'StoryRootPage');

export default StoryRootPageWithTelemetry;
