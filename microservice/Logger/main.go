package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"net/url"
	"os"
	"strconv"
	"time"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
	_ "github.com/aws/aws-sdk-go/service/lambda"
	"github.com/aws/aws-sdk-go/service/s3"
)

type Response struct {
	ID string `json:"log_id"`
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
func Handler(request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	eventTable := os.Getenv("event_table")

	const (
		layoutISO = "2006-01-02T15:04:05+1100"
		layoutUS  = "200601"
	)

	date := time.Now().Format(layoutISO)

	ID := request.QueryStringParameters["id"]
	serviceID := request.QueryStringParameters["serviceId"]
	serviceVersion := request.QueryStringParameters["serviceVersion"]
	trackingID := request.QueryStringParameters["trackingId"]
	stage := request.QueryStringParameters["stage"]
	eventType := request.QueryStringParameters["eventType"]
	message := request.QueryStringParameters["message"]

	fmt.Println("ID: " + ID)
	fmt.Println("serviceID: " + serviceID)
	fmt.Println("serviceVersion: " + serviceVersion)
	fmt.Println("trackingID: " + trackingID)
	fmt.Println("stage: " + stage)
	fmt.Println("eventType: " + eventType)
	fmt.Println("message: " + message)

	unEscapedMessage, err := url.QueryUnescape(message)

	LogEvent(
		ID,
		trackingID,
		serviceID,
		serviceVersion,
		stage,
		eventType,
		date,
		unEscapedMessage,
		eventTable)

	resp := &Response{
		ID: ID,
	}

	responseBody, err := json.Marshal(resp)
	if err != nil {
		return events.APIGatewayProxyResponse{}, err
	}
	return events.APIGatewayProxyResponse{Body: string(responseBody), StatusCode: 200}, nil

}

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

	fmt.Println("Successfully added '" + item.ID)
}

func checkError(message string, err error) {
	if err != nil {
		log.Fatal(message, err)
	}
}

func AddFileToS3(s *session.Session, fileDir string) error {

	// Open the file for use
	file, err := os.Open(fileDir)
	if err != nil {
		return err
	}
	defer file.Close()

	// Get file size and read the file content into a buffer
	fileInfo, _ := file.Stat()
	var size int64 = fileInfo.Size()
	buffer := make([]byte, size)
	file.Read(buffer)

	// Config settings: this is where you choose the bucket, filename, content-type etc.
	// of the file you're uploading.
	_, err = s3.New(s).PutObject(&s3.PutObjectInput{
		Bucket:               aws.String("S3_BUCKET"),
		Key:                  aws.String(fileDir),
		ACL:                  aws.String("private"),
		Body:                 bytes.NewReader(buffer),
		ContentLength:        aws.Int64(size),
		ContentType:          aws.String(http.DetectContentType(buffer)),
		ContentDisposition:   aws.String("attachment"),
		ServerSideEncryption: aws.String("AES256"),
	})
	return err
}

// folderName := fmt.Sprintf("/year=/month=/day=")

// fmt.Printf(folderName)

// //var folderName = $"/year={date.Year}/month={date.ToString("MM")}/day={date.ToString("dd")}";

// filename := string(date)
// fmt.Printf(string(date))

// csv := `{"SuperHero Name", "Power", "Weakness"}`

// reader := strings.NewReader(csv)

//https://stackoverflow.com/questions/47621804/upload-object-to-aws-s3-without-creating-a-file-using-aws-sdk-go

// dt.Columns.Add("id", typeof(String));
// dt.Columns.Add("command", typeof(String));
// dt.Columns.Add("user", typeof(String));
// dt.Columns.Add("team", typeof(String));
// dt.Columns.Add("channel", typeof(String));
// dt.Columns.Add("payload", typeof(String));
// dt.Columns.Add("status", typeof(String));
// dt.Columns.Add("date_time_unix", typeof(String));
// dt.Columns.Add("date_time_loaded_utc", typeof(String));
// dt.Columns.Add("tracking_id", typeof(String));

// sess, err := session.NewSession(&aws.Config{
// 	Region: aws.String(region)})
// if err != nil {
// 	log.Fatal(err)
// }

// uploader := s3manager.NewUploader(sess)

// _, err = uploader.Upload(&s3manager.UploadInput{
// 	Bucket: aws.String(bucket),
// 	Key:    aws.String(filename),
// 	Body:   reader,
// })

// if err != nil {
// 	fmt.Printf("Unable to upload %q to %q, %v", filename, bucket, err)
// }

// fmt.Printf("Successfully uploaded %q to %q\n", filename, bucket)
