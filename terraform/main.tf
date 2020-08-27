# ---------------------------------------------------------------------------------------------------------------------
# Terraform State
# ---------------------------------------------------------------------------------------------------------------------
terraform {
  backend "s3" {
    bucket         = "selfservice.bradmccoy.io"
    key            = "global/s3/terraform.tfstate"
    region         = "us-west-2"
    dynamodb_table = "terraform_self_service_locks"
    encrypt        = true
  }
}

resource "aws_s3_bucket" "terraform_state" {
  bucket = var.application_s3_bucket
  tags     = var.tags
  versioning {
    enabled = true
  }
  server_side_encryption_configuration {
    rule {
      apply_server_side_encryption_by_default {
        sse_algorithm = "AES256"
      }
    }
  }
}

resource "aws_dynamodb_table" "terraform_self_service_locks" {
  name         = "terraform_self_service_locks"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "LockID"
  tags     = var.tags
  attribute {
    name = "LockID"
    type = "S"
  }
}

resource "null_resource" "deploy" {
  provisioner "local-exec" {
    command = "/bin/bash sudo deploy.sh"
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# Secrets
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_secretsmanager_secret" "app_secret" {
  name = var.secret_id
}

# ---------------------------------------------------------------------------------------------------------------------
# Datastore
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_dynamodb_table" "command" {
  name           = "command"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "command"
  range_key      = "team"
  tags = var.tags
    attribute {
    name = "command"
    type = "S"
  }
    attribute {
    name = "team"
    type = "S"
  }
}

resource "aws_dynamodb_table" "submission" {
  name           = "submission"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "id"
  range_key      = "team"
  tags = var.tags
    attribute {
    name = "id"
    type = "S"
  }
    attribute {
    name = "team"
    type = "S"
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# IAM
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_iam_role" "self_service_role" {
  name = "SelfServiceLambdaExecutionRole"
  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": [
          "lambda.amazonaws.com"
        ]
      },
      "Action": "sts:AssumeRole"
    },
    {
      "Effect": "Allow",
      "Principal": {
        "AWS": [
          "arn:aws:iam::${var.aws_account_id}:root"
        ]
      },
      "Action": "sts:AssumeRole",
      "Condition": {}
    }
  ]
}
EOF
  tags = var.tags
}

resource "aws_iam_policy" "self_service_lambda_execution_policy" {
  name        = "SelfServiceLambdaExecutionPolicy"
  description = "Lambda Execution Policy"
  policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "VisualEditor0",
            "Effect": "Allow",
            "Action": [
                "secretsmanager:GetSecretValue",
                "secretsmanager:DescribeSecret"
            ],
            "Resource": [
                "${aws_secretsmanager_secret.app_secret.arn}"
            ]
        },
        {
            "Sid": "VisualEditor1",
            "Effect": "Allow",
            "Action": [
                "states:*",
                "ec2:CreateNetworkInterface",
                "logs:CreateLogStream",
                "ec2:DescribeNetworkInterfaces",
                "dax:*",
                "kms:*",
                "lambda:*",
                "ec2:DeleteNetworkInterface",
                "logs:CreateLogGroup",
                "logs:PutLogEvents"
            ],
            "Resource": "*"
        },
        {
            "Sid": "VisualEditor2",
            "Effect": "Allow",
            "Action": "dynamodb:*",
            "Resource": [
                "${aws_dynamodb_table.command.arn}",
                "${aws_dynamodb_table.command.arn}/*",
                "${aws_dynamodb_table.submission.arn}",
                "${aws_dynamodb_table.submission.arn}/*"
            ]
        },
        {
            "Sid": "VisualEditor3",
            "Effect": "Allow",
            "Action": "s3:*",
            "Resource": [
                "${aws_s3_bucket.terraform_state.arn}"
            ]
        }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "SelfServiceRolePolicyAttachment" {
  role       = aws_iam_role.self_service_role.name
  policy_arn = aws_iam_policy.self_service_lambda_execution_policy.arn
}

resource "aws_iam_role" "cloudwatch" {
  name = "api_gateway_cloudwatch_global"
  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "",
      "Effect": "Allow",
      "Principal": {
        "Service": "apigateway.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "cloudwatch" {
  name = "default"
  role = aws_iam_role.cloudwatch.id

  policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "logs:CreateLogGroup",
                "logs:CreateLogStream",
                "logs:DescribeLogGroups",
                "logs:DescribeLogStreams",
                "logs:PutLogEvents",
                "logs:GetLogEvents",
                "logs:FilterLogEvents"
            ],
            "Resource": "*"
        }
    ]
}
EOF
}

