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

resource "aws_athena_database" "logs" {
  name   = "logs"
  bucket = aws_s3_bucket.logging.bucket
}

resource "aws_athena_database" "metrics" {
  name   = "metrics"
  bucket = aws_s3_bucket.metrics.bucket
}

resource "aws_athena_named_query" "athena_query" {
  name      = "query"
  workgroup = aws_athena_workgroup.athena_workgroup.id
  database  = aws_athena_database.logs.name
  query     = "SELECT * FROM ${aws_athena_database.logs.name} limit 10;"
}