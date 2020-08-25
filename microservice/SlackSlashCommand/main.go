package main

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"os"
	"strconv"
	"time"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/awserr"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/secretsmanager"
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
		ResponseURL string `json:"response_url"`
		TriggerID   string `json:"trigger_id"`
	} `json:"body"`
	Headers struct {
		Accept                 string `json:"Accept"`
		AcceptEncoding         string `json:"Accept-Encoding"`
		ContentType            string `json:"Content-Type"`
		Host                   string `json:"Host"`
		UserAgent              string `json:"User-Agent"`
		XAmznTraceID           string `json:"X-Amzn-Trace-Id"`
		XForwardedFor          string `json:"X-Forwarded-For"`
		XForwardedPort         string `json:"X-Forwarded-Port"`
		XForwardedProto        string `json:"X-Forwarded-Proto"`
		XSlackRequestTimestamp string `json:"X-Slack-Request-Timestamp"`
		XSlackSignature        string `json:"X-Slack-Signature"`
	} `json:"headers"`
}

type Secrets struct {
	AuthToken     string `json:"AuthToken"`
	BotUserToken  string `json:"BotUserToken"`
	SigningSecret string `json:"SigningSecret"`
}

func Handler(request Request) (string, error) {
	secretName := os.Getenv("secret_id")
	region := os.Getenv("region")

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

	fmt.Print("##")
	fmt.Printf(secrets.SigningSecret)
	fmt.Print("##")

	signatureBaseString := fmt.Sprintf("v0:%v:%v", request.Headers.XSlackRequestTimestamp, request.Body)

	h := hmac.New(sha256.New, []byte(secrets.SigningSecret))
	h.Write([]byte(signatureBaseString))
	sha := hex.EncodeToString(h.Sum(nil))

	fmt.Printf("*")
	fmt.Printf("v0=" + sha)
	fmt.Printf("**")
	fmt.Printf(request.Headers.XSlackSignature)
	fmt.Printf("***")

	if request.Headers.XSlackSignature != "v0="+sha {
		fmt.Println("Signature Did Not Match")
		return "User not authorised", nil
	}

	fmt.Printf("****")

	currentTime := time.Now().Unix()
	str := strconv.FormatInt(currentTime, 10)
	fmt.Println(currentTime)
	fmt.Println(str)

	return "Thanks for executing the slash command", nil
}

func main() {
	lambda.Start(Handler)
}
