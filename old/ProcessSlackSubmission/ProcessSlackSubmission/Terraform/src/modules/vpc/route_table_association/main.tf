resource "aws_route_table_association" "association" {
  subnet_id      = "${var.subnet}"
  route_table_id = "${var.aws_route_table}"
}