# ---------------------------------------------------------------------------------------------------------------------
# Lambda
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_lambda_function" "slack_slash_command" {
    function_name = "SlackSlashCommand"
    description = "Slack Slash Command"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/SlackSlashCommand/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/SlackSlashCommand/main.zip"
    memory_size = 3008
    timeout = 300
    //source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackSlashCommand/main.zip"))
    depends_on = [
      null_resource.deploy
    ]
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
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
   depends_on = [
      null_resource.deploy
    ]
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
    depends_on = [
      null_resource.deploy
    ]
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

resource "aws_lambda_function" "endpoint_service" {
    function_name = "EndpointService"
    description = "Endpoint Service"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/EndpointService/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/EndpointService/main.zip"
    memory_size = 3008
    timeout = 300
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/EndpointService/main.zip"))
    depends_on = [
      null_resource.deploy
    ]
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
      }
   }
}

resource "aws_cloudwatch_log_group" "endpoint_service_logs" {
  name              = "/aws/lambda/${aws_lambda_function.endpoint_service.function_name}"
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
    memory_size = 3008
    timeout = 300
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/Logger/main.zip"))
    depends_on = [
      null_resource.deploy
    ]
    environment {
      variables = {
        secret_id = var.secret_id
        region = var.aws_region
        environment = var.environment
      }
   }
}

resource "aws_cloudwatch_log_group" "logger_function_logs" {
  name              = "/aws/lambda/${aws_lambda_function.logger_function.function_name}"
  retention_in_days = 7
}

# ---------------------------------------------------------------------------------------------------------------------
# API
# ---------------------------------------------------------------------------------------------------------------------
resource "aws_api_gateway_account" "api_gw_account" {
  cloudwatch_role_arn = aws_iam_role.cloudwatch.arn
}

resource "aws_api_gateway_rest_api" "api_gateway" {
  name        = "SelfService"
  description = "Self Service API"
  body        = data.template_file.api_swagger.rendered
}

data "template_file" api_swagger{
  template = file("./swagger.yaml")

  vars = {
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
# Messaging
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_sns_topic" "sns_submission" {
  name = "self-service-submission"
  delivery_policy = <<EOF
{
  "http": {
    "defaultHealthyRetryPolicy": {
      "minDelayTarget": 20,
      "maxDelayTarget": 20,
      "numRetries": 3,
      "numMaxDelayRetries": 0,
      "numNoDelayRetries": 0,
      "numMinDelayRetries": 0,
      "backoffFunction": "linear"
    },
    "disableSubscriptionOverrides": false,
    "defaultThrottlePolicy": {
      "maxReceivesPerSecond": 1
    }
  }
}
EOF
}

resource "aws_sqs_queue" "submission_dlq" {
  name = "submission_dlq"
}

resource "aws_sqs_queue" "submission_queue" {
  name                       = "submission_queue"
  redrive_policy             = "{\"deadLetterTargetArn\":\"${aws_sqs_queue.submission_dlq.arn}\",\"maxReceiveCount\":5}"
  visibility_timeout_seconds = 300
}

resource "aws_sns_topic_subscription" "submission_subscription" {
  topic_arn            = "self-service-submission"
  protocol             = "sqs"
  endpoint             = aws_sqs_queue.submission_queue.arn
}

resource "aws_sqs_queue_policy" "submisison_queue_policy" {
  queue_url = aws_sqs_queue.submission_queue.id
  policy    = aws_iam_policy_document.submission_queue_iam_policy.json
}

data "aws_iam_policy_document" "submission_queue_iam_policy" {
  policy_id = "SQSSendAccess"
  statement {
    sid       = "SQSSendAccessStatement"
    effect    = "Allow"
    actions   = ["SQS:SendMessage"]
    resources = [aws_sqs_queue.submission_queue.arn]
    principals {
      identifiers = ["*"]
      type        = "*"
    }
    condition {
      test     = "ArnEquals"
      values   = [aws_sns_topic.sns_submission.arn]
      variable = "aws:SourceArn"
    }
  }
}

resource "aws_lambda_event_source_mapping" "submission_event_source_mapping" {
  batch_size        = 1other
  event_source_arn  = aws_sqs_queue.submission_queue.arn
  enabled           = false
  function_name     = aws_lambda_function.endpoint_service.arn
}