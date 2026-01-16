import { FC, ReactElement } from 'react';
import { MessageBar, MessageBarType, Stack, Text } from '@fluentui/react';
import { ErrorResponse } from '../../models';

interface ErrorDisplayProps {
    error: ErrorResponse | null;
    onDismiss?: () => void;
}

export const ErrorDisplay: FC<ErrorDisplayProps> = ({ error, onDismiss }): ReactElement => {
    if (!error) {
        return <></>;
    }

    return (
        <Stack>
            <MessageBar
                messageBarType={MessageBarType.error}
                onDismiss={onDismiss}
                dismissButtonAriaLabel="Close"
            >
                <Stack>
                    <Text>{error.error}</Text>
                    {error.correlation_id && (
                        <Text variant="small" style={{ marginTop: '8px' }}>
                            Correlation ID: {error.correlation_id}
                        </Text>
                    )}
                </Stack>
            </MessageBar>
        </Stack>
    );
};

export default ErrorDisplay;
