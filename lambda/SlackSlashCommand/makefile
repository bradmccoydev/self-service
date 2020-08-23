.PHONY:compile
.PHONY:test
.PHONY:ship
.PHONY:install
compile:
		go get "github.com/bradmccoydev/pkg/stringhelper"
		go get "github.com/bradmccoydev/pkg/dynamodb"
		go get "github.com/bradmccoydev/pkg/slack"
		GOOS=linux go build -o main main.go
test:   compile
		docker run --rm -v "/Users/bmccoy/Development/ACS-GitLab/self-service/SlackDynamicDataSource":/var/task lambci/lambda:go1.x main '{"JiraAdminUser": "bmccoy"}'
ship:   compile
		cd ~/Development/ACS-GitLab/self-service/build/SlackDynamicDataSource; zip -r main.zip main
		cd ~/Development/ACS-GitLab/self-service/functions/SlackDynamicDataSource/terraform; terraform init;terraform apply
install: go get "github.com/aws/aws-lambda-go/lambda"
		go get "github.com/aws/aws-sdk-go/aws"
		go get "github.com/aws/aws-sdk-go/aws/credentials"
		go get "github.com/aws/aws-sdk-go/aws/session"
		go get "github.com/aws/aws-sdk-go/service/s3"
		go get "github.com/aws/aws-sdk-go/service/s3/s3manager"
		go get "github.com/lib/pq"
		go get -u all
		