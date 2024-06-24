resource "aws_iam_role" "lambda_execution_role" {
  name               = "${var.naming_prefix}-lambda-role"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume_role.json
}

resource "aws_iam_role_policy_attachment" "lambda_policy_attachment" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaSQSQueueExecutionRole"
  role       = aws_iam_role.lambda_execution_role.name
}

## S3 to store the lambda code
# resource "aws_s3_bucket" "lambda_bucket" {
#   bucket = "${var.naming_prefix}-lambda-bucket"
# }

# ## EVENT LAMBDAS

# resource "aws_lambda_function" "daily_broadcast" {
#   function_name = "DailyBroadcast"
#   role          = aws_iam_role.lambda_execution_role.arn
#   handler       = "DailyBroadcast::DailyBroadcast.Function::FunctionHandler"
#   s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
#   s3_key        = "DailyBroadcast.zip"

#   runtime = "dotnet8"
# }

# ## ENDPOINT LAMBDAS

# resource "aws_lambda_function" "get_events" {
#   function_name = "GetEvents"
#   role          = aws_iam_role.lambda_execution_role.arn
#   handler       = "GetEvents::GetEvents.Function::FunctionHandler"
#   s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
#   s3_key        = "GetEvents.zip"

#   runtime = "dotnet8"
# }