import { ComponentType, ReactElement } from 'react';
import WithApplicationInsights from '../components/telemetryWithAppInsights';
import WorldStateStubEditor from '../components/WorldState/WorldStateStubEditor';

const WorldStatePage: ComponentType<unknown> = (): ReactElement => {
    return <WorldStateStubEditor />;
};

const WorldStatePageWithTelemetry = WithApplicationInsights(WorldStatePage, 'WorldStatePage');

export default WorldStatePageWithTelemetry;
