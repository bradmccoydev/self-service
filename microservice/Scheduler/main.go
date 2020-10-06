package main

import (
	"bytes"
	"context"
	"fmt"
	"io/ioutil"
	"net/http"
	"net/url"
	"os"
	"strconv"
	"time"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/aws/external"
	"github.com/aws/aws-sdk-go/aws/session"
	v4 "github.com/aws/aws-sdk-go/aws/signer/v4"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
)

type Request struct {
	ServiceID      string `json:"service_id"`
	ServiceVersion string `json:"service_version"`
	Source         string `json:"source"`
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
	DateTime       string `json:"date_time"`
	Message        string `json:"message"`
}

// Handler - the actual logic
func Handler(request Request) (string, error) {
	region := os.Getenv("region")
	serviceTable := os.Getenv("application_table")
	loggerEndpoint := os.Getenv("logger_endpoint")
	serviceEndpoint := os.Getenv("service_endpoint")
	trackingID := GetUnixTimestamp()

	if request.ServiceID == "" {
		fmt.Printf("No service ID provided")
		return "No service ID provided", nil
	}

	service := GetServiceDetails(
		request.ServiceID,
		request.ServiceVersion,
		serviceTable)

	message := url.QueryEscape(service.Service + " CW scheduled")
	params := fmt.Sprintf("id=%v&serviceId=%v&serviceVersion=%v&trackingId=%v&stage=%v&eventType=%v&message=%v", trackingID, request.ServiceID, request.ServiceVersion, trackingID, request.Source, "Log", message)
	ApiPost(loggerEndpoint, params, region)

	// *******************

	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	cfg, err := external.LoadDefaultAWSConfig()
	if err != nil {
		panic("unable to load SDK config, " + err.Error())
	}

	cfg.Region = region
	ctx := context.Background()

	URL := fmt.Sprintf("%v?serviceId=%v&serviceVersion=%v&trackingId=%v", serviceEndpoint, service.Service, service.Version, trackingID)
	var requestJSON = []byte(fmt.Sprintf(`{"service_id":"%v","service_version":"%v","tracking_id":"%v"}`, service.Service, service.Version, trackingID))
	req, err := http.NewRequest("POST", URL, bytes.NewBuffer(requestJSON))
	req.Header.Set("Accept", "application/json")
	req.Header.Set("Content-Type", "application/json")
	req = req.WithContext(ctx)
	signer := v4.NewSigner(sess.Config.Credentials)
	_, err = signer.Sign(req, nil, "execute-api", cfg.Region, time.Now())
	res, err := http.DefaultClient.Do(req)

	// *******************

	if err != nil {
		message = url.QueryEscape("API Error : " + serviceEndpoint)
		params = fmt.Sprintf("id=%v&serviceId=%v&serviceVersion=%v&trackingId=%v&stage=%v&eventType=%v&message=%v", GetUnixTimestamp(), request.ServiceID, request.ServiceVersion, trackingID, "APIGW", "Error", message)
		ApiPost(loggerEndpoint, params, region)

		fmt.Printf("failed to sign request: (%v)\n", err)
		return "error", nil
	}

	defer res.Body.Close()
	body, _ := ioutil.ReadAll(res.Body)

	if res.StatusCode != 200 {
		message = url.QueryEscape("API Error : " + serviceEndpoint + string(body))
		params = fmt.Sprintf("id=%v&serviceId=%v&serviceVersion=%v&trackingId=%v&stage=%v&eventType=%v&message=%v", GetUnixTimestamp(), request.ServiceID, request.ServiceVersion, trackingID, "APIGW", "Error", message)
		ApiPost(loggerEndpoint, params, region)
		fmt.Printf("service returned a status not 200: (%d)\n", res.StatusCode)
		return "error", nil
	}

	message = url.QueryEscape("API Request Sent for service: " + request.ServiceID)
	params = fmt.Sprintf("id=%v&serviceId=%v&serviceVersion=%v&trackingId=%v&stage=%v&eventType=%v&message=%v", GetUnixTimestamp(), request.ServiceID, request.ServiceVersion, trackingID, "APIGW", "Log", message)
	ApiPost(loggerEndpoint, params, region)

	return "Service Scheduled", nil
}

// Entrypoint by AWS Lambda
func main() {
	lambda.Start(Handler)
}

func GetUnixTimestamp() string {
	time := time.Now().Unix()
	return strconv.FormatInt(time, 10)
}

func ApiPost(
	url string,
	params string,
	region string) {

	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	cfg, err := external.LoadDefaultAWSConfig()
	if err != nil {
		panic("unable to load SDK config, " + err.Error())
	}

	cfg.Region = region
	ctx := context.Background()

	URL := fmt.Sprintf("%v?%v", url, params)
	fmt.Println(fmt.Sprintf("%v?%v", url, params))
	var requestJSON = []byte(`{"service_id":"tst","service_version":"tst","tracking_id":"tst"}`)

	req, err := http.NewRequest("POST", URL, bytes.NewBuffer(requestJSON))
	req.Header.Set("Accept", "application/json")
	req.Header.Set("Content-Type", "application/json")

	req = req.WithContext(ctx)
	signer := v4.NewSigner(sess.Config.Credentials)
	_, err = signer.Sign(req, nil, "execute-api", cfg.Region, time.Now())
	res, err := http.DefaultClient.Do(req)
	if err != nil {
		panic("Error Logging, " + err.Error())
	}

	defer res.Body.Close()
	body, _ := ioutil.ReadAll(res.Body)
	if res.StatusCode != 200 {
		panic("service returned a status not 200: (%d)" + strconv.Itoa(res.StatusCode) + string(body))
	}
	fmt.Println(string(body))

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
