resource "aws_iam_role" "lambda_execution_role" {
  name               = "${var.naming_prefix}-lambda-role"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume_role.json
}

resource "aws_iam_role_policy_attachment" "lambda_policy_attachment" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaSQSQueueExecutionRole"
  role       = aws_iam_role.eb_instance_role.name
}

resource "aws_lambda_function" "daily_broadcast" {
  function_name = "DailyBroadcast"
  role          = aws_iam_role.lambda_execution_role.arn
  handler       = "DailyBroadcast::DailyBroadcast.Function::FunctionHandler"

  runtime = "dotnet8"
}