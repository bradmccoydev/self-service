package main

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"log"
	"os"
	"strconv"
	"time"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/awserr"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/secretsmanager"
	"github.com/aws/aws-sdk-go/service/sns"
)

type Request struct {
	Body struct {
		Token       string `json:"token"`
		TeamID      string `json:"team_id"`
		TeamDomain  string `json:"team_domain"`
		ChannelID   string `json:"channel_id"`
		ChannelName string `json:"channel_name"`
		UserID      string `json:"user_id"`
		UserName    string `json:"user_name"`
		Command     string `json:"command"`
		Text        string `json:"text"`
		APIAppID    string `json:"api_app_id"`
		ResponseURL string `json:"response_url"`
		TriggerID   string `json:"trigger_id"`
	} `json:"body"`
	Headers struct {
		Accept                    string `json:"Accept"`
		AcceptEncoding            string `json:"Accept-Encoding"`
		CloudFrontForwardedProto  string `json:"CloudFront-Forwarded-Proto"`
		CloudFrontIsDesktopViewer string `json:"CloudFront-Is-Desktop-Viewer"`
		CloudFrontIsMobileViewer  string `json:"CloudFront-Is-Mobile-Viewer"`
		CloudFrontIsSmartTVViewer string `json:"CloudFront-Is-SmartTV-Viewer"`
		CloudFrontIsTabletViewer  string `json:"CloudFront-Is-Tablet-Viewer"`
		CloudFrontViewerCountry   string `json:"CloudFront-Viewer-Country"`
		ContentType               string `json:"Content-Type"`
		Host                      string `json:"Host"`
		UserAgent                 string `json:"User-Agent"`
		Via                       string `json:"Via"`
		XAmzCfID                  string `json:"X-Amz-Cf-Id"`
		XAmznTraceID              string `json:"X-Amzn-Trace-Id"`
		XForwardedFor             string `json:"X-Forwarded-For"`
		XForwardedPort            string `json:"X-Forwarded-Port"`
		XForwardedProto           string `json:"X-Forwarded-Proto"`
		XSlackRequestTimestamp    string `json:"X-Slack-Request-Timestamp"`
		XSlackSignature           string `json:"X-Slack-Signature"`
	} `json:"headers"`
}

type Secrets struct {
	AuthToken     string `json:"AuthToken"`
	BotUserToken  string `json:"BotUserToken"`
	SigningSecret string `json:"SigningSecret"`
}

type Message struct {
	Default string `json:"default"`
}

type Person struct {
	Name string `json:"name"`
}

