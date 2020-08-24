resource "aws_security_group" "aws_security_group" {
  name        = "${var.sg_name}"
  description = "${var.sg_description}"
  vpc_id      = "${var.vpc_id}"

  tags {
    Name         = "${var.sg_tag_name}"
  }
}
resource "aws_security_group_rule" "ingress_cidr" {
  count             = "${var.ingress_ports == "default_null" ? 0 : length(split(",", var.ingress_ports))}"
  type              = "ingress"
  from_port         = "${element(split(",", var.ingress_ports), count.index)}"
  to_port           = "${element(split(",", var.ingress_ports), count.index)}"
  protocol          = "${element(split(",", var.ingress_protocol), count.index)}"
  cidr_blocks       = ["${element(var.ingress_cidrs, count.index)}"]
  security_group_id = "${aws_security_group.aws_security_group.id}"
}


resource "aws_security_group_rule" "egress_sgid" {
  count             = "${var.sgid_egress_ports == "default_null" ? 0 : length(split(",", var.sgid_egress_ports))}"
  type              = "egress"
  from_port         = "${element(split(",", var.sgid_egress_ports), count.index)}"
  to_port           = "${element(split(",", var.sgid_egress_ports), count.index)}"
  protocol          = "${element(split(",", var.sgid_egress_protocol), count.index)}"
  source_security_group_id = "${element(split(",", var.egress_source_sg_id), count.index)}"
  security_group_id = "${aws_security_group.aws_security_group.id}"
}