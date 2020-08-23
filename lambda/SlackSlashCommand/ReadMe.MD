## Slack Slash Command

# Step 1: Payload comes in from API
{
 "body": {
   "token": "DzIaa4JKVlC9Rt5GXTltAZD4",
   "team_id": "T03JXKJBE",
   "team_domain": "temenos",
   "channel_id": "GKX9Y4N4S",
   "channel_name": "privategroup",
   "user_id": "U9364SRSM",
   "user_name": "bmccoy",
   "command": "/freshy",
   "text": "dbquery",
   "response_url": "https://hooks.slack.com/commands/T03JXKJBE/1031829760931/iqTF9Lwf93iVaHa2r16sUjn3",
   "trigger_id": "1031829760995.3643664388.4704eeb9123349e8fde0a8630602bb76"
 },
 "headers": {
   "Accept": "application/json,/",
   "Accept-Encoding": "gzip,deflate",
   "Content-Type": "application/x-www-form-urlencoded",
   "Host": "cewmhpv4yk.execute-api.us-west-2.amazonaws.com",
   "User-Agent": "Slackbot 1.0 (+https://api.slack.com/robots)",
   "X-Amzn-Trace-Id": "Root=1-5e854f07-902098c28bfb70e88fd07e4e",
   "X-Forwarded-For": "54.146.246.162",
   "X-Forwarded-Port": "443",
   "X-Forwarded-Proto": "https",
   "X-Slack-Request-Timestamp": "1585794823",
   "X-Slack-Signature": "v0=226137a2fb56a765156e64252e837ce4eaf957ba2ca6450bb73f2b0b550f0919"
 }
}

# Step 2: Deseralize Object
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