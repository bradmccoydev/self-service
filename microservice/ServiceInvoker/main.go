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
)

type Request struct {
	ServiceID      string `json:"service_id"`
	ServiceVersion string `json:"service_version"`
	TrackingID     string `json:"tracking_id"`
	Body           string `json:"body"`
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
func Handler(request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	serviceTable := os.Getenv("service_table")
	eventTable := os.Getenv("event_table")

	fmt.Printf(serviceTable)
	fmt.Printf(eventTable)

	email := request.QueryStringParameters["test"]
	fmt.Printf(email)
	fmt.Printf("hello\n")

	fmt.Printf(request.Body)

	// rawParam1, found := request.QueryParameters["param1"]
	// if found {
	// 	// query parameters are typically URL encoded so to get the value
	// 	value, err := url.QueryUnescape(rawParam1)
	// 	if nil != err {
	// 		return handleError(err)
	// 	}
	// 	// ... now use the value as needed
	// }

	// body, err := base64.StdEncoding.DecodeString(request.Body)
	// fmt.Printf(string(body))

	//fmt.Printf(request.Body)

	resp := &Request{
		ServiceID: "test",
	}

	responseBody, err := json.Marshal(resp)
	if err != nil {
		return events.APIGatewayProxyResponse{}, err
	}
	return events.APIGatewayProxyResponse{Body: string(responseBody), StatusCode: 200}, nil

	// fmt.Println("Received body: ", request)
	// //fmt.Println("Received body: ", request.Body)

	// fmt.Printf("Processing request data for request %s.\n", request.RequestContext.RequestID)
	// fmt.Printf("Body size = %d.\n", len(request.Body))

	// fmt.Println("Headers:")
	// for key, value := range request.Headers {
	// 	fmt.Printf("    %s: %s\n", key, value)
	// }

	// code := 200
	// response, error := json.Marshal(Request{Response: "Hello, " + name.Body})
	// if error != nil {
	// 	log.Println(error)
	// 	response = []byte("Internal Server Error")
	// 	code = 500
	// }

	// fmt.Printf("****")
	// fmt.Printf(request.ServiceID)
	// fmt.Printf(request.ServiceVersion)
	// fmt.Printf(request.TrackingID)
	// fmt.Printf("###")

	// if request.ServiceID == "" {
	// 	fmt.Printf("No service ID provided")
	// 	return "No service ID provided", nil
	// }

	// service := GetServiceDetails(
	// 	request.ServiceID,
	// 	request.ServiceVersion,
	// 	serviceTable)

	// LogEvent(
	// 	GetUnixTimestamp(),
	// 	request.TrackingID,
	// 	service.Service,
	// 	service.Version,
	// 	"Schedule",
	// 	"Log",
	// 	"2020-09-15",
	// 	service.Service+" invoked",
	// 	eventTable)

	// fmt.Printf("Do Logic")

	// LogEvent(
	// 	GetUnixTimestamp(),
	// 	request.TrackingID,
	// 	service.Service,
	// 	service.Version,
	// 	"Service Executed",
	// 	"Log",
	// 	"2020-09-08",
	// 	"Service Request Sent",
	// 	eventTable)

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
