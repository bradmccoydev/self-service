#!/bin/sh

echo "Building GoLang Artifacts"

echo "Building Test Artifacts"
    GOOS=linux go build -o build/microservice/SlackSlashCommand/main microservice/SlackSlashCommand/main.go
    zip -r build/microservice/SlackSlashCommand/main.zip build/microservice/SlackSlashCommand/main
    aws --region us-west-2 s3 cp build/microservice/SlackSlashCommand/main.zip s3://selfservice.bradmccoy.io/microservice/SlackSlashCommand/main.zip

    GOOS=linux go build -o build/microservice/SlackDynamicDataSource/main microservice/SlackDynamicDataSource/main.go
    zip -r build/microservice/SlackDynamicDataSource/main.zip build/microservice/SlackDynamicDataSource/main
    aws --region us-west-2 s3 cp build/microservice/SlackDynamicDataSource/main.zip s3://selfservice.bradmccoy.io/microservice/SlackDynamicDataSource/main.zip


echo "Artifacts Built"