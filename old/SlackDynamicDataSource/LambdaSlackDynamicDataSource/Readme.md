# AWS API Details

## Integration Request

Mapping Templates = application/x-www-form-urlencoded
{ "body": "$input.path('$')" }

## Integration Request

Mapping Templates = application/json
$input.path('$')

## Example Input Payload

{"type":"dialog_suggestion","token":"H1SvLWnFUASZPKW04vdfGpxl","action_ts":"1554091005.958659","team":{"id":"T03JXKJBE","domain":"avoka"},"user":{"id":"U9364SRSM","name":"bmccoy"},"channel":{"id":"GGCG5TTPC","name":"privategroup"},"name":"tm_license_key","value":"","callback_id":"dbquery","state":"Limo"}
