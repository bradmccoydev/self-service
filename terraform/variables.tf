variable "application_name" {
  description = "Aws Account ID"
}

variable "aws_account_id" {
  description = "Aws Account ID"
}

variable "aws_region" {
  description = "Aws Region"
}

variable "environment" {
  description = "Environment Name"
}

variable "environment_lower" {
  description = "Environment Name Lower"
}

variable "application_s3_bucket" {
  description = "AWS S3 State Bucket"
}

variable "logging_s3_bucket" {
  description = "Logging S3 Bucket"
}

variable "metrics_s3_bucket" {
  description = "Logging S3 Bucket"
}

variable "ui_s3_bucket" {
  description = "UI S3 Bucket"
}

variable "root_domain_name" {
  description = "Root Domain name"
}

variable "application_subdomain" {
  description = "Application Sub Domain"
}

variable "api_subdomain" {
  description = "Api Sub Domain"
}

variable "route53_zone_id" {
  description = "Route 53"
}

variable "ssl_cert" {
  description = "SSL Cert ARN"
}

variable "secret_id" {
  description = "Secret ID"
}

variable "cognito_role_external_id" {
  description = "Cognito Unique ID"
}

variable "tags" {
  description = "AWS Tags"
}