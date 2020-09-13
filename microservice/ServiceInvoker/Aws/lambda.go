package lambdainvoker

import (
	"encoding/json"
	"fmt"

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/lambda"
)

func InvokeLambda(
	functionName string,
	key string,
	value string) string {
	sess, err := session.NewSession(&aws.Config{
		Region: aws.String("us-west-2")},
	)

	svc := lambda.New(sess)

	body, err := json.Marshal(map[string]interface{}{
		key: value,
	})

	type Payload struct {
		Body string `json:"body"`
	}
	p := Payload{
		Body: string(body),
	}
	payload, err := json.Marshal(p)

	if err != nil {
		fmt.Println("Json Marshalling error")
	}

	result, err := svc.Invoke(&lambda.InvokeInput{
		FunctionName:   aws.String("DataSourceJql-Staging"),
		InvocationType: aws.String("RequestResponse"),
		LogType:        aws.String("Tail"),
		Payload:        payload,
	})

	if err != nil {
		fmt.Println("error")
		fmt.Println(err.Error())
	}

	return string(result.Payload)
}
