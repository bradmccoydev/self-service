package main

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

// // Request - the input we receive from the API Gateway
// type Request struct {
// 	Resource                        string `json:"resource"`
// 	Path                            string `json:"path"`
// 	HTTPMethod                      string `json:"httpMethod"`
// 	Headers                         string `json:"headers"`
// 	MultiValueHeaders               string `json:"multiValueHeaders"`
// 	QueryStringParameters           string `json:"queryStringParameters"`
// 	MultiValueQueryStringParameters string `json:"multiValueQueryStringParameters"`
// 	PathParameters                  string `json:"pathParameters"`
// 	StageVariables                  string `json:"stageVariables"`
// 	RequestContext                  string `json:"requestContext"`
// 	Body                            string `json:"body"`
// 	IsBase64Encoded                 string `json:"isBase64Encoded"`
// }

// // Arguments - key value pairs for the command arguments
// type arguments struct {
// 	Key   string `json:"key"`
// 	Value string `json:"value"`
// }

// // Command - the self service command definition
// type command struct {
// 	Command   string      `json:"command"`
// 	Arguments []arguments `json:"arguments"`
// }

// // Response - structure defining what we pass back
// type Response struct {
// 	Payload string `json:"Payload"`
// }

//-----------------------------------------------------
//
// Test routines
//
//-----------------------------------------------------

// Test Lambda Handler
func TestGetBrokers(t *testing.T) {

	// Setup test data
	assert := assert.New(t)
	var tests = []struct {
		testName    string
		commandName string
		expectError string
	}{
		{"Valid command name.", "dbquery", "false"},
		{"Invalid command name.", "fred", "true"},
	}

	// Iterate through the test data
	for _, test := range tests {

		// Build the command
		var cmd = Command{}
		cmd.Command = test.commandName
		bytes, _ := json.Marshal(cmd)
		body := string(bytes)

		// Build the request
		var request = Request{}
		request.Body = body

		// Run each test
		_, err := Handler(request)
		if test.expectError == "false" {
			assert.Nil(err)
		} else {
			assert.NotNil(err)
		}
	}
}
