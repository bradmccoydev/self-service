# ---------------------------------------------------------------------------------------------------------------------
# SNS
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

resource "aws_sns_topic_subscription" "submission_subscription" {
  topic_arn            = aws_sns_topic.sns_submission.arn
  protocol             = "sqs"
  endpoint             = aws_sqs_queue.submission_queue.arn
  depends_on = [
    aws_sqs_queue.submission_queue
  ]
}
