# ---------------------------------------------------------------------------------------------------------------------
# Slack API
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_api_gateway_account" "api_gw_account" {
  cloudwatch_role_arn = aws_iam_role.cloudwatch.arn
}

resource "aws_api_gateway_rest_api" "api_gateway" {
  name        = "SelfService"
  description = "Slack Self Service API"
  body        = data.template_file.api_swagger.rendered
}

data "template_file" api_swagger{
  template = file("./slack-swagger.yaml")

  vars = {
    Slack = aws_lambda_function.slack_slash_command_staging.invoke_arn
    SlackSlashCommand = aws_lambda_function.slack_slash_command.invoke_arn
    ProcessSlackSubmission = aws_lambda_function.process_slack_submission.invoke_arn
    SlackDynamicDataSource = aws_lambda_function.slack_dynamic_data_source.invoke_arn
  }
}

resource "aws_api_gateway_deployment" "api-gateway-deployment" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  stage_name  = var.environment
}

resource "aws_lambda_permission" "apigw_permission_slash_command" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.slack_slash_command.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigw_permission_process_submission" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.process_slack_submission.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigw_permission_dynamic_data_source" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.slack_dynamic_data_source.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway.execution_arn}/*/*"
}

# ---------------------------------------------------------------------------------------------------------------------
# Master API
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_api_gateway_rest_api" "api_gateway_master" {
  name        = "MasterAPIGateway"
  description = "Self Service Master API"
  body        = data.template_file.api_swagger.rendered
}

data "template_file" api_swagger_master{
  template = file("./master-swagger.yaml")

  vars = {
    ServiceInvoker = aws_lambda_function.service_invoker.invoke_arn
    ServiceMetadata = aws_lambda_function.service_metadata.invoke_arn
    Logger = aws_lambda_function.logger_function.invoke_arn
  }
}

resource "aws_api_gateway_deployment" "api-gateway-deployment-master" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  stage_name  = var.environment
}

resource "aws_lambda_permission" "apigw_permission_service_metadata" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.service_metadata.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway_master.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigw_permission_service_invoker" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.service_invoker.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway_master.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigw_permission_logger_function" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.logger_function.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway_master.execution_arn}/*/*"
}