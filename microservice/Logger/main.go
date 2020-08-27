package main

import (
	_ "github.com/lib/pq"

	"github.com/aws/aws-lambda-go/lambda"
	_ "github.com/aws/aws-sdk-go/service/lambda"
)

type Request struct {
	Body string `json:"body"`
}

func Handler(request Request) (string, error) {

	return "nil", nil
}

func main() {
	lambda.Start(Handler)
}
