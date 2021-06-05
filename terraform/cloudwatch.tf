# ---------------------------------------------------------------------------------------------------------------------
# Cloudwatch Rule
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_cloudwatch_event_rule" "framework_test" {
    name = "framework-daily-test"
    description = "Daily Test of Framework"
    schedule_expression = "rate(1 week)"
}

resource "aws_cloudwatch_event_target" "framework_test" {
    rule = aws_cloudwatch_event_rule.framework_test.name
    target_id = "framework_test"
    arn = aws_lambda_function.scheduling_producer.arn
    input = <<DOC
    {
        "service_id": "test",
        "service_version": "1",
        "source": "CloudwatchRule",
        "command": ["bin/console", "scheduled-task"]
    }
    DOC
}

resource "aws_lambda_permission" "cloudwatch_rule_permission" {
    statement_id = "AllowExecutionFromCloudWatch"
    action = "lambda:InvokeFunction"
    function_name = aws_lambda_function.scheduling_producer.function_name
    principal = "events.amazonaws.com"
    source_arn = aws_cloudwatch_event_rule.framework_test.arn
}