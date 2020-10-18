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
      }
    }
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg]
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

resource "aws_lambda_function" "application_consumer" {
    function_name = "ApplicationConsumer"
    description = "Application Consumer Endpoint Service Invoker"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ServiceInvoker/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ServiceInvoker/main.zip"
    memory_size = 256
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ApplicaitonConsumer/main.zip"))
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg.id]
    }
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
        application_table = aws_dynamodb_table.application.name
        application_queue = aws_sqs_queue.application_queue.id
        logging_queue = aws_sqs_queue.logging_queue.id
      }
   }
}

resource "aws_lambda_event_source_mapping" "application_event_source_mapping" {
  batch_size        = 1
  event_source_arn  = aws_sqs_queue.application_queue.arn
  enabled           = true
  function_name     = aws_lambda_function.application_consumer.arn
}

resource "aws_cloudwatch_log_group" "application_consumer_logs" {
  name              = "/aws/lambda/${aws_lambda_function.application_consumer.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "application_controller" {
    function_name = "ApplicationController"
    description = "ApplicationController"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ServiceInvoker/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ServiceInvoker/main.zip"
    memory_size = 256
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ApplicationController/main.zip"))
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg.id]
    }
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
        application_table = aws_dynamodb_table.application.name
        application_queue = aws_sqs_queue.application_queue.id
        logging_queue = aws_sqs_queue.logging_queue.id
      }
   }
}

resource "aws_cloudwatch_log_group" "application_controller_logs" {
  name              = "/aws/lambda/${aws_lambda_function.application_controller.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "cicd_controller" {
    function_name = "CiCdController"
    description = "CI/CD Controller"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ServiceInvoker/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ServiceInvoker/main.zip"
    memory_size = 256
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/CiCdController/main.zip"))
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg.id]
    }
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
        application_table = aws_dynamodb_table.application.name
      }
   }
}

resource "aws_cloudwatch_log_group" "cicd_controller_logs" {
  name              = "/aws/lambda/${aws_lambda_function.cicd_controller.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "logging_consumer" {
    function_name = "LoggingConsumer"
    description = "LoggingConsumer"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/Logger/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/Logger/main.zip"
    memory_size = 128
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/LoggingConsumer/main.zip"))
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg.id]
    }
    environment {
      variables = {
        S3_BUCKET = aws_s3_bucket.logging.id
        S3_PATH = "/framework"
        S3_REGION = var.aws_region
        ENVIRONMENT = var.environment
        LOG_LEVEL = "DEBUG"
      }
   }
}

resource "aws_lambda_event_source_mapping" "logging_event_source_mapping" {
  batch_size        = 1
  event_source_arn  = aws_sqs_queue.logging_queue.arn
  enabled           = true
  function_name     = aws_lambda_function.logging_consumer.arn
}

resource "aws_cloudwatch_log_group" "logger_function_logs" {
  name              = "/aws/lambda/${aws_lambda_function.logging_consumer.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "metrics_consumer" {
    function_name = "MetricsConsumer"
    description = "Metrics Consumer"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/Logger/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/Logger/main.zip"
    memory_size = 128
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/LoggingConsumer/main.zip"))
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg.id]
    }
    environment {
      variables = {
        S3_BUCKET = aws_s3_bucket.metrics.id
        S3_PATH = "/framework"
        S3_REGION = var.aws_region
        ENVIRONMENT = var.environment
        LOG_LEVEL = "DEBUG"
      }
   }
}

resource "aws_lambda_event_source_mapping" "metrics_event_source_mapping" {
  batch_size        = 1
  event_source_arn  = aws_sqs_queue.metrics_queue.arn
  enabled           = true
  function_name     = aws_lambda_function.metrics_consumer.arn
}

resource "aws_cloudwatch_log_group" "metrics_function_logs" {
  name              = "/aws/lambda/${aws_lambda_function.metrics_consumer.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "ui_controller" {
    function_name = "UiController"
    description = "UiController"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/ServiceMetadata/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/ServiceMetadata/main.zip"
    memory_size = 256
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/UiController/main.zip"))
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg.id]
    }
    environment {
      variables = {
        bucket = var.application_s3_bucket
        region = var.aws_region
        environment = var.environment
        application_table = aws_dynamodb_table.application.name
      }
   }
}

resource "aws_cloudwatch_log_group" "service_metadata_logs" {
  name              = "/aws/lambda/${aws_lambda_function.ui_controller.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "scheduling_producer" {
    function_name = "SchedulingProducer"
    description = "Scheduling Producer"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/Scheduler/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/Scheduler/main.zip"
    memory_size = 256
    timeout = 30
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/Scheduler/main.zip"))
    vpc_config {
      subnet_ids = [aws_subnet.private_1.id]
      security_group_ids = [aws_security_group.vpc-sg.id]
    }
    environment {
      variables = {
        bucket = var.application_s3_bucket
        region = var.aws_region
        environment = var.environment
        application_table = aws_dynamodb_table.application.name
        application_queue = aws_sqs_queue.application_queue.id
        logging_queue = aws_sqs_queue.logging_queue.id
        logger_endpoint = "https://${aws_api_gateway_rest_api.api_gateway_master.id}.execute-api.${var.aws_region}.amazonaws.com/${var.environment}/log"
        service_endpoint = "https://${aws_api_gateway_rest_api.api_gateway_master.id}.execute-api.${var.aws_region}.amazonaws.com/${var.environment}/invokeService"
      }
   }
}

resource "aws_cloudwatch_log_group" "scheduler" {
  name              = "/aws/lambda/${aws_lambda_function.scheduling_producer.function_name}"
  retention_in_days = 7
}