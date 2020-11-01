export const listApplications =
`query listApplications {
  listApplicationMetadataRegistries {
    items {
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
    nextToken
  }
}
`;

export const GetApplication =
`query getApplication {
  getApplicationMetadataRegistry {
    items {
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
    nextToken
  }
}
`;