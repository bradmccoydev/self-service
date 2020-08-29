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
          "lambda.amazonaws.com"
        ]
      },
      "Action": "sts:AssumeRole"
    },
    {
      "Effect": "Allow",
      "Principal": {
        "AWS": [
          "arn:aws:iam::${var.aws_account_id}:root"
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
                "${aws_secretsmanager_secret.app_secret.arn}"
            ]
        },
        {
            "Sid": "VisualEditor1",
            "Effect": "Allow",
            "Action": [
                "states:*",
                "ec2:CreateNetworkInterface",
                "logs:CreateLogStream",
                "ec2:DescribeNetworkInterfaces",
                "dax:*",
                "kms:*",
                "lambda:*",
                "ec2:DeleteNetworkInterface",
                "logs:CreateLogGroup",
                "logs:PutLogEvents"
            ],
            "Resource": "*"
        },
        {
            "Sid": "VisualEditor2",
            "Effect": "Allow",
            "Action": "dynamodb:*",
            "Resource": [
                "${aws_dynamodb_table.command.arn}",
                "${aws_dynamodb_table.command.arn}/*",
                "${aws_dynamodb_table.submission.arn}",
                "${aws_dynamodb_table.submission.arn}/*"
            ]
        },
        {
            "Sid": "VisualEditor3",
            "Effect": "Allow",
            "Action": "s3:*",
            "Resource": [
                "${aws_s3_bucket.terraform_state.arn}"
            ]
        },
        {
            "Sid": "VisualEditor4",
            "Effect": "Allow",
            "Action": "sqs:*",
            "Resource": [
                "${aws_sqs_queue.submission_queue.arn}"
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

resource "aws_iam_role" "cloudwatch" {
  name = "api_gateway_cloudwatch_global"
  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "",
      "Effect": "Allow",
      "Principal": {
        "Service": "apigateway.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "cloudwatch" {
  name = "default"
  role = aws_iam_role.cloudwatch.id

  policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "logs:CreateLogGroup",
                "logs:CreateLogStream",
                "logs:DescribeLogGroups",
                "logs:DescribeLogStreams",
                "logs:PutLogEvents",
                "logs:GetLogEvents",
                "logs:FilterLogEvents"
            ],
            "Resource": "*"
        }
    ]
}
EOF
}
