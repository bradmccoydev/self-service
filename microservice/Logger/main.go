package main

import (
	"bytes"
	"fmt"
	"log"
	"net/http"
	"os"
	"strings"
	"time"

	_ "github.com/lib/pq"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	_ "github.com/aws/aws-sdk-go/service/lambda"
	"github.com/aws/aws-sdk-go/service/s3"
	"github.com/aws/aws-sdk-go/service/s3/s3manager"
)

type Request struct {
	Body string `json:"body"`
}

func Handler(request Request) (string, error) {
	region := os.Getenv("region")
	bucket := os.Getenv("bucket")

	const (
		layoutISO = "2006-01-02T15:04:05+1100"
		layoutUS  = "200601"
	)

	date := time.Now().Format(layoutISO)
	folderName := fmt.Sprintf("/year=/month=/day=")

	fmt.Printf(folderName)

	//var folderName = $"/year={date.Year}/month={date.ToString("MM")}/day={date.ToString("dd")}";

	filename := string(date)
	fmt.Printf(string(date))

	csv := `{"SuperHero Name", "Power", "Weakness"}`

	reader := strings.NewReader(csv)

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
