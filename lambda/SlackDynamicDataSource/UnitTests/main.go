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

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/lambda"
)

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

func main() {
	signingSecret := os.Args[1]

	body := "payload=%7B%22type%22%3A%22dialog_suggestion%22%2C%22token%22%3A%22H1SvLWnFUASZPKW04vdfGpxl%22%2C%22action_ts%22%3A%221585373739.940102%22%2C%22team%22%3A%7B%22id%22%3A%22T03JXKJBE%22%2C%22domain%22%3A%22temenos%22%7D%2C%22user%22%3A%7B%22id%22%3A%22U9364SRSM%22%2C%22name%22%3A%22bmccoy%22%7D%2C%22channel%22%3A%7B%22id%22%3A%22GGCG5TTPC%22%2C%22name%22%3A%22privategroup%22%7D%2C%22name%22%3A%22environment%22%2C%22value%22%3A%22%22%2C%22callback_id%22%3A%22dbquery%22%2C%22state%22%3A%22%27ClientCode%27%3A%27tmregression%27%2C%27dialog%27%3A%272%27%2C%27number_of_dialogs%27%3A%272%27%2C%27ShowHeaders%27%3A%27true%27%2C%27self_service_environment%27%3A%27production%27%2C%27callback_id%27%3A%27dbquery%27%22%7D"
	requestTimeStamp := "1585373739"
	signatureBaseString := fmt.Sprintf("v0:%v:%v", requestTimeStamp, body)
	slackSignature := ""
	externalDataSourceJSON := `{ "values": [{ "source": "DynamoDb", "name": "environment","table": "customer", "key": "customer_code", "attribute": "environments","value":"ClientCode" }] }`

	dt := InvokeLambda("DataSourceJql-Staging", "Brad", "McCoy")
	fmt.Println(dt)

	h := hmac.New(sha256.New, []byte(signingSecret))
	h.Write([]byte(signatureBaseString))
	sha := hex.EncodeToString(h.Sum(nil))

	fmt.Println(slackSignature)
	fmt.Println("v0=" + sha)

	decodedValue, err := url.QueryUnescape(body)
	if err != nil {
		log.Fatal(err)
		return
	}

	suggestion := strings.Replace(decodedValue, "payload=", "", -1)

	var dialogSuggestion DialogSuggestion
	json.Unmarshal([]byte(suggestion), &dialogSuggestion)

	externalDataSourceJSON = GetCommandDetails(
		"command-production",
		dialogSuggestion.CallbackID,
		dialogSuggestion.Team.ID,
		"external_data_source")

	var externalDataSource ExternalDataSource
	json.Unmarshal([]byte(externalDataSourceJSON), &externalDataSource)

	tableName := ""
	key := ""
	value := ""
	attribute := ""
	source := ""

	for _, dataSource := range externalDataSource.Values {
		if dialogSuggestion.Name == dataSource.Name {
			source = dataSource.Source

			value = GetStateValue(dialogSuggestion.State, dataSource.Value)
			//value = "ALPINE"

			tableName = dataSource.Table
			key = dataSource.Key
			attribute = dataSource.Attribute
		}
	}

	if source == "Lambda" {
		fmt.Println("lambda")
		InvokeLambda("SlackDynamicDataSource", "Brad", "McCoy")
	}

	list := ""
	if source == "DynamoDb" {
		list = GetStringListQuery(
			tableName,
			key,
			value,
			attribute)
	}
	fmt.Printf(list)
}

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

func GetCommandDetails(
	tableName string,
	partitionKey string,
	sortKey string,
	attribute string) string {

	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	svc := dynamodb.New(sess)

	params := &dynamodb.GetItemInput{
		TableName: aws.String(tableName),
		AttributesToGet: []*string{
			aws.String(attribute),
		},
		Key: map[string]*dynamodb.AttributeValue{
			"command": {
				S: aws.String(partitionKey),
			},
			"team": {
				S: aws.String(sortKey),
			},
		},
	}

	resp, err := svc.GetItem(params)

	if err != nil {
		fmt.Println("Got error decrypting data: ", err)
		os.Exit(1)
	}

	output := ""

	for _, line := range resp.Item {
		output = *line.S
	}

	return output
}

func GetStateValue(state string, name string) string {
	state = strings.Replace(state, "{", "", -1)
	state = strings.Replace(state, "}", "", -1)

	attribute := ""
	value := ""
	lines := strings.Split(state, "',")

	for _, line := range lines {
		x := before(line, "':")
		y := after(line, ":'")
		attribute = strings.Replace(x, "'", "", -1)
		value = strings.Replace(y, "'", "", -1)

		if attribute == name {
			fmt.Printf("match:" + value)
			return value
		}
	}
	return "Error"
}

func between(value string, a string, b string) string {
	posFirst := strings.Index(value, a)
	if posFirst == -1 {
		return ""
	}
	posLast := strings.Index(value, b)
	if posLast == -1 {
		return ""
	}
	posFirstAdjusted := posFirst + len(a)
	if posFirstAdjusted >= posLast {
		return ""
	}
	return value[posFirstAdjusted:posLast]
}

func before(value string, a string) string {
	pos := strings.Index(value, a)
	if pos == -1 {
		return ""
	}
	return value[0:pos]
}

func after(value string, a string) string {
	pos := strings.LastIndex(value, a)
	if pos == -1 {
		return ""
	}
	adjustedPos := pos + len(a)
	if adjustedPos >= len(value) {
		return ""
	}
	return value[adjustedPos:len(value)]
}

func GetStringListQuery(
	tableName string,
	key string,
	value string,
	attribute string) string {

	sess := session.Must(session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	}))

	svc := dynamodb.New(sess)

	params := &dynamodb.GetItemInput{
		TableName: aws.String(tableName),
		AttributesToGet: []*string{
			aws.String(attribute),
		},
		Key: map[string]*dynamodb.AttributeValue{
			key: {
				S: aws.String(value),
			},
		},
	}

	resp, err := svc.GetItem(params)

	if err != nil {
		fmt.Println("Got error decrypting data: ", err)
		os.Exit(1)
	}

	output := "{'options':["

	for _, line := range resp.Item {
		for _, x := range line.L {
			attribute := strings.Replace(*x.S, "url=", "", -1)
			output = output + "{'label':'" + attribute + "','value':'" + attribute + "'},"
		}
	}

	output = strings.TrimSuffix(output, ",")
	output = output + "]}"

	return output
}