func Handler(request Request) (string, error) {
	secretName := os.Getenv("secret_id")
	region := os.Getenv("region")
	snsTopicArn := os.Getenv("sns_topic_arn")

	svc := secretsmanager.New(session.New(),
		aws.NewConfig().WithRegion(region))

	input := &secretsmanager.GetSecretValueInput{
		SecretId:     aws.String(secretName),
		VersionStage: aws.String("AWSCURRENT"),
	}

	result, err := svc.GetSecretValue(input)
	if err != nil {
		if aerr, ok := err.(awserr.Error); ok {
			switch aerr.Code() {
			case secretsmanager.ErrCodeDecryptionFailure:
				// Secrets Manager can't decrypt the protected secret text using the provided KMS key.
				fmt.Println(secretsmanager.ErrCodeDecryptionFailure, aerr.Error())

			case secretsmanager.ErrCodeInternalServiceError:
				// An error occurred on the server side.
				fmt.Println(secretsmanager.ErrCodeInternalServiceError, aerr.Error())

			case secretsmanager.ErrCodeInvalidParameterException:
				// You provided an invalid value for a parameter.
				fmt.Println(secretsmanager.ErrCodeInvalidParameterException, aerr.Error())

			case secretsmanager.ErrCodeInvalidRequestException:
				// You provided a parameter value that is not valid for the current state of the resource.
				fmt.Println(secretsmanager.ErrCodeInvalidRequestException, aerr.Error())

			case secretsmanager.ErrCodeResourceNotFoundException:
				// We can't find the resource that you asked for.
				fmt.Println(secretsmanager.ErrCodeResourceNotFoundException, aerr.Error())
			}
		} else {
			// Print the error, cast err to awserr.Error to get the Code and
			// Message from an error.
			fmt.Println(err.Error())
		}
	}

	// var secretString, decodedBinarySecret string
	// if result.SecretString != nil {
	// 	secretString = *result.SecretString
	// } else {
	// 	decodedBinarySecretBytes := make([]byte, base64.StdEncoding.DecodedLen(len(result.SecretBinary)))
	// 	len, err := base64.StdEncoding.Decode(decodedBinarySecretBytes, result.SecretBinary)
	// 	if err != nil {
	// 		fmt.Println("Base64 Decode Error:", err)
	// 		return "Error with secret", nil
	// 	}
	// 	decodedBinarySecret = string(decodedBinarySecretBytes[:len])
	// }

	var secrets Secrets
	json.Unmarshal([]byte(*result.SecretString), &secrets)

	now := time.Now()
	n, err := strconv.ParseInt(request.Headers.XSlackRequestTimestamp, 10, 64)
	if err != nil {
		fmt.Printf("%d of type %T", n, n)
	}
	if (now.Unix() - n) > 60*5 {
		fmt.Println("replay attack")
		return "Replay Attack", nil
	}

	fmt.Printf(fmt.Sprintf("%v", request.Body))

	sigBasestring := "v0:" + request.Headers.XSlackRequestTimestamp + ":" + fmt.Sprintf("%v", request.Body)
	secret := secrets.SigningSecret
	h := hmac.New(sha256.New, []byte(secret))
	h.Write([]byte(sigBasestring))

	sha := hex.EncodeToString(h.Sum(nil))
	sha = "v0=" + sha

	fmt.Printf("@@")
	fmt.Printf(sha)

	if sha != request.Headers.XSlackSignature {
		fmt.Println("signature mismatch11")
	}

	signatureBaseString := fmt.Sprintf("v0:%v:%v", request.Headers.XSlackRequestTimestamp, request.Body)

	ha := hmac.New(sha256.New, []byte(secrets.SigningSecret))
	ha.Write([]byte(signatureBaseString))
	shas := hex.EncodeToString(ha.Sum(nil))

	fmt.Printf("*")
	fmt.Printf("v0=" + shas)
	fmt.Printf("**")
	fmt.Printf(request.Headers.XSlackSignature)
	fmt.Printf("***")

	if request.Headers.XSlackSignature != "v0="+sha {
		fmt.Println("Signature Did Not Match")
		//return "User not authorised", nil
	}

	fmt.Printf("****")

	currentTime := time.Now().Unix()
	str := strconv.FormatInt(currentTime, 10)
	fmt.Println(currentTime)
	fmt.Println(str)

	snsClient := sns.New(session.New(),
		aws.NewConfig().WithRegion(region))

	person := Person{
		Name: "Brad McCoy",
	}
	personStr, _ := json.Marshal(person)

	message := Message{
		Default: string(personStr),
	}
	messageBytes, _ := json.Marshal(message)
	messageStr := string(messageBytes)

	params := &sns.PublishInput{
		TopicArn:         aws.String(snsTopicArn),
		Message:          aws.String(messageStr),
		MessageStructure: aws.String("json"),
	}

	resp, err := snsClient.Publish(params)

	if err != nil {
		log.Fatal(err)
	}

	log.Print(resp)

	return "Thanks for executing the slash command", nil
}

func main() {
	lambda.Start(Handler)
}

// func getCommandFromBody(req Request) (string, error) {

// 	// Pull out the body
// 	body := req.Body

// 	// Massage body into the command structure
// 	reqData := Command{}
// 	err := json.Unmarshal([]byte(body), &reqData)
// 	if err != nil {
// 		fmt.Println("Error reported during unmarshalling of request body: ", err)
// 		return "", err
// 	}

// 	// Get the requested command & return it
// 	cmd := reqData.Command
// 	return cmd, nil
// }
