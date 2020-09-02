#!/bin/sh

echo "Building Artifacts"

GOOS=linux go build -o ./../build/microservice/SlackSlashCommand/main ./../microservice/SlackSlashCommand/main.go
zip -r ./../build/microservice/SlackSlashCommand/main.zip ./../build/microservice/SlackSlashCommand/main
aws --region us-west-2 s3 cp ./../build/microservice/SlackSlashCommand/main.zip s3://selfservice.bradmccoy.io/microservice/SlackSlashCommand/main.zip
aws lambda update-function-code --function-name SlackSlashCommand --s3-bucket selfservice.bradmccoy.io --s3-key microservice/SlackSlashCommand/main.zip

GOOS=linux go build -o ./../build/microservice/SlackDynamicDataSource/main ./../microservice/SlackDynamicDataSource/main.go
zip -r ./../build/microservice/SlackDynamicDataSource/main.zip ./../build/microservice/SlackDynamicDataSource/main
aws --region us-west-2 s3 cp ./../build/microservice/SlackDynamicDataSource/main.zip s3://selfservice.bradmccoy.io/microservice/SlackDynamicDataSource/main.zip
aws lambda update-function-code --function-name SlackDynamicDataSource --s3-bucket selfservice.bradmccoy.io --s3-key microservice/SlackDynamicDataSource/main.zip

GOOS=linux go build -o ./../build/microservice/ServiceInvoker/main ./../microservice/ServiceInvoker/main.go
zip -r ./../build/microservice/ServiceInvoker/main.zip ./../build/microservice/ServiceInvoker/main
aws --region us-west-2 s3 cp ./../build/microservice/ServiceInvoker/main.zip s3://selfservice.bradmccoy.io/microservice/ServiceInvoker/main.zip
#aws lambda update-function-code --function-name EndpointService --s3-bucket selfservice.bradmccoy.io --s3-key microservice/EndpointService/main.zip

GOOS=linux go build -o ./../build/microservice/Logger/main ./../microservice/Logger/main.go
zip -r ./../build/microservice/Logger/main.zip ./../build/microservice/Logger/main
aws --region us-west-2 s3 cp ./../build/microservice/Logger/main.zip s3://selfservice.bradmccoy.io/microservice/Logger/main.zip
#aws lambda update-function-code --function-name EndpointService --s3-bucket selfservice.bradmccoy.io --s3-key microservice/EndpointService/main.zip


echo "Artifacts Built"