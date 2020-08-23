.PHONY:compile

compile:
	dotnet build
	dotnet publish -c Release
	zip -r ProcessSlackSubmission/bin/Release/netcoreapp2.1/ProcessSlackSubmission.zip ProcessSlackSubmission/bin/Release/netcoreapp2.1/publish
	aws --region us-west-2 s3 cp ProcessSlackSubmission/bin/Release/netcoreapp2.1/ProcessSlackSubmission.zip s3://selfservice.bradmccoy.io-staging/microservice/ProcessSlackSubmission/aws/ProcessSlackSubmission.zip
	