import { classifyApiError } from './apiErrorHandler';

/**
 * Configuration for retry logic
 */
export interface RetryConfig {
    /** Maximum number of retry attempts */
    maxRetries: number;
    /** Initial delay in milliseconds before first retry */
    initialDelayMs: number;
    /** Maximum delay in milliseconds (exponential backoff cap) */
    maxDelayMs: number;
    /** Multiplier for exponential backoff */
    backoffMultiplier: number;
    /** Optional jitter to add to delays (prevents thundering herd) */
    jitterMs?: number;
}

/**
 * Default retry configuration
 */
const DEFAULT_RETRY_CONFIG: RetryConfig = {
    maxRetries: 3,
    initialDelayMs: 500,
    maxDelayMs: 5000,
    backoffMultiplier: 2,
    jitterMs: 100,
};

/**
 * Calculates delay with exponential backoff and optional jitter
 */
function calculateDelay(attempt: number, config: RetryConfig): number {
    const exponentialDelay = Math.min(
        config.initialDelayMs * Math.pow(config.backoffMultiplier, attempt),
        config.maxDelayMs
    );
    
    const jitter = config.jitterMs ? Math.random() * config.jitterMs : 0;
    
    return exponentialDelay + jitter;
}

/**
 * Waits for the specified number of milliseconds
 */
function sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Executes a function with bounded retry logic for transient failures only
 * 
 * @param fn Function to execute (should return a Promise)
 * @param endpoint Optional endpoint URL for error classification
 * @param config Retry configuration (uses defaults if not provided)
 * @returns Promise that resolves with the function result or rejects after max retries
 */
export async function withRetry<T>(
    fn: () => Promise<T>,
    endpoint?: string,
    config: Partial<RetryConfig> = {}
): Promise<T> {
    const retryConfig: RetryConfig = { ...DEFAULT_RETRY_CONFIG, ...config };
    let lastError: unknown;

    for (let attempt = 0; attempt <= retryConfig.maxRetries; attempt++) {
        try {
            return await fn();
        } catch (error) {
            lastError = error;
            
            // Classify the error
            const classification = classifyApiError(error, endpoint);
            
            // Only retry transient errors
            if (!classification.shouldRetry || attempt >= retryConfig.maxRetries) {
                // If we've exhausted retries, throw the last error
                throw error;
            }
            
            // Calculate delay and wait before retrying
            const delay = calculateDelay(attempt, retryConfig);
            await sleep(delay);
        }
    }

    // This should never be reached, but TypeScript needs it
    throw lastError;
}
