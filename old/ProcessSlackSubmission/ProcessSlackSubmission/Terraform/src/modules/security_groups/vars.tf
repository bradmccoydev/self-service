variable "ingress_ports" {
  default = "default_null"
}

variable "egress_ports" {
  default = "default_null"
}

variable "ingress_cidrs" {
  type = "list"
}

variable "ingress_protocol" {}



variable "sg_name" {}
variable "sg_description" {}
variable "vpc_id" {}
variable "sg_tag_name" {}

variable "sgid_egress_ports" {}
variable "sgid_egress_protocol" {}

variable "egress_source_sg_id" {}
