# ---------------------------------------------------------------------------------------------------------------------
# Lambda
# Note: If you want to update lambda uncomment source_code_hash this will force code update
# Slack
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
      api_gateway = aws_api_gateway_deployment.api_gateway_deployment_master.id
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

# ---------------------------------------------------------------------------------------------------------------------
# Self Service Lambdas
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_lambda_function" "service_invoker" {
    function_name = "ServiceInvoker"
    description = "Endpoint Service Invoker"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ServiceInvoker/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ServiceInvoker/main.zip"
    memory_size = 256
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ServiceInvoker/main.zip"))
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
        sns_topic_arn = aws_sns_topic.sns_submission.arn
        service_table = aws_dynamodb_table.service.name
        event_table = aws_dynamodb_table.event.name
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
    memory_size = 128
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/Logger/main.zip"))
    environment {
      variables = {
        bucket = var.application_s3_bucket
        region = var.aws_region
        environment = var.environment
        event_table = aws_dynamodb_table.event.name
      }
   }
}

resource "aws_cloudwatch_log_group" "logger_function_logs" {
  name              = "/aws/lambda/${aws_lambda_function.logger_function.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "service_metadata" {
    function_name = "ServiceMetadata"
    description = "Service Metadata"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ServiceMetadata/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ServiceMetadata/main.zip"
    memory_size = 256
    timeout = 30
    //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ServiceMetadata/main.zip"))
    environment {
      variables = {
        bucket = var.application_s3_bucket
        region = var.aws_region
        environment = var.environment
        service_table = aws_dynamodb_table.service.name
        event_table = aws_dynamodb_table.event.name
      }
   }
}

resource "aws_cloudwatch_log_group" "service_metadata_logs" {
  name              = "/aws/lambda/${aws_lambda_function.service_metadata.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "scheduler" {
    function_name = "Scheduler"
    description = "Scheduled"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/Scheduler/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/Scheduler/main.zip"
    memory_size = 256
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/Scheduler/main.zip"))
    environment {
      variables = {
        bucket = var.application_s3_bucket
        region = var.aws_region
        environment = var.environment
        service_table = aws_dynamodb_table.service.name
        master_api_id = aws_api_gateway_rest_api.api_gateway_master.id
        logger_endpoint = "https://${aws_api_gateway_rest_api.api_gateway_master.id}.execute-api.${var.aws_region}.amazonaws.com/${var.environment}/log"
        service_endpoint = "https://${aws_api_gateway_rest_api.api_gateway_master.id}.execute-api.${var.aws_region}.amazonaws.com/${var.environment}/invokeService"
      }
   }
}

resource "aws_cloudwatch_log_group" "scheduler" {
  name              = "/aws/lambda/${aws_lambda_function.scheduler.function_name}"
  retention_in_days = 7
}