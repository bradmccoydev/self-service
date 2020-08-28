# AWS Lambda Process Slack Slash Command



## Interactive Components

Interactivity Request URL:
 https://cewmhpv4yk.execute-api.us-west-2.amazonaws.com/Production/component

 ## Message Menus

 Options Load Url:
 https://cewmhpv4yk.execute-api.us-west-2.amazonaws.com/Production/dynamicdatasource


## Scopes

ADMIN:

admin

CONVERSTIONS:

channels:history
channels:read
channels:write
chat:write:bot
chat:write:user
groups:history
groups:read
groups:write
im:history
incoming-webhook
mpim:history

INTERACTIVITY:

bot
commands

USERS

users:read
users:read.email

Api Gateway config:

{
  "swagger": "2.0",
  "info": {
    "version": "2019-04-03T22:48:02Z",
    "title": "ShellyApi"
  },
  "host": "cewmhpv4yk.execute-api.us-west-2.amazonaws.com",
  "basePath": "/Production",
  "schemes": [
    "https"
  ],
  "paths": {
    "/component": {
      "post": {
        "consumes": [
          "application/x-www-form-urlencoded"
        ],
        "produces": [
          "application/json"
        ],
        "responses": {
          "200": {
            "description": "200 response",
            "schema": {
              "$ref": "#/definitions/Empty"
            }
          }
        },
        "x-amazon-apigateway-integration": {
          "uri": "arn:aws:apigateway:us-west-2:lambda:path/2015-03-31/functions/arn:aws:lambda:us-west-2:457972698173:function:ProcessSlackSubmission/invocations",
          "responses": {
            "default": {
              "statusCode": "200"
            }
          },
          "requestParameters": {
            "integration.request.header.X-Amz-Invocation-Type": "'Event'"
          },
          "passthroughBehavior": "never",
          "httpMethod": "POST",
          "requestTemplates": {
            "application/x-www-form-urlencoded": "## The `substring(8)` returns the input with everything after \"payload=\" which is the only parameter passed in,\r\n## everything else is encoded JSON inside that parameter.\r\n#set ($encodedJSON = $input.body.substring(8))\r\n$util.urlDecode(${encodedJSON})"
          },
          "contentHandling": "CONVERT_TO_TEXT",
          "type": "aws"
        }
      }
    },
    "/dynamicdatasource": {
      "post": {
        "consumes": [
          "application/x-www-form-urlencoded"
        ],
        "produces": [
          "application/json"
        ],
        "responses": {
          "200": {
            "description": "200 response",
            "schema": {
              "$ref": "#/definitions/Empty"
            }
          }
        },
        "x-amazon-apigateway-integration": {
          "uri": "arn:aws:apigateway:us-west-2:lambda:path/2015-03-31/functions/arn:aws:lambda:us-west-2:457972698173:function:SlackDynamicDataSource/invocations",
          "responses": {
            "default": {
              "statusCode": "200",
              "responseTemplates": {
                "application/json": "$input.path('$')"
              }
            }
          },
          "passthroughBehavior": "never",
          "httpMethod": "POST",
          "requestTemplates": {
            "application/x-www-form-urlencoded": "{ \"body\": \"$input.path('$')\" }"
          },
          "contentHandling": "CONVERT_TO_TEXT",
          "type": "aws"
        }
      }
    },
    "/slashcommands": {
      "post": {
        "consumes": [
          "application/x-www-form-urlencoded"
        ],
        "produces": [
          "application/json"
        ],
        "responses": {
          "200": {
            "description": "200 response",
            "schema": {
              "$ref": "#/definitions/Empty"
            }
          }
        },
        "x-amazon-apigateway-integration": {
          "uri": "arn:aws:apigateway:us-west-2:lambda:path/2015-03-31/functions/arn:aws:lambda:us-west-2:457972698173:function:ShellySlashCommand/invocations",
          "responses": {
            "default": {
              "statusCode": "200"
            }
          },
          "passthroughBehavior": "never",
          "httpMethod": "POST",
          "requestTemplates": {
            "application/x-www-form-urlencoded": "## convert HTML POST data or HTTP GET query string to JSON\r\n \r\n## get the raw post data from the AWS built-in variable and give it a nicer name\r\n#if ($context.httpMethod == \"POST\")\r\n #set($rawAPIData = $input.path(\"$\"))\r\n#elseif ($context.httpMethod == \"GET\")\r\n #set($rawAPIData = $input.params().querystring)\r\n #set($rawAPIData = $rawAPIData.toString())\r\n #set($rawAPIDataLength = $rawAPIData.length() - 1)\r\n #set($rawAPIData = $rawAPIData.substring(1, $rawAPIDataLength))\r\n #set($rawAPIData = $rawAPIData.replace(\", \", \"&\"))\r\n\r\n#else\r\n #set($rawAPIData = \"\")\r\n#end\r\n \r\n## Work around for Slack's stupidity:\r\n#set($rawAPIData = $rawAPIData.replace(\"%26amp%3B\", \"%26\"))\r\n#set($rawAPIData = $rawAPIData.replace(\"%26gt%3B\", \"%3C\"))\r\n#set($rawAPIData = $rawAPIData.replace(\"%26lt%3B\", \"%3E\"))\r\n \r\n## first we get the number of \"&\" in the string, this tells us if there is more than one key value pair\r\n#set($countAmpersands = $rawAPIData.length() - $rawAPIData.replace(\"&\", \"\").length())\r\n \r\n## if there are no \"&\" at all then we have only one key value pair.\r\n## we append an ampersand to the string so that we can tokenise it the same way as multiple kv pairs.\r\n## the \"empty\" kv pair to the right of the ampersand will be ignored anyway.\r\n#if ($countAmpersands == 0)\r\n #set($rawPostData = $rawAPIData + \"&\")\r\n#end\r\n \r\n## now we tokenise using the ampersand(s)\r\n#set($tokenisedAmpersand = $rawAPIData.split(\"&\"))\r\n \r\n## we set up a variable to hold the valid key value pairs\r\n#set($tokenisedEquals = [])\r\n \r\n## now we set up a loop to find the valid key value pairs, which must contain only one \"=\"\r\n#foreach( $kvPair in $tokenisedAmpersand )\r\n #set($countEquals = $kvPair.length() - $kvPair.replace(\"=\", \"\").length())\r\n #if ($countEquals == 1)\r\n  #set($kvTokenised = $kvPair.split(\"=\"))\r\n    #if ( ($kvTokenised.size() == 2) && ($kvTokenised[0].length() > 0) )\r\n   ## we found a valid key value pair. add it to the list.\r\n   #set($devNull = $tokenisedEquals.add($kvPair))\r\n  #end\r\n #end\r\n#end\r\n \r\n## next we set up our loop inside the output structure \"{\" and \"}\"\r\n{\r\n#foreach( $kvPair in $tokenisedEquals )\r\n  ## finally we output the JSON for this pair and append a comma if this isn't the last pair\r\n  #set($kvTokenised = $kvPair.split(\"=\"))\r\n \"$util.urlDecode($kvTokenised[0])\" : #if($kvTokenised.size() > 1 && $kvTokenised[1].length() > 0)\"$util.urlDecode($kvTokenised[1])\"#{else}\"\"#end#if( $foreach.hasNext ),#end\r\n#end\r\n}"
          },
          "contentHandling": "CONVERT_TO_TEXT",
          "type": "aws"
        }
      }
    }
  },
  "definitions": {
    "Empty": {
      "type": "object",
      "title": "Empty Schema"
    }
  }
}

