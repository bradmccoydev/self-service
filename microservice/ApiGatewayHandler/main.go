package main

import (
	"encoding/json"
	"fmt"
	"os"
	"reflect"
	"strings"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
)

// Request - the input we receive from the API Gateway
type Request struct {
	Resource                        string `json:"resource"`
	Path                            string `json:"path"`
	HTTPMethod                      string `json:"httpMethod"`
	Headers                         string `json:"headers"`
	MultiValueHeaders               string `json:"multiValueHeaders"`
	QueryStringParameters           string `json:"queryStringParameters"`
	MultiValueQueryStringParameters string `json:"multiValueQueryStringParameters"`
	PathParameters                  string `json:"pathParameters"`
	StageVariables                  string `json:"stageVariables"`
	RequestContext                  string `json:"requestContext"`
	Body                            string `json:"body"`
	IsBase64Encoded                 string `json:"isBase64Encoded"`
}

// Arguments - key value pairs for the command arguments
type Arguments struct {
	Key   string `json:"key"`
	Value string `json:"value"`
}

// Command - the self service command definition
type Command struct {
	Command   string      `json:"command"`
	Arguments []Arguments `json:"arguments"`
}

// Response - structure defining what we pass back
type Response struct {
	Payload string `json:"Payload"`
}

// dynamoItem structure - what we retrieve from the command table in DynamoDB
type dynamoItem struct {
	Command               string `json:"command"`
	CreatedBy             string `json:"created_by"`
	CreatedDate           string `json:"created_date"`
	Description           string `json:"description"`
	Documentation         string `json:"documentation"`
	Endpoint              string `json:"endpoint"`
	LastUpdatedBy         string `json:"last_updated_by"`
	LastUpdatedDate       string `json:"last_updated_date"`
	OrchestrationEngine   string `json:"orchestration_engine"`
	PreState              string `json:"preState"`
	RequiresApproval      string `json:"requires_approval"`
	RequiresAuthorization string `json:"requires_authorization"`
	Team                  string `json:"team"`
	Title                 string `json:"title"`
	Type                  string `json:"type"`
	Version               string `json:"version"`
}

// Handler - the actual logic
func Handler(request Request) (Response, error) {

	// Get LogLevel
	logLevel := os.Getenv("LogLevel")

	// Debug
	if strings.EqualFold(logLevel, "debug") {
		fmt.Println("Received the following inputs:")

		s := reflect.ValueOf(&request).Elem()
		fieldType := s.Type()
		for i := 0; i < s.NumField(); i++ {
			f := s.Field(i)
			fmt.Printf("%s = %v\n", fieldType.Field(i).Name, f.Interface())
		}
	}

	// Extract the command from the request body
	cmd, err := getCommandFromBody(request)
	if err != nil {
		fmt.Println("Got error extracting the command from the request body: ", err)
		os.Exit(1)
	}

	// Get the command details
	result, err := getCommandDetails(cmd)
	if err != nil {
		fmt.Println("Got error retrieving command details: ", err)
		os.Exit(1)
	}

	// Debug
	if strings.EqualFold(logLevel, "debug") {
		fmt.Println("Results from command lookup: ", result)
	}

	// Build the response object
	var response Response

	// Add the payload to the response
	response.Payload = "All good so far!"

	// Now iterate through the DynamoDB response & append
	// for _, i := range results {
	// 	response.Customers = append(response.Customers, customer{
	// 		i.CustomerCode,
	// 		i.AwsID,
	// 		i.AwsRegion,
	// 		request.RoleName,
	// 	})
	// }

	// Return the response
	return response, nil
}

// Entrypoint by AWS Lambda
func main() {
	lambda.Start(Handler)
}

// Get the command details from DynamoDB
func getCommandFromBody(req Request) (string, error) {

	// Pull out the body
	body := req.Body

	// Massage body into the command structure
	reqData := Command{}
	err := json.Unmarshal([]byte(body), &reqData)
	if err != nil {
		fmt.Println("Error reported during unmarshalling of request body: ", err)
		return "", err
	}

	// Get the requested command & return it
	cmd := reqData.Command
	return cmd, nil
}

// Get the command details from DynamoDB
func getCommandDetails(cmd string) (dynamoItem, error) {

	// Get a session
	sess := session.Must(session.NewSessionWithOptions(session.Options{SharedConfigState: session.SharedConfigEnable}))

	// Create DynamoDB client
	svc := dynamodb.New(sess)

	// Perform DynamoDB table scan
	tableName := "service"
	resp, err := svc.GetItem(&dynamodb.GetItemInput{
		TableName: aws.String(tableName),
		Key: map[string]*dynamodb.AttributeValue{
			"command": {
				S: aws.String(cmd),
			},
			"team": {
				S: aws.String("T03JXKJBE"),
			},
		},
	})
	if err != nil {
		fmt.Println("DynamoDB reported an error during table scan: ", err)
		empty := dynamoItem{}
		return empty, err
	}

	// Unmarshal the results into an array
	cmdDetails := dynamoItem{}
	err = dynamodbattribute.UnmarshalMap(resp.Item, &cmdDetails)
	if err != nil {
		fmt.Println("Error reported during unmarshal of DynamoDB results: ", err)
		empty := dynamoItem{}
		return empty, err
	}

	// Return the results
	return cmdDetails, nil
}
