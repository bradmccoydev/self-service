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

resource "aws_api_gateway_deployment" "api_gateway_deployment" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  stage_name  = var.environment
}

resource "aws_api_gateway_method_settings" "general_settings" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  stage_name  = aws_api_gateway_deployment.api_gateway_deployment.stage_name
  method_path = "*/*"

  settings {
    metrics_enabled        = true
    data_trace_enabled     = true
    logging_level          = "INFO"
    throttling_rate_limit  = 100
    throttling_burst_limit = 50
  }
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
  body        = data.template_file.api_swagger_master.rendered
}

resource "aws_api_gateway_domain_name" "master" {
  certificate_arn = var.ssl_cert
  domain_name     = "api.bradmccoy.io"
}

resource "aws_route53_record" "api_master" {
  name    = aws_api_gateway_domain_name.master.domain_name
  type    = "A"
  zone_id = var.route53_zone_id

  alias {
    evaluate_target_health = true
    name                   = aws_api_gateway_domain_name.master.cloudfront_domain_name
    zone_id                = aws_api_gateway_domain_name.master.cloudfront_zone_id
  }
}

data "template_file" api_swagger_master{
  template = file("./master-swagger.yaml")
  vars = {
    ServiceInvoker = aws_lambda_function.application_controller.invoke_arn
    ServiceMetadata = aws_lambda_function.ui_controller.invoke_arn
    Logger = aws_lambda_function.logging_consumer.invoke_arn
  }
}

resource "aws_api_gateway_deployment" "api_gateway_deployment_master" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway_master.id
  stage_name  = var.environment
}

resource "aws_api_gateway_method_settings" "general_settings_master" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway_master.id
  stage_name  = aws_api_gateway_deployment.api_gateway_deployment_master.stage_name
  method_path = "*/*"

  settings {
    metrics_enabled        = true
    data_trace_enabled     = true
    logging_level          = "INFO"
    throttling_rate_limit  = 100
    throttling_burst_limit = 50
  }
}

resource "aws_lambda_permission" "apigw_permission_service_metadata" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.ui_controller.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway_master.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigw_permission_service_invoker" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.application_controller.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway_master.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigw_permission_logger_function" {
   statement_id  = "AllowAPIGatewayInvoke"
   action        = "lambda:InvokeFunction"
   function_name = aws_lambda_function.logging_consumer.function_name
   principal     = "apigateway.amazonaws.com"
   source_arn = "${aws_api_gateway_rest_api.api_gateway_master.execution_arn}/*/*"
}