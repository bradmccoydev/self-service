package main

import (
	"fmt"
	"log"
	"strings"
	"time"

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/s3/s3manager"
)

func main() {
	const (
		layoutISO = "2006-01-02T15:04:05+1100"
		layoutUS  = "200601"
	)

	region := "us-west-2"
	bucket := "bradmccoy.io"
	filename := "logs/test.csv"

	date := time.Now().Format(layoutISO)

	fmt.Printf(string(date))

	csv := `123, command, user`

	reader := strings.NewReader(csv)

	sess, err := session.NewSession(&aws.Config{
		Region: aws.String(region)})
	if err != nil {
		log.Fatal(err)
	}

	uploader := s3manager.NewUploader(sess)

	_, err = uploader.Upload(&s3manager.UploadInput{
		Bucket: aws.String(bucket),
		Key:    aws.String(filename),
		Body:   reader,
	})

	if err != nil {
		fmt.Printf("Unable to upload %q to %q, %v", filename, bucket, err)
	}

	fmt.Printf("Successfully uploaded %q to %q\n", filename, bucket)

}
