import { useState, useCallback } from 'react';

export interface ApiCallState<T> {
    data: T | null;
    loading: boolean;
    error: Error | null;
}

export function useApiCall<T>() {
    const [state, setState] = useState<ApiCallState<T>>({
        data: null,
        loading: false,
        error: null,
    });

    const execute = useCallback(async (apiCall: () => Promise<T>) => {
        setState({ data: null, loading: true, error: null });
        try {
            const result = await apiCall();
            setState({ data: result, loading: false, error: null });
            return result;
        } catch (error) {
            const err = error instanceof Error ? error : new Error(String(error));
            setState({ data: null, loading: false, error: err });
            throw err;
        }
    }, []);

    const reset = useCallback(() => {
        setState({ data: null, loading: false, error: null });
    }, []);

    return {
        ...state,
        execute,
        reset,
    };
}
