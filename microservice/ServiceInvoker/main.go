package main

import (
	"fmt"
	"os"
	"strconv"
	"time"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
)

type Request struct {
	ServiceID      string `json:"service_id"`
	ServiceVersion string `json:"service_version"`
}

type Service struct {
	Service    string `json:"service"`
	Endpoint   string `json:"endpoint"`
	Type       string `json:"type"`
	Version    string `json:"version"`
	Parameters string `json:"parameters"`
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
func Handler(request Request) (string, error) {
	serviceTable := os.Getenv("service_table")
	eventTable := os.Getenv("event_table")

	if request.ServiceID == "" {
		fmt.Printf("No service ID provided")
		return "No service ID provided", nil
	}

	service := GetServiceDetails(
		request.ServiceID,
		request.ServiceVersion,
		serviceTable)

	initalTime := GetUnixTimestamp()

	LogEvent(
		initalTime,
		initalTime,
		service.Service,
		service.Version,
		"Schedule",
		"Log",
		"2020-09-15",
		service.Service+" scheduled",
		eventTable)

	fmt.Printf("Do Logic")

	LogEvent(
		GetUnixTimestamp(),
		initalTime,
		service.Service,
		service.Version,
		"API Request Sent",
		"Log",
		"2020-09-08",
		"API GW Request Sent",
		eventTable)

	return "working", nil
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

	//jsonString, err := json.Marshal(item.Parameters)
	//println(string(jsonString))
	//return string(jsonString)
	return item
}
