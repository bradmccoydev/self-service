resource "aws_athena_workgroup" "athena_workgroup" {
  name = "athena_workgroup"

  configuration {
    result_configuration {
      encryption_configuration {
        encryption_option = "SSE_KMS"
        kms_key_arn       = aws_kms_key.athena_kms_key.arn
      }
    }
  }
}

resource "aws_athena_database" "athena_logs" {
  name   = "logs"
  bucket = aws_s3_bucket.terraform_state.id
}

resource "aws_athena_named_query" "athena_query" {
  name      = "query"
  workgroup = aws_athena_workgroup.athena_workgroup.id
  database  = aws_athena_database.athena_logs.name
  query     = "SELECT * FROM ${aws_athena_database.athena_logs.name} limit 10;"
}