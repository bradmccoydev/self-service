resource "aws_sqs_queue" "submission_dlq" {
  name = "submission_dlq"
}

resource "aws_sqs_queue" "submission_queue" {
  name                  = "submission_queue"
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.submission_dlq.arn
    maxReceiveCount     = 4
  })
  visibility_timeout_seconds = 300
  tags     = var.tags
}

resource "aws_sqs_queue_policy" "submisison_queue_policy" {
  queue_url = aws_sqs_queue.submission_queue.id
  policy    = data.aws_iam_policy_document.submission_queue_iam_policy.json
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