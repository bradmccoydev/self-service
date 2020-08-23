//-----------------------------Internet Gateway----------------------//
resource "aws_internet_gateway" "internet_gateway" {

  vpc_id = "${var.vpc_id}"

  tags = {
    Name = "${var.igw_tag_name}"
  }
}