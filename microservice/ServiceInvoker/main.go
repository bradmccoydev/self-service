package main

import (
	"encoding/json"
	"fmt"
	"os"
	"strconv"
	"time"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
	"github.com/aws/aws-sdk-go/service/sfn"
)

type Service struct {
	Service          string `json:"service"`
	Title            string `json:"title"`
	Endpoint         string `json:"endpoint"`
	EndpointType     string `json:"endpoint_type"`
	Type             string `json:"type"`
	Version          string `json:"version"`
	Parameters       string `json:"parameters"`
	SchemaDefinition string `json:"schema_definition"`
}

type Event struct {
	ID             string `json:"id"`
	TrackingID     string `json:"tracking_id"`
	Service        string `json:"service"`
	ServiceVersion string `json:"service_version"`
	Stage          string `json:"stage"`
	EventType      string `json:"event_type"`
	DateTime       string `json:"datetime"`
	Message        string `json:"message"`
}

// Handler - the actual logic
func Handler(request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	serviceTable := os.Getenv("service_table")
	eventTable := os.Getenv("event_table")

	fmt.Printf(serviceTable)
	fmt.Printf(eventTable)

	serviceID := request.QueryStringParameters["serviceId"]
	serviceVersion := request.QueryStringParameters["serviceVersion"]
	trackingID := request.QueryStringParameters["trackingId"]

	service := GetServiceDetails(
		serviceID,
		serviceVersion,
		serviceTable)

	fmt.Println("serviceId: " + service.Service)
	fmt.Println("version: " + service.Version)
	fmt.Println("trackingId: " + trackingID)

	if service.EndpointType == "step_function" {
		sess := session.Must(session.NewSessionWithOptions(session.Options{
			SharedConfigState: session.SharedConfigEnable,
		}))

		svc := sfn.New(sess)

		params := &sfn.StartExecutionInput{
			Input:           aws.String(service.Parameters),
			Name:            aws.String(trackingID),
			StateMachineArn: aws.String(service.Endpoint),
		}

		req, sfnResp := svc.StartExecutionRequest(params)

		err := req.Send()
		if err == nil {
			fmt.Println(sfnResp)
		}
	}

	resp := &Event{
		Service: "Service Invoked",
	}

	responseBody, err := json.Marshal(resp)
	if err != nil {
		return events.APIGatewayProxyResponse{}, err
	}
	return events.APIGatewayProxyResponse{Body: string(responseBody), StatusCode: 200}, nil

}

// Entrypoint by AWS Lambda
func main() {
	lambda.Start(Handler)
}

func GetUnixTimestamp() string {
	time := time.Now().Unix()
	return strconv.FormatInt(time, 10)
}

// LogEvent function that logs events
func LogEvent(
	id string,
	trackingID string,
	service string,
	serviceVersion string,
	stage string,
	eventType string,
	dateTime string,
	message string,
	tableName string) {

	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	svc := dynamodb.New(sess)

	item := Event{
		ID:             id,
		TrackingID:     trackingID,
		Service:        service,
		ServiceVersion: serviceVersion,
		Stage:          stage,
		EventType:      eventType,
		DateTime:       dateTime,
		Message:        message,
	}

	av, err := dynamodbattribute.MarshalMap(item)
	fmt.Println(av)
	if err != nil {
		fmt.Println("Got error marshalling Log:")
		fmt.Println(err.Error())
		fmt.Println(av)
		os.Exit(1)
	}

	input := &dynamodb.PutItemInput{
		Item:      av,
		TableName: aws.String(tableName),
	}

	_, err = svc.PutItem(input)
	if err != nil {
		fmt.Println("Got error calling PutItem:")
		fmt.Println(err.Error())
		os.Exit(1)
	}
}

func GetServiceDetails(service string, version string, tableName string) Service {
	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	svc := dynamodb.New(sess)

	result, err := svc.GetItem(&dynamodb.GetItemInput{
		TableName: aws.String(tableName),
		Key: map[string]*dynamodb.AttributeValue{
			"service": {
				S: aws.String(service),
			},
			"version": {
				S: aws.String(version),
			},
		},
	})
	if err != nil {
		fmt.Println(err.Error())
	}

	item := Service{}

	err = dynamodbattribute.UnmarshalMap(result.Item, &item)
	if err != nil {
		panic(fmt.Sprintf("Failed to unmarshal Record, %v", err))
	}

	return item
}
