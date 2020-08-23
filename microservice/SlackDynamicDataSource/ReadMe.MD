## Slack Dynamic Data Source

# Step 1: Payload from slack comes in

{
 "body": "payload=%7B%22type%22%3A%22dialog_suggestion%22%2C%22token%22%3A%22H1SvLWnFUASZPKW04vdfGpxl%22%2C%22action_ts%22%3A%221585373739.940102%22%2C%22team%22%3A%7B%22id%22%3A%22T03JXKJBE%22%2C%22domain%22%3A%22temenos%22%7D%2C%22user%22%3A%7B%22id%22%3A%22U9364SRSM%22%2C%22name%22%3A%22bmccoy%22%7D%2C%22channel%22%3A%7B%22id%22%3A%22GGCG5TTPC%22%2C%22name%22%3A%22privategroup%22%7D%2C%22name%22%3A%22environment%22%2C%22value%22%3A%22%22%2C%22callback_id%22%3A%22dbquery%22%2C%22state%22%3A%22%27ClientCode%27%3A%27tmregression%27%2C%27dialog%27%3A%272%27%2C%27number_of_dialogs%27%3A%272%27%2C%27ShowHeaders%27%3A%27true%27%2C%27self_service_environment%27%3A%27production%27%2C%27callback_id%27%3A%27dbquery%27%22%7D",
 "headers": {
   "Accept": "application/json,/",
   "Accept-Encoding": "gzip,deflate",
   "Content-Type": "application/x-www-form-urlencoded",
   "Host": "cewmhpv4yk.execute-api.us-west-2.amazonaws.com",
   "User-Agent": "Slackbot 1.0 (+https://api.slack.com/robots)",
   "X-Amzn-Trace-Id": "Root=1-5e7ee22c-31b8f886b88eb6ead51299a2",
   "X-Forwarded-For": "34.239.111.232",
   "X-Forwarded-Port": "443",
   "X-Forwarded-Proto": "https",
   "X-Slack-Request-Timestamp": "1585373739",
   "X-Slack-Signature": "v0=763040618e994ee3bc1723b2d45fde03a89d682407afa4aad88bda6b27a3b713"
 }
}

# Step 2. Calculate Sha and match to verify.

{"type":"dialog_suggestion","token":"H1SvLWnFUASZPKW04vdfGpxl","action_ts":"1585373739.940102","team":{"id":"T03JXKJBE","domain":"temenos"},"user":{"id":"U9364SRSM","name":"bmccoy"},"channel":{"id":"GGCG5TTPC","name":"privategroup"},"name":"environment","value":"","callback_id":"dbquery","state":"'ClientCode':'tmregression','dialog':'2','number_of_dialogs':'2','ShowHeaders':'true','self_service_environment':'production','callback_id':'dbquery'"}

# Step 3. External Data Source 
Get the External Data Source by getting the headers and then querying the command table for the external data json.

{ "values": [{ "source": "DynamoDb", "name": "environment","table": "customer", "key": "customer_code", "attribute": "environments","value":"ClientCode" }] }

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

# Step 4a DynamoDb Query
Query the 
# Step 5b Lambda Query

# Step 6 
{'options':[{'label':'TEST','value':'TEST'},{'label':'DEV','value':'DEV'},{'label':'PROD','value':'PROD'},{'label':'STAG','value':'STAG'}]}

** Important note is that and illegal string is "url=" as slack cant handle it

