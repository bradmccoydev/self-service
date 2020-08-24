//--------------------------VPC---------------------------------//

resource "aws_vpc" "vpc" {
  cidr_block = "${var.cidr_block}"
  enable_dns_support = "${var.enable_dns_support}"
  enable_dns_hostnames = "${var.enable_dns_hostnames}"

  tags {
    Name = "${var.vpc_tag}"
  }
}



//-------------------Revoke Default SG access------------------//

//resource "null_resource" "default_sg" {
//
//  provisioner "local-exec" {
//    command = <<EOT
//default_secGroupId=`aws ec2 describe-security-groups --group-name default --output text | grep SECURITYGROUPS | cut -f3 &&
//aws ec2 revoke-security-group-ingress --group-id $default_secGroupId --protocol all --port all --source-group $default_secGroupId &&
//aws ec2 create-tags --resources $default_secGroupId --tags Key=Name,Value=DONOTUSE
//EOT
//  }
//}