import { FC, ReactElement } from 'react';
import { Spinner, Stack } from '@fluentui/react';

interface LoadingSpinnerProps {
    label?: string;
}

export const LoadingSpinner: FC<LoadingSpinnerProps> = ({ label = 'Loading...' }): ReactElement => {
    return (
        <Stack horizontalAlign="center" verticalAlign="center" style={{ padding: '20px' }}>
            <Spinner label={label} />
        </Stack>
    );
};

export default LoadingSpinner;
