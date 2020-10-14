# ---------------------------------------------------------------------------------------------------------------------
# Application queue
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_s3_bucket" "metrics" {
  bucket = var.metrics_s3_bucket
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

resource "aws_s3_bucket" "logging" {
  bucket = var.logging_s3_bucket
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

resource "aws_s3_bucket" "ui" {
   bucket = var.ui_s3_bucket
   acl    = "public-read"
   policy = <<POLICY
{
  "Version":"2012-10-17",
  "Statement":[{
    "Sid":"AddPerm",
    "Effect":"Allow",
    "Principal": "*",
    "Action":["s3:GetObject"],
    "Resource":["arn:aws:s3:::${var.application_subdomain}/*"]
  }]
}
POLICY
  website {
    index_document = "index.html"
    error_document = "index.html"
  }
}