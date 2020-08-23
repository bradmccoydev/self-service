#!/bin/sh

echo "Building GoLang Artifacts"

for dir in lambda/*; do
    echo build/$dir;
    GOOS=linux go build -o build/$dir/main $dir/main.go
    GOOS=linux go build -o build/$dir/UnitTests/main $dir/UnitTests/main.go
    zip -r build/$dir/main.zip build/$dir/main
    aws --region us-west-2 s3 cp build/$dir/main.zip s3://selfservice.bradmccoy.io/$dir/aws/main.zip
    aws --region us-west-2 s3 cp build/$dir/UnitTests/main s3://selfservice.bradmccoy.io/$dir/UnitTests/main
done

echo "Artifacts Built"