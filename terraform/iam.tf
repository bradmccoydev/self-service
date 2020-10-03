# ---------------------------------------------------------------------------------------------------------------------
# Lambda Execution Role
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
                "ec2:DescribeNetworkInterfaces",
                "ec2:DeleteNetworkInterface",
                "sqs:SendMessage",
                "sqs:ReceiveMessage",
                "sqs:DeleteMessage",
                "sqs:GetQueueAttributes",
                "sqs:ChangeMessageVisibility",
                "dax:*",
                "kms:*",
                "sqs:*
                "lambda:*",
                "logs:CreateLogGroup",
                "logs:PutLogEvents",
                "logs:CreateLogStream",
            ],
            "Resource": "*"
        },
        {
            "Sid": "VisualEditor2",
            "Effect": "Allow",
            "Action": "dynamodb:*",
            "Resource": [
                "${aws_dynamodb_table.service_catalog.arn}",
                "${aws_dynamodb_table.service_catalog.arn}/*",
                "${aws_dynamodb_table.application.arn}",
                "${aws_dynamodb_table.application.arn}/*",
                "${aws_dynamodb_table.event.arn}",
                "${aws_dynamodb_table.event.arn}/*"
            ]
        },
        {
            "Sid": "VisualEditor3",
            "Effect": "Allow",
            "Action": "s3:*",
            "Resource": [
                "${aws_s3_bucket.terraform_state.arn}"
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

resource "aws_iam_role_policy_attachment" "self_service_api_invoke_attachment" {
  role       = aws_iam_role.self_service_role.name
  policy_arn = aws_iam_policy.self_service_api_invoke_policy.arn
}

# resource "aws_iam_user" "developer" {
#   name = "developer"
#   path = "/"

#   tags = {
#     environment = "Dev"
#   }
# }

# resource "aws_iam_user_login_profile" "developer" {
#   user    = aws_iam_user.developer.name
#   pgp_key = "keybase:bmccoy"
# }

# output "password" {
#   value = aws_iam_user_login_profile.developer.encrypted_password
# }

# resource "aws_iam_access_key" "developer_iam" {
#   user = aws_iam_user.developer.name
# }

# resource "aws_iam_user_policy_attachment" "developer_role_attachment" {
#   user       = aws_iam_user.developer.name
#   policy_arn = aws_iam_policy.developer_policy.arn
# }

# resource "aws_iam_policy" "developer_policy" {
#   name        = "DeveloperPolicy"
#   description = "Developer Policy"
#   policy = <<EOF
# {
#     "Version": "2012-10-17",
#     "Statement": [
#         {
#             "Sid": "SecretsManager",
#             "Effect": "Allow",
#             "Action": [
#                 "secretsmanager:GetSecretValue",
#                 "secretsmanager:DescribeSecret",
#                 "iam:CreateAccessKey",
#                 "iam:ListAccessKeys"
#             ],
#             "Resource": [
#                 "arn:aws:iam::142035491160:user/wonboyn@gmail.com",
#                 "${aws_secretsmanager_secret.app_secret.arn}"
#             ]
#         },
#         {
#             "Sid": "VisualEditor1",
#             "Effect": "Allow",
#             "Action": [
#                 "sns:*",
#                 "lambda:*",
#                 "sqs:*"
#             ],
#             "Resource": [
#                 "${aws_lambda_function.application_consumer.arn}",
#                 "${aws_lambda_function.application_controller.arn}",
#                 "${aws_lambda_function.cicd_controller.arn}",
#                 "${aws_lambda_function.logging_consumer.arn}",
#                 "${aws_lambda_function.ui_controller.arn}",
#                 "${aws_lambda_function.scheduling_producer.arn}",
#                 "${aws_lambda_function.slack_slash_command_staging.arn}",
#                 "${aws_lambda_function.slack_slash_command.arn}",
#                 "${aws_lambda_function.process_slack_submission.arn}",
#                 "${aws_lambda_function.slack_dynamic_data_source.arn}",
#                 "${aws_sns_topic.sns_submission.arn}",
#                 "${aws_sqs_queue.application_queue.arn}",
#                 "${aws_sqs_queue.logging_queue.arn}",
#                 "${aws_sqs_queue.application_dlq.arn}",
#                 "${aws_sqs_queue.logging_dlq.arn}"
#             ]
#         },
#         {
#             "Sid": "VisualEditor5",
#             "Effect": "Allow",
#             "Action": [
#                 "sns:ListTopics",
#                 "sns:CreatePlatformEndpoint",
#                 "sns:Unsubscribe",
#                 "lambda:*",
#                 "states:*",
#                 "ec2:CreateNetworkInterface",
#                 "ec2:DescribeNetworkInterfaces",
#                 "ec2:DeleteNetworkInterface",
#                 "sns:CheckIfPhoneNumberIsOptedOut",
#                 "sns:OptInPhoneNumber",
#                 "sns:SetEndpointAttributes",
#                 "sns:ListEndpointsByPlatformApplication",
#                 "sns:DeletePlatformApplication",
#                 "sns:SetPlatformApplicationAttributes",
#                 "sqs:ListQueues",
#                 "sns:CreatePlatformApplication",
#                 "sns:SetSMSAttributes",
#                 "sns:GetPlatformApplicationAttributes",
#                 "sns:GetSubscriptionAttributes",
#                 "sns:ListSubscriptions",
#                 "sns:DeleteEndpoint",
#                 "sns:ListPhoneNumbersOptedOut",
#                 "sns:GetEndpointAttributes",
#                 "sns:SetSubscriptionAttributes",
#                 "sns:ListPlatformApplications",
#                 "sns:GetSMSAttributes"
#             ],
#             "Resource": "*"
#         },
#         {
#             "Sid": "VisualEditor2",
#             "Effect": "Allow",
#             "Action": "dynamodb:*",
#             "Resource": [
#                 "${aws_dynamodb_table.service_catalog.arn}",
#                 "${aws_dynamodb_table.service_catalog.arn}/*",
#                 "${aws_dynamodb_table.application.arn}",
#                 "${aws_dynamodb_table.application.arn}/*",
#                 "${aws_dynamodb_table.event.arn}",
#                 "${aws_dynamodb_table.event.arn}/*"
#             ]
#         },
#         {
#             "Sid": "S3Buckets",
#             "Effect": "Allow",
#             "Action": "s3:*",
#             "Resource": [
#                 "${aws_s3_bucket.terraform_state.arn}"
#             ]
#         },
#         {
#             "Sid": "SqsQueues",
#             "Effect": "Allow",
#             "Action": "sqs:*",
#             "Resource": [
#                 "${aws_sqs_queue.application_queue.arn}",
#                 "${aws_sqs_queue.logging_queue.arn}"
#             ]
#         }
#     ]
# }
# EOF
# }

# ---------------------------------------------------------------------------------------------------------------------
# Cloudwatch Role
# ---------------------------------------------------------------------------------------------------------------------

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

# ---------------------------------------------------------------------------------------------------------------------
# State Machine
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_iam_policy" "self_service_state_execution_policy" {
  name        = "SelfServiceStateExecutionPolicy"
  description = "Step Function Execution Policy"
  policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "lambda:InvokeFunction"
            ],
            "Resource": "*"
        }
    ]
}
EOF
}

