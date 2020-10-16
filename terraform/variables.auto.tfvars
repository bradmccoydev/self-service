application_name = "self-service"
aws_account_id = "142035491160"
aws_region = "us-west-2"
environment = "DEV"
environment_lower = "dev"
cidr_block = "10.0.0.0/16"
public_subnet_cidr_blocks = ["10.0.0.0/24", "10.0.2.0/24"]
private_subnet_cidr_blocks = ["10.0.1.0/24", "10.0.3.0/24"]
availability_zones = ["us-west-2a", "us-west-2b"]
application_s3_bucket = "selfservice.bradmccoy.io"
logging_s3_bucket = "logging.bradmccoy.io"
metrics_s3_bucket = "metrics.bradmccoy.io"
ui_s3_bucket = "ui.bradmccoy.io"
root_domain_name = "bradmccoy.io"
application_subdomain = "selfservice.bradmccoy.io"
api_subdomain = "api.bradmccoy.io"
route53_zone_id = "Z10188742DW2Y8MKNEC3Q"
ssl_cert = "arn:aws:acm:us-east-1:142035491160:certificate/febbde69-448b-47e6-b9b3-81f520397df3"
secret_id = "selfServiceSecret"
cognito_role_external_id = "self-service-01"
tags = {
    "provisioner"   = "terraform"
    "owner"         = "bradmccoy@gmail.com"
    "application"   = "self-service"
    "environment"   = "DEV"
}
