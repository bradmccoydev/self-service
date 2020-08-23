resource "aws_route_table" "route_table" {
  vpc_id           = "${var.vpc_id}"

  tags {
    Name = "${var.route_table_tag}"
  }
}

