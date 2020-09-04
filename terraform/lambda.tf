# ---------------------------------------------------------------------------------------------------------------------
# Lambda
# Note: If you want to update lambda uncomment source_code_hash this will force code update
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_lambda_function" "slack_slash_command_staging" {
    function_name = "SlackSlashCommand-Staging"
    description = "Slack Slash Command"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/SlackSlashCommand/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/SlackSlashCommand/main.zip"
    memory_size = 3008
    timeout = 300
    //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackSlashCommand/main.zip"))
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
        sns_topic_arn = aws_sns_topic.sns_submission.arn
      }
    }
}

resource "aws_cloudwatch_log_group" "slack_slash_command_logs_staging" {
  name              = "/aws/lambda/${aws_lambda_function.slack_slash_command_staging.function_name}"
  retention_in_days = 7
}

 resource "aws_lambda_function" "slack_slash_command" {
   function_name = "SlackSlashCommand"
   description = "Slack Slash Command"
   role          = aws_iam_role.self_service_role.arn
   handler       = "SlackSlashCommand::SlackSlashCommand.Function::FunctionHandler"
   runtime       = "dotnetcore2.1"
   s3_bucket = var.application_s3_bucket
   s3_key = "microservice/ProcessSlackSubmission/ProcessSlackSubmission.zip"
   memory_size = 3008
   timeout = 60
   //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackSlashCommand/main.zip"))
   environment {
    variables = {
      secret_id = var.secret_id
      region = var.aws_region
      environment = var.environment
      sns_topic_arn = aws_sns_topic.sns_submission.arn
    }
   }
  }  

resource "aws_cloudwatch_log_group" "slack_slash_command_logs" {
  name              = "/aws/lambda/${aws_lambda_function.slack_slash_command.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "process_slack_submission" {
   function_name = "ProcessSlackSubmission"
   description = "Process Slack Submission"
   role          = aws_iam_role.self_service_role.arn
   handler       = "ProcessSlackSubmission::ProcessSlackSubmission.Function::FunctionHandler"
   runtime       = "dotnetcore2.1"
   s3_bucket = var.application_s3_bucket
   s3_key = "microservice/ProcessSlackSubmission/ProcessSlackSubmission.zip"
   memory_size = 3008
   timeout = 60
   //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ProcessSlackSubmission/main.zip"))   
   environment {
    variables = {
      secret_id = var.secret_id
      region = var.aws_region
      environment = var.environment
      sns_topic_arn = aws_sns_topic.sns_submission.arn
    }
   }
}

resource "aws_cloudwatch_log_group" "process_slack_submission_logs" {
  name              = "/aws/lambda/${aws_lambda_function.process_slack_submission.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "slack_dynamic_data_source" {
    function_name = "SlackDynamicDataSource"
    description = "Slack Dynamic Data Source"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/SlackDynamicDataSource/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/SlackDynamicDataSource/main.zip"
    memory_size = 3008
    timeout = 300
    //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackDynamicDataSource/main.zip"))
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
        sns_topic_arn = aws_sns_topic.sns_submission.arn
      }
   }
}

resource "aws_cloudwatch_log_group" "slack_dynamic_data_source_logs" {
  name              = "/aws/lambda/${aws_lambda_function.slack_dynamic_data_source.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "service_invoker" {
    function_name = "ServiceInvoker"
    description = "Endpoint Service Invoker"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ServiceInvoker/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ServiceInvoker/main.zip"
    memory_size = 3008
    timeout = 300
    //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ServiceInvoker/main.zip"))
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
        sns_topic_arn = aws_sns_topic.sns_submission.arn
      }
   }
}

resource "aws_lambda_event_source_mapping" "submission_event_source_mapping" {
  batch_size        = 1
  event_source_arn  = aws_sqs_queue.submission_queue.arn
  enabled           = false
  function_name     = aws_lambda_function.service_invoker.arn
}

resource "aws_cloudwatch_log_group" "service_invoker_logs" {
  name              = "/aws/lambda/${aws_lambda_function.service_invoker.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "logger_function" {
    function_name = "Logger"
    description = "Logger"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/Logger/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/Logger/main.zip"
    memory_size = 512
    timeout = 300
    //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/Logger/main.zip"))
    environment {
      variables = {
        bucket = var.application_s3_bucket
        region = var.aws_region
        environment = var.environment
      }
   }
}

resource "aws_cloudwatch_log_group" "logger_function_logs" {
  name              = "/aws/lambda/${aws_lambda_function.logger_function.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "api_gateway_handler" {
    function_name = "ApiGatewayHandler"
    description = "Api Gateway Handler"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ApiGatewayHandler/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ApiGatewayHandler/main.zip"
    memory_size = 512
    timeout = 300
    //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ApiGatewayHandler/main.zip"))
    environment {
      variables = {
        bucket = var.application_s3_bucket
        region = var.aws_region
        environment = var.environment
      }
   }
}

resource "aws_cloudwatch_log_group" "api_gateway_handler_logs" {
  name              = "/aws/lambda/${aws_lambda_function.api_gateway_handler.function_name}"
  retention_in_days = 7
}