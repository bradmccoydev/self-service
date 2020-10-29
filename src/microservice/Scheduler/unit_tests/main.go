package main

import (
	"bytes"
	"fmt"
	"net/http"
	"os"
)

func main() {
	URL := "https://55rtcp3psrfghhppkz7wt5yzfe.appsync-api.us-west-2.amazonaws.com/graphql"
	APIKeys := os.Getenv("api_key")
	query := `{ "query": "query { getApplicationMetadataRegistry(id: \"bazaar\", version: \"1\") {id} }" }`

	//secretID := os.Getenv("secret_id")

	// sess := session.Must(session.NewSessionWithOptions(session.Options{
	// 	SharedConfigState: session.SharedConfigEnable,
	// }))

	// svc := secretsmanager.New(sess)

	// apiKeySecretInput := &secretsmanager.GetSecretValueInput{
	// 	SecretId:     aws.String(secretKey),
	// 	VersionStage: aws.String("AWSCURRENT"),
	// }

	// APIKey, err := svc.GetSecretValue(apiKeySecretInput)
	// test := *APIKey.SecretString
	// fmt.Printf(test)

	// cfg, err := external.LoadDefaultAWSConfig()
	// if err != nil {
	// 	panic("unable to load SDK config, " + err.Error())
	// }

	//cfg.Region = "us-west-2"

	var requestJSON = []byte(query)

	req, err := http.NewRequest("POST", URL, bytes.NewBuffer(requestJSON))
	req.Header.Set("Accept", "application/json")
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("x-api-key", APIKeys)

	res, err := http.DefaultClient.Do(req)

	if err != nil {
		panic("unable to load SDK config, " + err.Error())
	}

	buf := new(bytes.Buffer)
	buf.ReadFrom(res.Body)
	newStr := buf.String()

	fmt.Printf(newStr)
}
