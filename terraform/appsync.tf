# resource "aws_appsync_graphql_api" "main" {
#   name                = "${var.namespace}-api"
#   authentication_type = "AMAZON_COGNITO_USER_POOLS"

#   user_pool_config {
#     user_pool_id   = "${aws_cognito_user_pool.main.id}"
#     aws_region     = "${var.region}"
#     default_action = "ALLOW"
#   }

#   schema = "${file("../schema.graphql")}"
# }