resource "aws_iam_role" "slack_role" {
  name = "SlackRole"
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

resource "aws_iam_policy" "self_service_api_invoke_policy" {
  name        = "SelfServiceApiInvokePolicy"
  description = "Permission To Invoke API"
  policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "execute-api:Invoke",
                "execute-api:InvalidateCache"
            ],
            "Resource":[
               "${aws_api_gateway_rest_api.api_gateway.arn}/*",
               "${aws_api_gateway_rest_api.api_gateway_master.arn}/*",
               "${aws_api_gateway_rest_api.api_gateway_master.arn}/*/POST/log",
               "${aws_api_gateway_rest_api.api_gateway_master.arn}/*/POST/invokeService",
               "arn:aws:execute-api:us-west-2:142035491160:wz6j6e66o2/*/POST/log",
               "arn:aws:execute-api:us-west-2:142035491160:wz6j6e66o2/*/POST/invokeService",
               "arn:aws:execute-api:us-west-2:142035491160:wz6j6e66o2/*/*/*/*"
            ]
        }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "slack_api_gateway_role_policy_attachment" {
  role       = aws_iam_role.slack_role.name
  policy_arn = aws_iam_policy.self_service_api_invoke_policy.arn
}

resource "aws_iam_role" "state_execution_role" {
  name = "SelfServiceStateExecutionRole"
  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "states.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF
  tags     = var.tags
}

resource "aws_iam_role_policy_attachment" "self_service_state_execution_role_policy_attachment" {
  role       = aws_iam_role.state_execution_role.name
  policy_arn = aws_iam_policy.self_service_state_execution_policy.arn
}
