# ---------------------------------------------------------------------------------------------------------------------
# AppSync GraphQL API
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_appsync_graphql_api" "main" {
  name                = "${var.application_name}-api"
  authentication_type = "API_KEY"

  additional_authentication_provider {
    authentication_type = "AWS_IAM"
  }

  #authentication_type = "AMAZON_COGNITO_USER_POOLS"

  # user_pool_config {
  #   user_pool_id   = aws_cognito_user_pool.main.id
  #   aws_region     = var.aws_region
  #   default_action = "ALLOW"
  # }

  log_config {
    cloudwatch_logs_role_arn = aws_iam_role.appsync.arn
    field_log_level          = "ERROR"
  }


  schema = file("./../src/api/schema.graphql")
}

resource "aws_appsync_api_key" "key" {
  api_id  = aws_appsync_graphql_api.main.id
}

# ---------------------------------------------------------------------------------------------------------------------
# Data Sources
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_appsync_datasource" "application" {
  name             = "ApplicationTable"
  api_id           = aws_appsync_graphql_api.main.id
  service_role_arn = aws_iam_role.appsync.arn
  type             = "AMAZON_DYNAMODB"

  dynamodb_config {
    table_name = aws_dynamodb_table.application.name
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# Resolvers
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_appsync_resolver" "get_application_metadata_registry" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "getApplicationMetadataRegistry"
  type              = "Query"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("./../src/api/resolvers/Query.getApplicationMetadataRegistry.req.vtl")
  response_template = <<EOF
  $util.toJson($ctx.result)
  EOF
}

resource "aws_appsync_resolver" "list_application_metadata_registry" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "listApplicationMetadataRegistries"
  type              = "Query"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("./../src/api/resolvers/Query.listApplicationMetadataRegistries.req.vtl")
  response_template = <<EOF
  $util.toJson($ctx.result)
  EOF
}

resource "aws_appsync_resolver" "create_application_metadata" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "createApplicationMetadataRegistry"
  type              = "Mutation"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("./../src/api/resolvers/Mutation.createApplicationMetadataRegistry.req.vtl")
  response_template = <<EOF
  $util.toJson($ctx.result)
  EOF
}

resource "aws_appsync_resolver" "update_application_metadata" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "updateApplicationMetadataRegistry"
  type              = "Mutation"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("./../src/api/resolvers/Mutation.updateApplicationMetadataRegistry.req.vtl")
  response_template = <<EOF
  $util.toJson($ctx.result)
  EOF
}

resource "aws_appsync_resolver" "delete_application_metadata" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "deleteApplicationMetadataRegistry"
  type              = "Mutation"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("./../src/api/resolvers/Mutation.deleteApplicationMetadataRegistry.req.vtl")
  response_template = <<EOF
  $util.toJson($ctx.result)
  EOF
}