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

# ---------------------------------------------------------------------------------------------------------------------
# API
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_lambda_function" "SlackSlashCommand" {
    function_name = "SlackSlashCommand"
    description = "Slack Slash Command"
    role          = "${module.self_service_lambda_execution_role.arn}"
    handler       = "build/microservice/golang/SlackSlashCommand/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/golang/SlackSlashCommand/aws/main.zip"
    memory_size = 512
    timeout = 300
    source_code_hash = "${base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackSlashCommand/main.zip"))}"
    environment {
    variables = {
        ShellySigningSecret = "SlackSigningSecret"
        }
    }
}

resource "aws_lambda_function" "SlackProcessSubmission" {
    function_name = "SlackProcessSubmission"
    description = "Slack Process Submission"
    role          = "${module.self_service_lambda_execution_role.arn}"
    handler       = "build/microservice/golang/SlackProcessSubmission/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/golang/SlackSlashCommand/aws/main.zip"
    memory_size = 512
    timeout = 300
    source_code_hash = "${base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackProcessSubmission/main.zip"))}"
    environment {
    variables = {
        ShellySigningSecret = "SlackSigningSecret"
        }
    }
}

resource "aws_lambda_function" "SlackDynamicDataSource" {
    function_name = "SlackDynamicDataSource"
    description = "Slack Dynamic Data Source"
    role          = "${module.self_service_lambda_execution_role.arn}"
    handler       = "build/microservice/golang/SlackDynamicDataSource/main"
    runtime       = "go1.x"
    s3_bucket = var.application_s3_bucket
    s3_key = "microservice/golang/SlackDynamicDataSource/aws/main.zip"
    memory_size = 512
    timeout = 300
    source_code_hash = "${base64encode(sha256("~/Development/bradmccoydev/self-service/build/SlackDynamicDataSource/main.zip"))}"
    environment {
    variables = {
        ShellySigningSecret = "SlackSigningSecret"
        }
    }
}