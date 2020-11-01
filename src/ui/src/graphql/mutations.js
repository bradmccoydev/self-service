export const createApplicationMetadataRegistry =
`mutation createApplication($input: CreateApplicationMetadataRegistryInput!) {
    createApplicationMetadataRegistry(input: $input) {
        created_by
        created_date
        cron_schedule
        description
        documentation
        endpoint
        endpoint_id
        id
        last_executed
        last_updated_by
        last_updated_date
        locked
        parameters
        schema_definition
        team
        title
        type
        version
    }
}`;

export const updateApplicationMetadataRegistry =
`mutation updateApplication($input: UpdateApplicationMetadataRegistryInput!) {
    updateApplicationMetadataRegistry(input: $input) {
        created_by
        created_date
        cron_schedule
        description
        documentation
        endpoint
        endpoint_id
        id
        last_executed
        last_updated_by
        last_updated_date
        locked
        parameters
        schema_definition
        team
        title
        type
        version
    }
}`;

export const deleteApplicationMetadataRegistry =
`mutation deleteApplication($input: DeleteApplicationMetadataRegistryInput!) {
    deleteApplicationMetadataRegistry(input: $input) {
        id
    }
}
`;
