output "id" {
  description = "The ID of the VPC"
  value       = "${aws_route_table_association.association.id}"
}
