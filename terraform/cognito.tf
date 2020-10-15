# resource "aws_cognito_user_pool" "test_app" {
#   name = "test app"

#   alias_attributes = ["email", "preferred_username"]

#   schema {
#     attribute_data_type = "String"
#     mutable             = true
#     name                = "nickname"
#     required            = true
#   }

#   password_policy {
#     minimum_length    = "8"
#     require_lowercase = false
#     require_numbers   = false
#     require_symbols   = false
#     require_uppercase = false
#   }

#   mfa_configuration        = "OFF"
#   auto_verified_attributes = ["email"]

#   verification_message_template {
#     default_email_option  = "CONFIRM_WITH_LINK"
#     email_message_by_link = "Your life will be dramatically improved by signing up! {##Click Here##}"
#     email_subject_by_link = "Welcome to to a new world and life!"
#   }
#   email_configuration {
#     reply_to_email_address = "a-email-for-people-to@reply.to"
#   }

#   tags = {
#     project = "No Meat May"
#   }

#   device_configuration {
#     challenge_required_on_new_device      = true
#     device_only_remembered_on_user_prompt = true
#   }
# }

# resource "aws_cognito_user_pool_domain" "test_app" {
#   user_pool_id = "${aws_cognito_user_pool.test_app.id}"
#   # DOMAIN PREFIX
#   domain = "test-app-<add random number on the end>"
# }

# resource "aws_cognito_user_pool_client" "test_app" {
#   user_pool_id = "${aws_cognito_user_pool.test_app.id}"

#   # APP CLIENTS
#   name                   = "test-app-client"
#   refresh_token_validity = 30
#   read_attributes  = ["nickname"]
#   write_attributes = ["nickname"]

#   # APP INTEGRATION -
#   # APP CLIENT SETTINGS
#   supported_identity_providers = ["COGNITO"]
#   callback_urls                = ["http://localhost:3000"]
#   logout_urls                  = ["http://localhost:3000"]
# }

# resource "aws_cognito_identity_pool" "test_app_id_pool" {
#   identity_pool_name               = "test app"
#   allow_unauthenticated_identities = false
#   cognito_identity_providers {
#     client_id               = aws_cognito_user_pool_client.test_app.id
#     provider_name           = aws_cognito_user_pool.test_app.endpoint
#     server_side_token_check = false
#   }

#   supported_login_providers = {
#     "graph.facebook.com" = "<your App ID goes here. Refer to picture at the top>"
#   }
# }


# //https://johncodeinaire.com/aws-cognito-terraform-tutorial/