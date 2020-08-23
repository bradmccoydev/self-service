# AWS Lambda Process Slack Submission


## Environment Variables
All sensitive environment variables will be encrypted with KMS.  They will be encrypted at rest and in transit, and decrypted when the lambda function is executed.

Access Key - AWS Access Key
Secret Key - AWS Secret Key
Region - AWS Region
SlackToken - Slack App Token
AuthToken - Slack App Token
SigningSecret - Signing secret for verifying requests
SlackWebhook - Slack Web Hook for logging errors

## Security
There are several layers of security within this app.

1. Api tokens are restricted to our Lambdas IP address.
2. Requests are verified with verifying the HTTP timestamp header to make sure that it has been made within 20 seconds.
3. Requests are verified by matching the slack signature.  This is made by getting the Slack signing secret that is encrypted in the environment variable and then creating the signature by concatenating the slack request timestamp, and the payload it then gets the HMAC SHA 256 and matches it against the HTTP header.  If this doesnt match then the app wont proceed.
4. There is also a list of approved people that can use the slack slash command, this information is stored in DynamoDb and will be looked up to see if the user is approved to run the command, the slack user is send in the payload by slack.
5. There is also an optional approval process, this will also be configured in dynamodb, and wont run any code until the authorized approvers have approved the request.

## Infrastruture
This code will be hosted in AWS. It will use API Gateway, Lambda, DynamoDB, and IAM. 

## Interactive Components

Interactivity Request URL:
 https://cewmhpv4yk.execute-api.us-west-2.amazonaws.com/Production/component

 ## Message Menus

 Options Load Url:
 https://cewmhpv4yk.execute-api.us-west-2.amazonaws.com/Production/dynamicdatasource


## Scopes

ADMIN:

admin

CONVERSTIONS:

channels:history
channels:read
channels:write
chat:write:bot
chat:write:user
groups:history
groups:read
groups:write
im:history
incoming-webhook
mpim:history

INTERACTIVITY:

bot
commands

USERS

users:read
users:read.email

Slack Users:
rlampen: "UA4DK4XK6"

jemmett: "UA3UWKP7A"
bmccoy: "U9364SRSM"
awakimin: "U03PQKDJ6"
ywakimin: "UAB66HN00"
pkommireddi: "UA7MX3NUU"
adsouza: "UHEVC2LGM"
isarchami: "UA0NY9C74"
rmavani: "UC6MG0LSV"

jkanicky: "U9HC879MG"
awhiteley: "UBFGM0QFP"
lhassett: "U7SSE3CAU"
areiley: "U7TE154TD"
talkhatib: "U93S37X4M"

ssharpless: "U03NH6RJK"
jmiller: "U9FQFU64B"


    "function-subnets"     : "subnet-04f6a6c14f90d19cc",
    "function-security-groups" : "sg-05a8fb4959cfb5bed",