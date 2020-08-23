
locals {
  aws_account_id = "457972698173"
  aws_region     = "${var.aws_region}"
  environment = "${var.environment}"
  environment_lower = "${var.environment_lower}"
  subnet_id = "subnet-04f6a6c14f90d19cc"
  security_group_id = "sg-05a8fb4959cfb5bed"
}

resource "aws_s3_bucket" "terraform_state" {
  bucket = var.state_s3_bucket
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