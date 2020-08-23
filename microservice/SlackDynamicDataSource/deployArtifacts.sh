#!/bin/sh

echo "Building Test Artifacts"
    GOOS=linux go build -o main main.go
    zip -r main.zip main
    aws --region us-west-2 s3 cp main.zip s3://bradmccoy.io/microservice/golang/SlackDynamicDataSource/aws/main.zip

echo "Artifacts Built"