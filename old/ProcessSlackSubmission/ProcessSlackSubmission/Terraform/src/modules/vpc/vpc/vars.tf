
variable "cidr_block" {}

variable "enable_dns_support" {
  default = "true"
}
variable "enable_dns_hostnames" {
  default = "true"
}
variable "vpc_tag" {}