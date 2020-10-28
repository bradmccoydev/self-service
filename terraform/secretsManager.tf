# ---------------------------------------------------------------------------------------------------------------------
# Application
# ---------------------------------------------------------------------------------------------------------------------

resource "random_string" "random" {
  length = 3
  special = false
  upper = false
  lower = true
  number = false
}

resource "aws_secretsmanager_secret" "app_secret" {
  name = "self-service-${random_string.random.result}"
}

# ---------------------------------------------------------------------------------------------------------------------
# AppSync
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_secretsmanager_secret" "app_sync" {
  name = "app-sync-${random_string.random.result}"
}