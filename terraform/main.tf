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
  tags = var.tags
    attribute {
    name = "command"
    type = "S"
  }
}

resource "aws_dynamodb_table" "submission_table" {
  name           = "submission"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "id"
  tags = var.tags
    attribute {
    name = "id"
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
          "appsync.amazonaws.com",
          "lambda.amazonaws.com"
        ]
      },
      "Action": "sts:AssumeRole"
    },
    {
      "Effect": "Allow",
      "Principal": {
        "AWS": [
          "arn:aws:iam::142035491160:root"
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
                "arn:aws:secretsmanager:us-west-2:142035491160:secret:SelfServiceCredentials"
            ]
        },
        {
            "Sid": "VisualEditor1",
            "Effect": "Allow",
            "Action": [
                "states:*",
                "secretsmanager:*",
                "ec2:CreateNetworkInterface",
                "logs:CreateLogStream",
                "ec2:DescribeNetworkInterfaces",
                "dax:*",
                "kms:*",
                "lambda:*",
                "ec2:DeleteNetworkInterface",
                "logs:CreateLogGroup",
                "logs:PutLogEvents",
                "s3:*"
            ],
            "Resource": "*"
        },
        {
            "Sid": "VisualEditor2",
            "Effect": "Allow",
            "Action": "dynamodb:*",
            "Resource": [
                "arn:aws:dynamodb:us-west-2:142035491160:table/command",
                "arn:aws:dynamodb:us-west-2:142035491160:table/command/*"
            ]
        },
        {
            "Sid": "VisualEditor3",
            "Effect": "Allow",
            "Action": "s3:*",
            "Resource": [
                "arn:aws:s3:::selfservice.bradmccoy.io"
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

# ---------------------------------------------------------------------------------------------------------------------
# Lambda
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_lambda_function" "SlackSlashCommand" {
    function_name = "SlackSlashCommand"
    description = "Slack Slash Command"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/golang/SlackSlashCommand/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/SlackSlashCommand/aws/main.zip"
    memory_size = 3008
    timeout = 300
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackSlashCommand/main.zip"))
    environment {
      variables = {
        secret_id = var.secret_id
      }
    }

resource "aws_lambda_function" "ProcessSlackSubmission" {
   function_name = "ProcessSlackSubmission"
   description = "Process Slack Submission"
   role          = aws_iam_role.self_service_role.arn
   handler       = "ProcessSlackSubmission::ProcessSlackSubmission.Function::FunctionHandler"
   runtime       = "dotnetcore2.1"
   s3_bucket = var.application_s3_bucket
   s3_key = "microservice/ProcessSlackSubmission/aws/ProcessSlackSubmission.zip"
   memory_size = 3008
   timeout = 60
   source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/ProcessSlackSubmission/main.zip"))   
   environment {
    variables = {
      secret_id = var.secret_id
    }
   }

resource "aws_lambda_function" "SlackDynamicDataSource" {
    function_name = "SlackDynamicDataSource"
    description = "Slack Dynamic Data Source"
    role          = aws_iam_role.self_service_role.arn
    handler       = "build/microservice/golang/SlackDynamicDataSource/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/SlackDynamicDataSource/aws/main.zip"
    memory_size = 3008
    timeout = 300
    source_code_hash = base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackDynamicDataSource/main.zip"))
    environment {
      variables = {
        secret_id = var.secret_id
      }
   }

# ---------------------------------------------------------------------------------------------------------------------
# API
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_api_gateway_rest_api" "api-gateway" {
  name        = "SelfService"
  description = "Self Service API"
  body        = data.template_file.api_swagger.rendered
}

data "template_file" api_swagger{
  template = file("./swagger.yaml")

  vars = {
    SlackSlashCommand = aws_lambda_function.SlackSlashCommand.invoke_arn
    ProcessSlackSubmission = aws_lambda_function.ProcessSlackSubmission.invoke_arn
    SlackDynamicDataSource = aws_lambda_function.SlackDynamicDataSource.invoke_arn
  }
}

resource "aws_api_gateway_deployment" "api-gateway-deployment" {
  rest_api_id = "${aws_api_gateway_rest_api.api-gateway.id}"
  stage_name  = "DEV"
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