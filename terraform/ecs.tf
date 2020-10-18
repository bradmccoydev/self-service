resource "aws_ecs_cluster" "ecs_cluster" {
  name = var.ecs_cluster_name
}


# resource "aws_ecs_task_definition" "ecs_task_definition" {
#   family                   = var.ecs_task_definition
#   task_role_arn            = aws_iam_role.ecs_task_role.arn
#   execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
#   network_mode             = "awsvpc"
#   cpu                      = "1024"
#   memory                   = "1024"
#   requires_compatibilities = ["FARGATE"]
#   }