Mapping Template
## convert HTML POST data or HTTP GET query string to JSON
 
## get the raw post data from the AWS built-in variable and give it a nicer name
#if ($context.httpMethod == "POST")
 #set($rawAPIData = $input.path("$"))
#elseif ($context.httpMethod == "GET")
 #set($rawAPIData = $input.params().querystring)
 #set($rawAPIData = $rawAPIData.toString())
 #set($rawAPIDataLength = $rawAPIData.length() - 1)
 #set($rawAPIData = $rawAPIData.substring(1, $rawAPIDataLength))
 #set($rawAPIData = $rawAPIData.replace(", ", "&"))

#else
 #set($rawAPIData = "")
#end
 
## Work around for Slack's stupidity:
#set($rawAPIData = $rawAPIData.replace("%26amp%3B", "%26"))
#set($rawAPIData = $rawAPIData.replace("%26gt%3B", "%3C"))
#set($rawAPIData = $rawAPIData.replace("%26lt%3B", "%3E"))
 
## first we get the number of "&" in the string, this tells us if there is more than one key value pair
#set($countAmpersands = $rawAPIData.length() - $rawAPIData.replace("&", "").length())
 
## if there are no "&" at all then we have only one key value pair.
## we append an ampersand to the string so that we can tokenise it the same way as multiple kv pairs.
## the "empty" kv pair to the right of the ampersand will be ignored anyway.
#if ($countAmpersands == 0)
 #set($rawPostData = $rawAPIData + "&")
#end
 
## now we tokenise using the ampersand(s)
#set($tokenisedAmpersand = $rawAPIData.split("&"))
 
## we set up a variable to hold the valid key value pairs
#set($tokenisedEquals = [])
 
## now we set up a loop to find the valid key value pairs, which must contain only one "="
#foreach( $kvPair in $tokenisedAmpersand )
 #set($countEquals = $kvPair.length() - $kvPair.replace("=", "").length())
 #if ($countEquals == 1)
  #set($kvTokenised = $kvPair.split("="))
    #if ( ($kvTokenised.size() == 2) && ($kvTokenised[0].length() > 0) )
   ## we found a valid key value pair. add it to the list.
   #set($devNull = $tokenisedEquals.add($kvPair))
  #end
 #end
#end
 
## next we set up our loop inside the output structure "{" and "}"
{
    "body": {
    #foreach( $kvPair in $tokenisedEquals )
      ## finally we output the JSON for this pair and append a comma if this isn't the last pair
      #set($kvTokenised = $kvPair.split("="))
     "$util.urlDecode($kvTokenised[0])" : #if($kvTokenised.size() > 1 && $kvTokenised[1].length() > 0)"$util.urlDecode($kvTokenised[1])"#{else}""#end#if( $foreach.hasNext ),#end
    #end
},
  "headers": {
    #foreach($param in $input.params().header.keySet())
    "$param": "$util.escapeJavaScript($input.params().header.get($param))" #if($foreach.hasNext),#end
    
    #end  
  }
}

    "function-subnets"     : "subnet-04f6a6c14f90d19cc",
    "function-security-groups" : "sg-05a8fb4959cfb5bed",