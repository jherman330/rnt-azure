export interface VersionMetadata {
    version_id: string;
    user_id: string;
    timestamp: string; // ISO 8601 date string
    source_request_id?: string;
    prior_version_id?: string;
    environment?: string;
    llm_assisted: boolean;
}
