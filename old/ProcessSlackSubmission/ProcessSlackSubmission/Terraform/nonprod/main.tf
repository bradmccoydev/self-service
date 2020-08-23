//-------------------------------------------VPCs------------------------------------------------//
module "jumpbox_vpc" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/vpc/vpc"
  cidr_block = "192.168.16.0/24"
  vpc_tag = "PROD-Jumpbox-VPC"
}
module "main_vpc" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/vpc/vpc"
  cidr_block = "172.16.0.0/16"
  vpc_tag = "PROD-VPC"
}
module "prod_vpc_peering" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/vpc/vpc_peering"
  jumpbox_vpc = "${module.jumpbox_vpc.vpc_id}"
  main_vpc = "${module.main_vpc.vpc_id}"
  nonprod_vpc_peering_tag_name = "PROD-Jumpbox-VPC-to-PROD-VPC"
}

//-------------------------------------------IGWs------------------------------------------------//

module "jumpbox_igw" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/vpc/internet_gateway"
  vpc_id = "${module.jumpbox_vpc.vpc_id}"
  igw_tag_name = "PROD-Jumpbox-VPC-igw"
}

module "prod_igw" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/vpc/internet_gateway"
  vpc_id = "${module.main_vpc.vpc_id}"
  igw_tag_name = "PROD-VPC-igw"

}

//-----------------------------------------Subnets---------------------------------------//


//----------------------------SUBNET A --------------------------------//
module "jumpbox_subnet_A" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/vpc/subnet"
  cidr_block = "192.168.16.0/26"
  availability_zone = "ap-southeast-2a"
  vpc_id = "${module.jumpbox_vpc.vpc_id}"
  subnet_tag_name = "PROD-Jumpbox-A"
}

//----------------------Jumpbox Route Table & Subnets associations--------------------------------//

module "jumpbox_route_table" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/vpc/route_table"
  vpc_id = "${module.jumpbox_vpc.vpc_id}"
  route_table_tag = "PROD-Jumpbox-rt"
}

module "jumpbox_routes" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/modules/vpc/route"
  cidr_block_1 = "0.0.0.0/0"
  jumpbox_gateway_id = "${module.jumpbox_igw.id}"
  cidr_block_2 = "172.16.0.0/16"
  peering_gateway_id = "${module.prod_vpc_peering.id}"
  route_table = "${module.jumpbox_route_table.id}"
}

module "jumpbox_subnet_association_A" {
  source = "/Users/rmavani/Documents/Avoka-Infra/src/modules/vpc/route_table_association"
  subnet = "${module.jumpbox_subnet_A.id}"
  aws_route_table = "${module.jumpbox_route_table.id}"
}

//-----------------------------------------Security Groups-------------------------------------------------//

module "PROD_Jumpbox_sg" {
  source = "/Users/bmccoy/Development/⁨ProcessSlackSubmission⁩/⁨ProcessSlackSubmission⁩/⁨Terraform⁩/src/modules/security_groups/jumpbox-sg"
  sg_name = "PROD-Jumpbox-secgroup"
  vpc_id = "${module.jumpbox_vpc.vpc_id}"
  sg_description = "PROD-Jumpbox-secgroup"
  sg_tag_name = "PROD-Jumpbox-secgroup"

  ingress_protocol = "tcp"
  ingress_ports = "50022"
  ingress_cidrs = [
    "114.141.100.200/29"]

  egress_protocol = "tcp,tcp,udp,tcp,tcp"
  egress_ports = "50022,50022,123,443,80"
  egress_cidrs = [
    "172.16.128.0/18",
    "172.16.64.0/18",
    "0.0.0.0/0",
    "0.0.0.0/0",
    "0.0.0.0/0"]
}