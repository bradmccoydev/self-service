resource "aws_route" "external" {
  route_table_id = "${var.route_table}"
  destination_cidr_block = "${var.cidr_block_1}"
  gateway_id = "${var.jumpbox_gateway_id}"
}
resource "aws_route" "internal" {
  route_table_id = "${var.route_table}"
  destination_cidr_block = "${var.cidr_block_2}"
  gateway_id = "${var.peering_gateway_id}"
}
