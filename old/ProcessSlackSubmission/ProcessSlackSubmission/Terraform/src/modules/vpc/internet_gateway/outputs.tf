output "id" {
  description = "The ID of the VPC"
  value       = "${aws_internet_gateway.internet_gateway.id}"
}
