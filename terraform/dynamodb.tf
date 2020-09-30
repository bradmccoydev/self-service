# ---------------------------------------------------------------------------------------------------------------------
# DynamoDB
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_dynamodb_table" "application" {
  name           = "application"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "application"
  range_key      = "version"
  tags = var.tags
    attribute {
    name = "application"
    type = "S"
  }
    attribute {
    name = "version"
    type = "S"
  }
}

resource "aws_dynamodb_table" "service_catalog" {
  name           = "service_catalog"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "service"
  range_key      = "version"
  tags = var.tags
    attribute {
    name = "service"
    type = "S"
  }
    attribute {
    name = "version"
    type = "S"
  }
}

resource "aws_dynamodb_table" "event" {
  name           = "event"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "id"
  tags = var.tags
    attribute {
    name = "id"
    type = "S"
  }
}

# resource "aws_dynamodb_table_item" "admin" {
#   table_name = aws_dynamodb_table.service.name
#   hash_key   = aws_dynamodb_table.service.hash_key
#   range_key = aws_dynamodb_table.service.range_key

#   item = <<ITEM
# {
#   "app_id": {
#     "S": "Shelly"
#   },
#   "approver_groups": {
#     "SS": [
#       "admin"
#     ]
#   },
#   "approver_users": {
#     "SS": [
#       "brad.mccoy@cevo.com.au",
#       "brad.mccoy@davidjones.com.au"
#     ]
#   },
#   "authorised_groups": {
#     "SS": [
#       "daas"
#     ]
#   },
#   "authorised_users": {
#     "SS": [
#       "brad.mccoy@cevo.com.au",
#       "brad.mccoy@davidjones.com.au"
#     ]
#   },
#   "service": {
#     "S": "admin"
#   },
#   "created_by": {
#     "S": "brad.mccoy@cevo.com.au"
#   },
#   "created_date": {
#     "S": "2020-08-03T01:29:41.617Z"
#   },
#   "description": {
#     "S": "This Is The Admin Console for adding users to commands etc"
#   },
#   "dialog": {
#     "S": "{\"type\": \"select\",\"hint\": \"Slack Slash Command\",\"label\": \"Command\",\"name\": \"command\",\"options\":[{\"label\":\"ipinbound\",\"value\":\"ipinbound\"},{\"label\":\"portaldeployment\",\"value\":\"portaldeployment\"},{\"label\":\"andoncord\",\"value\":\"andoncord\"},{\"label\":\"backlog\",\"value\":\"backlog\"},{\"label\":\"ipoutbound\",\"value\":\"ipoutbound\"},{\"label\":\"generatecsr\",\"value\":\"generatecsr\"},{\"label\":\"dbquery\",\"value\":\"dbquery\"},{\"label\":\"awsconfig\",\"value\":\"awsconfig\"},{\"label\":\"jiraaddusertogroup\",\"value\":\"jiraaddusertogroup\"},{\"label\":\"tmrestart\",\"value\":\"tmrestart\"},{\"label\":\"listipinbound\",\"value\":\"listipinbound\"},{\"label\":\"listipoutbound\",\"value\":\"listipoutbound\"},{\"label\":\"downloadtminstaller\",\"value\":\"downloadtminstaller\"},{\"label\":\"jiracreatecsproject\",\"value\":\"jiracreatecsproject\"},{\"label\":\"disablealerts\",\"value\":\"disablealerts\"},{\"label\":\"parrot\",\"value\":\"parrot\"},{\"label\":\"licensing\",\"value\":\"licensing\"},{\"label\":\"listproxyoutbound\",\"value\":\"listproxyoutbound\"},{\"label\":\"dbqueryalldbs\",\"value\":\"dbqueryalldbs\"},{\"label\":\"template\",\"value\":\"template\"}]},{\"type\": \"text\",\"hint\": \"Temenos Email\",\"label\": \"Email\",\"name\": \"email\"}"
#   },
#   "documentation": {
#     "S": "This command is for adding users to commands.  You will need the email of the person that you want to add, and then the command will add the user to the command. The email should be the temenos email"
#   },
#   "endpoint": {
#     "S": "arn:aws:states:us-west-2:142035491160:stateMachine:AddUserToSlackCommand-Staging"
#   },
#   "last_updated_by": {
#     "S": "brad.mccoy@cevo.com.au"
#   },
#   "last_updated_date": {
#     "S": "2019-10-27T01:29:41.617Z"
#   },
#   "model": {
#     "S": "{\"id\":\"732c52ab-dd31-4853-8c9e-e9c025e0c66f\",\"offsetX\":0,\"offsetY\":0,\"zoom\":100,\"gridSize\":0,\"links\":[],\"nodes\":[{\"id\":\"323773f5-9265-4de0-9cc1-26cb4108a974\",\"type\":\"default\",\"selected\":false,\"x\":218.5,\"y\":132,\"extras\":{},\"ports\":[{\"id\":\"b03178ca-e759-42b6-8c98-aa233f81bd81\",\"type\":\"default\",\"selected\":false,\"name\":\"in-1\",\"parentNode\":\"323773f5-9265-4de0-9cc1-26cb4108a974\",\"links\":[],\"in\":true,\"label\":\"In\"},{\"id\":\"c4a8bb98-20c6-41d4-8cd2-86cd5e780ad0\",\"type\":\"default\",\"selected\":false,\"name\":\"out-1\",\"parentNode\":\"323773f5-9265-4de0-9cc1-26cb4108a974\",\"links\":[],\"in\":false,\"label\":\"Out\"}],\"name\":\"ExecuteMySqlCommand\",\"color\":\"rgb(0,192,255)\"},{\"id\":\"44085bc3-15d4-4c75-920a-4ad9a8e04b29\",\"type\":\"default\",\"selected\":false,\"x\":314.5,\"y\":274,\"extras\":{},\"ports\":[{\"id\":\"b6ac82d6-d2cd-4f28-b82f-48e7ec52e1be\",\"type\":\"default\",\"selected\":false,\"name\":\"in-1\",\"parentNode\":\"44085bc3-15d4-4c75-920a-4ad9a8e04b29\",\"links\":[],\"in\":true,\"label\":\"In\"},{\"id\":\"37cb00fe-a0a1-4fdf-84e3-c8b24999fdce\",\"type\":\"default\",\"selected\":false,\"name\":\"out-1\",\"parentNode\":\"44085bc3-15d4-4c75-920a-4ad9a8e04b29\",\"links\":[],\"in\":false,\"label\":\"Out\"}],\"name\":\"SendSlackMessage\",\"color\":\"rgb(0,192,255)\"}]}"
#   },
#   "orchestration_engine": {
#     "S": "StepFunctions"
#   },
#   "preState": {
#     "S": "'number_of_dialogs':'1','self_service_environment':'production'"
#   },
#   "requires_approval": {
#     "S": "false"
#   },
#   "requires_authorization": {
#     "S": "true"
#   },
#   "team": {
#     "S": "T03JXKJBE"
#   },
#   "title": {
#     "S": "Admin Console"
#   },
#   "type": {
#     "S": "dialog_submission"
#   },
#   "version": {
#     "S": "1"
#   }
# }
# ITEM
# }