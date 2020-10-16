# ---------------------------------------------------------------------------------------------------------------------
# Identity Pool
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_cognito_identity_pool" "main" {
  identity_pool_name               = "${var.application_name} ${var.environment}"
  allow_unauthenticated_identities = false

  cognito_identity_providers {
    client_id               = aws_cognito_user_pool_client.web.id
    provider_name           = "cognito-idp.${var.aws_region}.amazonaws.com/${aws_cognito_user_pool.main.id}"
    server_side_token_check = false
  }
}

resource "aws_cognito_identity_pool_roles_attachment" "main" {
  identity_pool_id = aws_cognito_identity_pool.main.id

  roles = {
    "authenticated"   = aws_iam_role.cognito_authenticated.arn
    "unauthenticated" = aws_iam_role.cognito_unauthenticated.arn
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# User Pool
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_cognito_user_pool" "main" {
  name = "${var.application_name}-${var.environment}"

  verification_message_template {
    default_email_option = "CONFIRM_WITH_CODE"
    email_message        = "Self Service: Your verification code is {####}"
    email_subject        = "Self Service Verification Code"
  }

  auto_verified_attributes = ["email"]

  schema {
    attribute_data_type = "String"
    name                = "email"
    required            = true
    mutable             = true

    string_attribute_constraints {
      min_length = 5
      max_length = 300
    }
  }

  password_policy {
    minimum_length    = 10
    require_uppercase = true
    require_lowercase = true
    require_numbers   = true
    require_symbols   = false
  }

  sms_configuration {
    external_id    = var.cognito_role_external_id
    sns_caller_arn = aws_iam_role.cognito_sns_role.arn
  }

  admin_create_user_config {
    allow_admin_create_user_only = true

    invite_message_template {
      sms_message   = "Self Service Invitation. USR: {username} PWD: {####}  https://${var.application_subdomain}"
      email_subject = "Self Service Invitation"

      email_message = <<EOF
You have been added to the Self Service: https://${var.application_subdomain}
Username: {username}
Temporary Password: {####}
You will be asked to change your password after first logging in.
EOF
    }
  }

  tags = var.tags
}

# ---------------------------------------------------------------------------------------------------------------------
# User Groups
# ---------------------------------------------------------------------------------------------------------------------


resource "aws_cognito_user_group" "manager" {
  name         = "manager"
  user_pool_id = aws_cognito_user_pool.main.id
  description  = "Permission to send worker notifications"
}

# ---------------------------------------------------------------------------------------------------------------------
# Clients
# ---------------------------------------------------------------------------------------------------------------------


resource "aws_cognito_user_pool_client" "web" {
  name         = "${var.application_name}-client-web"
  user_pool_id = aws_cognito_user_pool.main.id
}