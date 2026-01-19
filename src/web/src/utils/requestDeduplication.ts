/**
 * Tracks in-flight requests to prevent duplicate concurrent requests
 */
class RequestDeduplicator {
    private inFlightRequests: Map<string, Promise<unknown>> = new Map();

    /**
     * Executes a request function, deduplicating concurrent requests with the same key
     * 
     * @param key Unique key for the request (e.g., 'GET:/api/story-root')
     * @param fn Function that performs the actual request
     * @returns Promise that resolves/rejects with the request result
     */
    async execute<T>(key: string, fn: () => Promise<T>): Promise<T> {
        // If a request with this key is already in flight, return that promise
        const existingRequest = this.inFlightRequests.get(key);
        if (existingRequest) {
            return existingRequest as Promise<T>;
        }

        // Create new request and track it
        const requestPromise = fn()
            .finally(() => {
                // Remove from tracking once complete (success or failure)
                this.inFlightRequests.delete(key);
            });

        this.inFlightRequests.set(key, requestPromise);

        return requestPromise;
    }

    /**
     * Cancels all in-flight requests (useful for cleanup)
     */
    clear(): void {
        this.inFlightRequests.clear();
    }

    /**
     * Gets the number of in-flight requests
     */
    getPendingCount(): number {
        return this.inFlightRequests.size;
    }
}

// Singleton instance for global request deduplication
export const requestDeduplicator = new RequestDeduplicator();

/**
 * Creates a deduplicated version of an async function
 * 
 * @param fn Function to deduplicate
 * @param keyGenerator Function that generates a unique key for each call
 */
export function deduplicateRequest<TArgs extends unknown[], TReturn>(
    fn: (...args: TArgs) => Promise<TReturn>,
    keyGenerator: (...args: TArgs) => string
): (...args: TArgs) => Promise<TReturn> {
    return async (...args: TArgs): Promise<TReturn> => {
        const key = keyGenerator(...args);
        return requestDeduplicator.execute(key, () => fn(...args));
    };
}
