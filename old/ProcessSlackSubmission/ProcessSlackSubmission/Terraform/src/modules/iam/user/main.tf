//----------------------------------Avoka User---------------------------------//

resource "aws_iam_user" "Avoka_users" {

  count = "${length(split(",", var.avoka_user_name))}"
  name = "${element(split(",", var.avoka_user_name), count.index)}"
  //name = "${var.avoka_user_name}"
  path = "${var.path}"

  provisioner "local-exec" {
    command = <<EOT
temp_pass=`pwgen -1 -sy 17` &&
echo $temp_pass > ${element(split(",", var.avoka_user_name), count.index)}.txt &&
aws iam create-login-profile --user-name ${element(split(",", var.avoka_user_name), count.index)}  --password $temp_pass --password-reset-required
EOT
  }
}

resource "aws_iam_group_policy" "AvokaIT" {
  name = "AvokaITPolicy"
  group = "${aws_iam_group.AvokaIT.id}"
  //user = "${aws_iam_user.Avoka_users.name}"

  policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Action": [
                "ec2:*",
                "elasticloadbalancing:*",
                "cloudwatch:*",
                "autoscaling:*",
                "route53:*",
                "route53domains:*",
                "s3:*",
                "ses:*",
                "sns:*",
                "iam:*",
                "cloudfront:*",
                "acm:*",
                "logs:*",
		"support:*",
		"rds:*",
		"kms:*",
		"cloudtrail:*",
		"config:*",
		"aws-marketplace:ViewSubscriptions",
                "aws-marketplace:Subscribe",
                "aws-marketplace:Unsubscribe"
            ],
            "Effect": "Allow",
            "Resource": "*"
        }
    ]
}
EOF
}

