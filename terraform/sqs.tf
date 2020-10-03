# ---------------------------------------------------------------------------------------------------------------------
# Application queue
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_sqs_queue" "application_dlq" {
  name = "submission_dlq.fifo"
  fifo_queue                  = true
}

resource "aws_sqs_queue" "application_queue" {
  name                  = "application_queue.fifo"
  fifo_queue                  = true
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.application_dlq.arn
    maxReceiveCount     = 4
  })
  visibility_timeout_seconds = 300
  tags     = var.tags
}

# resource "aws_sqs_queue_policy" "application_queue_policy" {
#   queue_url = aws_sqs_queue.application_queue.id
#   policy    = data.aws_iam_policy_document.application_queue_iam_policy.json
# }

# data "aws_iam_policy_document" "application_queue_iam_policy" {
#   policy_id = "SQSSendAccess"
#   statement {
#     sid       = "SQSSendAccessStatement"
#     effect    = "Allow"
#     actions   = ["SQS:SendMessage"]
#     resources = [aws_sqs_queue.application_queue.arn]
#     principals {
#       identifiers = ["*"]
#       type        = "*"
#     }
#     condition {
#       test     = "ArnEquals"
#       values   = [aws_sns_topic.sns_submission.arn]
#       variable = "aws:SourceArn"
#     }
#   }
# }

# ---------------------------------------------------------------------------------------------------------------------
# Logging queue
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_sqs_queue" "logging_dlq" {
  name = "logging_dlq.fifo"
  fifo_queue                  = true
}

resource "aws_sqs_queue" "logging_queue" {
  name                  = "logging_queue.fifo"
  fifo_queue                  = true
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.logging_dlq.arn
    maxReceiveCount     = 4
  })
  visibility_timeout_seconds = 300
  tags     = var.tags
}