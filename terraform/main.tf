# ---------------------------------------------------------------------------------------------------------------------
# Terraform State
# ---------------------------------------------------------------------------------------------------------------------
terraform {
  backend "s3" {
    bucket         = "terraform.bradmccoy.io"
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
# KMS
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_kms_key" "athena_kms_key" {
  deletion_window_in_days = 7
  description             = "Athena KMS Key"
}

# ---------------------------------------------------------------------------------------------------------------------
# Secrets
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_secretsmanager_secret" "app_secret" {
  name = var.secret_id
}