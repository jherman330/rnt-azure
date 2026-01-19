import { AxiosError } from 'axios';
import { ErrorResponse } from '../models';

/**
 * Types of errors that can occur during API calls
 */
export enum ErrorType {
    /** Transient error - should retry (5xx, network errors) */
    Transient = 'transient',
    /** Actionable error - user should see this (4xx except specific cases) */
    Actionable = 'actionable',
    /** Non-actionable error - expected/valid state, no user notification needed (404 for missing story root, etc.) */
    NonActionable = 'non-actionable',
}

/**
 * Result of error classification
 */
export interface ErrorClassification {
    type: ErrorType;
    shouldRetry: boolean;
    shouldShowToast: boolean;
    message: string;
    correlationId?: string;
}

/**
 * Determines if an HTTP status code represents a transient error
 */
function isTransientStatusCode(status: number): boolean {
    // 5xx server errors are transient (retryable)
    // 429 Too Many Requests is transient (rate limiting)
    // 408 Request Timeout is transient
    return status >= 500 || status === 429 || status === 408;
}

/**
 * Determines if an HTTP status code represents a non-actionable error
 * (expected/valid states that shouldn't trigger user notifications)
 */
function isNonActionableStatusCode(status: number, endpoint?: string): boolean {
    // 404 on story-root GET is expected (now handled by returning empty object, but kept for safety)
    if (status === 404 && endpoint?.includes('/api/story-root') && endpoint.endsWith('/')) {
        return true;
    }
    return false;
}

/**
 * Checks if an error is a network error (no response received)
 */
function isNetworkError(error: AxiosError): boolean {
    return !error.response && (error.code === 'ECONNABORTED' || error.code === 'ERR_NETWORK' || error.message.includes('Network Error'));
}

/**
 * Classifies an API error to determine how it should be handled
 */
export function classifyApiError(error: unknown, endpoint?: string): ErrorClassification {
    const axiosError = error as AxiosError<ErrorResponse>;
    
    // Network errors (no response) are transient
    if (isNetworkError(axiosError)) {
        return {
            type: ErrorType.Transient,
            shouldRetry: true,
            shouldShowToast: false, // Will show toast after max retries
            message: axiosError.message || 'Network error occurred',
        };
    }

    const status = axiosError.response?.status;
    
    if (!status) {
        // Unknown error format
        return {
            type: ErrorType.Actionable,
            shouldRetry: false,
            shouldShowToast: true,
            message: axiosError.message || 'An unexpected error occurred',
            correlationId: axiosError.response?.data?.correlation_id,
        };
    }

    // Non-actionable errors (expected states)
    if (isNonActionableStatusCode(status, endpoint)) {
        return {
            type: ErrorType.NonActionable,
            shouldRetry: false,
            shouldShowToast: false,
            message: axiosError.response?.data?.error || axiosError.message,
            correlationId: axiosError.response?.data?.correlation_id,
        };
    }

    // Transient errors (retryable)
    if (isTransientStatusCode(status)) {
        return {
            type: ErrorType.Transient,
            shouldRetry: true,
            shouldShowToast: false, // Will show toast after max retries
            message: axiosError.response?.data?.error || axiosError.message || `Server error (${status})`,
            correlationId: axiosError.response?.data?.correlation_id,
        };
    }

    // Actionable errors (4xx client errors, except those handled above)
    return {
        type: ErrorType.Actionable,
        shouldRetry: false,
        shouldShowToast: true,
        message: axiosError.response?.data?.error || axiosError.message || `Request failed (${status})`,
        correlationId: axiosError.response?.data?.correlation_id,
    };
}

/**
 * Extracts error message and correlation ID from an error
 */
export function extractErrorDetails(error: unknown): { message: string; correlationId?: string } {
    const axiosError = error as AxiosError<ErrorResponse>;
    
    const message = axiosError.response?.data?.error || axiosError.message || 'An error occurred';
    const correlationId = axiosError.response?.data?.correlation_id;
    
    return { message, correlationId };
}
