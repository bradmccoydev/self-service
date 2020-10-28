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


  schema = file("../schema.graphql")
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
  service_role_arn = aws_iam_role.appsync_dynamo_datasource.arn
  type             = "AMAZON_DYNAMODB"

  dynamodb_config {
    table_name = aws_dynamodb_table.application.name
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# Resolvers
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_appsync_resolver" "listApplications" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "listApplications"
  type              = "Query"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("../resolvers/Query.listApplications.req.vtl")
  response_template = file("../resolvers/Query.listApplications.res.vtl")
}

resource "aws_appsync_resolver" "createApplication" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "createWorker"
  type              = "Mutation"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("../resolvers/Mutation.createApplication.req.vtl")
  response_template = file("../resolvers/Mutation.createApplication.res.vtl")
}

resource "aws_appsync_resolver" "updateApplication" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "updateWorker"
  type              = "Mutation"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("../resolvers/Mutation.updateApplication.req.vtl")
  response_template = file("../resolvers/Mutation.updateApplication.res.vtl")
}

resource "aws_appsync_resolver" "deleteApplication" {
  api_id            = aws_appsync_graphql_api.main.id
  field             = "deleteWorker"
  type              = "Mutation"
  data_source       = aws_appsync_datasource.application.name
  request_template  = file("../resolvers/Mutation.deleteApplication.req.vtl")
  response_template = file("../resolvers/Mutation.deleteApplication.res.vtl")
}