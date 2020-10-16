# ---------------------------------------------------------------------------------------------------------------------
# Cloudfront
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_route53_record" "frontend_record" {
  zone_id = var.route53_zone_id
  name    = var.application_subdomain
  type    = "A"

  alias {
    name = aws_cloudfront_distribution.frontend_cloudfront_distribution.domain_name
    zone_id = aws_cloudfront_distribution.frontend_cloudfront_distribution.hosted_zone_id
    evaluate_target_health = false
  }
}

# ---------------------------------------------------------------------------------------------------------------------
# API
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_route53_record" "api_master" {
  name    = aws_api_gateway_domain_name.master.domain_name
  type    = "A"
  zone_id = var.route53_zone_id

  alias {
    evaluate_target_health = true
    name                   = aws_api_gateway_domain_name.master.cloudfront_domain_name
    zone_id                = aws_api_gateway_domain_name.master.cloudfront_zone_id
  }
}