package main

import (
	"bytes"
	"encoding/csv"
	"log"
	"net/http"
	"os"

	_ "github.com/lib/pq"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	_ "github.com/aws/aws-sdk-go/service/lambda"
	"github.com/aws/aws-sdk-go/service/s3"
)

type Request struct {
	Body string `json:"body"`
}

func Handler(request Request) (string, error) {
	var data = [][]string{{"Line1", "Hello Readers of"}, {"Line2", "golangcode.com"}}

	file, err := os.Create("result.csv")
	checkError("Cannot create file", err)
	defer file.Close()

	writer := csv.NewWriter(file)
	defer writer.Flush()

	for _, value := range data {
		err := writer.Write(value)
		checkError("Cannot write to file", err)
	}

	s, err := session.NewSession(&aws.Config{Region: aws.String("S3_REGION")})
	if err != nil {
		log.Fatal(err)
	}

	// Upload
	err = AddFileToS3(s, "result.csv")
	if err != nil {
		log.Fatal(err)
	}

	return "nil", nil
}

func main() {
	lambda.Start(Handler)
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
