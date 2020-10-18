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

variable "cidr_block" {
  description = "VPC Cidr block"
}

variable "public_subnet_cidr_block_1" {
  description = "public subnet 1 CIDR block"
}

variable "public_subnet_cidr_block_2" {
  description = "public subnet 2 CIDR block"
}


variable "private_subnet_cidr_block_1" {
  description = "private subnet 1 CIDR block"
}

variable "private_subnet_cidr_block_2" {
  description = "private subnet 2 CIDR block"
}

variable "availability_zone_1" {
  description = "Availability zone 1"
}

variable "availability_zone_2" {
  description = "Availability zone 2"
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

variable "ecs_cluster_name" {
  description = "AWS ECS Cluster Name"
}

variable "ecs_task_execution_role" {
  description = "AWS ECS Task Execution Role Name"
}

variable "ecs_task_definition" {
  description = "AWS ECS Task Definition Name"
}