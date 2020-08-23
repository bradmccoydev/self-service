package main

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"log"
	"net/url"
	"os"
	"strings"

	dynamodb "github.com/bradmccoydev/pkg/dynamodb"
	lambdaInvoker "github.com/bradmccoydev/pkg/lambdainvoker"
	stringHelper "github.com/bradmccoydev/pkg/stringhelper"
	_ "github.com/lib/pq"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/secretsmanager"
)

type Request struct {
	Body    string `json:"body"`
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

type DialogSuggestion struct {
	Type     string `json:"type"`
	Token    string `json:"token"`
	ActionTs string `json:"action_ts"`
	Team     struct {
		ID     string `json:"id"`
		Domain string `json:"domain"`
	} `json:"team"`
	User struct {
		ID   string `json:"id"`
		Name string `json:"name"`
	} `json:"user"`
	Channel struct {
		ID   string `json:"id"`
		Name string `json:"name"`
	} `json:"channel"`
	Name       string `json:"name"`
	Value      string `json:"value"`
	CallbackID string `json:"callback_id"`
	State      string `json:"state"`
}

type ExternalDataSource struct {
	Values []struct {
		Source    string `json:"source"`
		Name      string `json:"name"`
		Table     string `json:"table"`
		Key       string `json:"key"`
		Attribute string `json:"attribute"`
		Value     string `json:"value"`
	} `json:"values"`
}

func Handler(request Request) (string, error) {
	signingSecretKey := os.Getenv("ShellySigningSecret")

	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	svc := secretsmanager.New(sess)

	signingSecretInput := &secretsmanager.GetSecretValueInput{
		SecretId:     aws.String(signingSecretKey),
		VersionStage: aws.String("AWSCURRENT"),
	}

	signingSecret, err := svc.GetSecretValue(signingSecretInput)

	if err != nil {
		fmt.Println("Got error decrypting data: ", err)
		os.Exit(1)
	}

	output := "{'options':[{'label':'Error - Contact TCS','value':'error'}]}"

	signatureBaseString := fmt.Sprintf("v0:%v:%v", request.Headers.XSlackRequestTimestamp, request.Body)

	h := hmac.New(sha256.New, []byte(*signingSecret.SecretString))
	h.Write([]byte(signatureBaseString))
	sha := hex.EncodeToString(h.Sum(nil))

	fmt.Println(request.Headers.XSlackSignature)
	fmt.Println("v0=" + sha)

	if request.Headers.XSlackSignature != "v0="+sha {
		newError := strings.Replace(output, "Error", "UnAuth", -1)
		fmt.Println(newError)
	}

	decodedValue, err := url.QueryUnescape(request.Body)
	if err != nil {
		log.Fatal(err)
	}

	fmt.Println("Decoded: " + decodedValue)
	fmt.Println("Header: " + request.Headers.XSlackRequestTimestamp)

	suggestion := strings.Replace(decodedValue, "payload=", "", -1)
	fmt.Println("Suggestion: " + suggestion)

	var dialogSuggestion DialogSuggestion
	json.Unmarshal([]byte(suggestion), &dialogSuggestion)

	fmt.Println("callback: ", dialogSuggestion.CallbackID)
	fmt.Println("callback: ", dialogSuggestion.Team.ID)

	externalDataSourceJSON := dynamodb.GetCommandDetails(
		"command-production",
		dialogSuggestion.CallbackID,
		dialogSuggestion.Team.ID,
		"external_data_source")

	fmt.Println("externalDataSourceJSON: ", externalDataSourceJSON)
	fmt.Println("callback", dialogSuggestion.Team.ID)

	var externalDataSource ExternalDataSource
	json.Unmarshal([]byte(externalDataSourceJSON), &externalDataSource)

	tableName := ""
	key := ""
	value := ""
	attribute := ""
	source := ""

	for _, dataSource := range externalDataSource.Values {
		if dialogSuggestion.Name == dataSource.Name {
			fmt.Println("Source", dataSource.Source)
			fmt.Println("Value", dataSource.Value)
			fmt.Println("Table", dataSource.Table)
			fmt.Println("Key", dataSource.Key)
			fmt.Println("Attribute", dataSource.Attribute)

			source = dataSource.Source

			if source == "DynamoDb" {
				value = stringHelper.GetStateValue(dialogSuggestion.State, dataSource.Value)
			}

			tableName = dataSource.Table
			key = dataSource.Key
			attribute = dataSource.Attribute
		}
	}

	if source == "Lambda" {
		output = lambdaInvoker.InvokeLambda(tableName, "brad", "mccoy")
		output = strings.TrimSuffix(output, "\"")
		output = strings.TrimPrefix(output, "\"")
	}

	if source == "DynamoDb" {
		output = dynamodb.GetStringListQuery(
			tableName,
			key,
			value,
			attribute)
	}

	fmt.Println("output: " + strings.Replace(output, "\"", "", -1))

	return output, nil
}

func main() {
	lambda.Start(Handler)
}
