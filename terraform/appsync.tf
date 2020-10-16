# ---------------------------------------------------------------------------------------------------------------------
# AppSync GraphQL API
# ---------------------------------------------------------------------------------------------------------------------


# resource "aws_appsync_graphql_api" "main" {
#   name                = "${var.application_name}-api"
#   authentication_type = "AMAZON_COGNITO_USER_POOLS"

#   user_pool_config {
#     user_pool_id   = aws_cognito_user_pool.main.id
#     aws_region     = var.aws_region
#     default_action = "ALLOW"
#   }

#   schema = "${file("../schema.graphql")}"
# }

# # ---------------------------------------------------------------------------------------------------------------------
# # Data Sources
# # ---------------------------------------------------------------------------------------------------------------------


# resource "aws_appsync_datasource" "application" {
#   name             = "WorkerTable"
#   api_id           = aws_appsync_graphql_api.main.id
#   service_role_arn = aws_iam_role.appsync_dynamo_datasource.arn
#   type             = "AMAZON_DYNAMODB"

#   dynamodb_config {
#     table_name = aws_dynamodb_table.application.name
#   }
# }

# resource "aws_appsync_datasource" "notifier" {
#   name             = "NotifierFunction"
#   api_id           = aws_appsync_graphql_api.main.id
#   service_role_arn = aws_iam_role.appsync_notifier_lambda_datasource.arn
#   type             = "AWS_LAMBDA"

#   lambda_config {
#     function_arn = var.notifier_fn_arn
#   }
# }

# # ---------------------------------------------------------------------------------------------------------------------
# # Resolvers
# # ---------------------------------------------------------------------------------------------------------------------


# resource "aws_appsync_resolver" "listWorkers" {
#   api_id            = aws_appsync_graphql_api.main.id
#   field             = "listWorkers"
#   type              = "Query"
#   data_source       = aws_appsync_datasource.worker.name
#   request_template  = "${file("../resolvers/Query.listWorkers.req.vtl")}"
#   response_template = "${file("../resolvers/Query.listWorkers.res.vtl")}"
# }

# resource "aws_appsync_resolver" "createWorker" {
#   api_id            = aws_appsync_graphql_api.main.id
#   field             = "createWorker"
#   type              = "Mutation"
#   data_source       = aws_appsync_datasource.worker.name
#   request_template  = "${file("../resolvers/Mutation.createWorker.req.vtl")}"
#   response_template = "${file("../resolvers/Mutation.createWorker.res.vtl")}"
# }

# resource "aws_appsync_resolver" "updateWorker" {
#   api_id            = aws_appsync_graphql_api.main.id
#   field             = "updateWorker"
#   type              = "Mutation"
#   data_source       = aws_appsync_datasource.worker.name
#   request_template  = "${file("../resolvers/Mutation.updateWorker.req.vtl")}"
#   response_template = "${file("../resolvers/Mutation.updateWorker.res.vtl")}"
# }

# resource "aws_appsync_resolver" "deleteWorker" {
#   api_id            = aws_appsync_graphql_api.main.id
#   field             = "deleteWorker"
#   type              = "Mutation"
#   data_source       = aws_appsync_datasource.worker.name
#   request_template  = "${file("../resolvers/Mutation.deleteWorker.req.vtl")}"
#   response_template = "${file("../resolvers/Mutation.deleteWorker.res.vtl")}"
# }

# resource "aws_appsync_resolver" "notifyWorker" {
#   api_id            = aws_appsync_graphql_api.main.id
#   field             = "notifyWorker"
#   type              = "Mutation"
#   data_source       = aws_appsync_datasource.notifier.name
#   request_template  = "${file("../resolvers/Mutation.notifyWorker.req.vtl")}"
#   response_template = "${file("../resolvers/Mutation.notifyWorker.res.vtl")}"
# }