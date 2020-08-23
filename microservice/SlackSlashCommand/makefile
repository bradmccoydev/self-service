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
		docker run --rm -v "/Users/bmccoy/Development/bradmccoydev/self-service/SlackDynamicDataSource":/var/task lambci/lambda:go1.x main '{"JiraAdminUser": "bmccoy"}'
ship:   compile
		cd ~/Development/bradmccoydev/self-service/build/SlackDynamicDataSource; zip -r main.zip main
		cd ~/Development/bradmccoydev/self-service/functions/SlackDynamicDataSource/terraform; terraform init;terraform apply