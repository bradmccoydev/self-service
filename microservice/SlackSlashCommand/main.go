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

	dynamodb "github.com/bradmccoydev/pkg/dynamodb"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
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
	signingSecretKey := os.Getenv("SlackAppCredentialsStaging")

	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	svc := secretsmanager.New(sess)

	secretsInput := &secretsmanager.GetSecretValueInput{
		SecretId:     aws.String(signingSecretKey),
		VersionStage: aws.String("AWSCURRENT"),
	}

	secretStore, err := svc.GetSecretValue(secretsInput)

	if err != nil {
		fmt.Println("Got error decrypting data: ", err)
		os.Exit(1)
	}

	var secrets Secrets
	json.Unmarshal([]byte(*secretStore.SecretString), &secrets)

	signatureBaseString := fmt.Sprintf("v0:%v:%v", request.Headers.XSlackRequestTimestamp, request.Body)

	h := hmac.New(sha256.New, []byte(secrets.SigningSecret))
	h.Write([]byte(signatureBaseString))
	sha := hex.EncodeToString(h.Sum(nil))

	if request.Headers.XSlackSignature != "v0="+sha {
		fmt.Println(request.Headers.XSlackSignature)
		fmt.Println("v0=" + sha)
	}

	currentTime := time.Now().Unix()
	str := strconv.FormatInt(currentTime, 10)
	fmt.Println(currentTime)
	fmt.Println(str)

	dynamodb.LogRequest("brad", "bradmccoydev@gmail.com", "dojo", "brad", "payload", "endpoint", "approvers", "d", "d", "d", "", "submission-production")

	return "Thanks for executing the slash command", nil
}

func main() {
	lambda.Start(Handler)
}
