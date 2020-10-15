data "aws_acm_certificate" "ssl_cert" {
   domain   = "*.${var.root_domain_name}"
}

resource "aws_cloudfront_distribution" "frontend_cloudfront_distribution" {
  origin {
    custom_origin_config {
      http_port              = "80"
      https_port             = "443"
      origin_protocol_policy = "http-only"
      origin_ssl_protocols   = ["TLSv1", "TLSv1.1", "TLSv1.2"]
    }
    domain_name = aws_s3_bucket.ui.website_endpoint
    origin_id   = var.application_subdomain
  }
  
  enabled             = true
  is_ipv6_enabled     = true
  default_root_object = "index.html"
  tags = var.tags
  
  default_cache_behavior {
    viewer_protocol_policy = "redirect-to-https"
    compress               = true
    allowed_methods        = ["GET", "HEAD"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = var.application_subdomain
    min_ttl                = 0
    default_ttl            = 86400
    max_ttl                = 31536000
    
    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }
  }
  
  custom_error_response {
    error_caching_min_ttl = 3000
    error_code            = 404
    response_code         = 200
    response_page_path    = "/index.html"
  }
  
  aliases = ["${var.application_subdomain}"]
  
  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }
  
  viewer_certificate {
    acm_certificate_arn = data.aws_acm_certificate.ssl_cert.arn
    ssl_support_method  = "sni-only"
  }
}

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