package main

import (
	"fmt"

	"github.com/aws/aws-lambda-go/lambda"
)

// Handler - the actual logic
func Handler(request string) (string, error) {
	fmt.Printf(request)
	return "working", nil
}

// Entrypoint by AWS Lambda
func main() {
	lambda.Start(Handler)
}
