resource "aws_iam_role" "lambda_execution_role" {
  name               = "${var.naming_prefix}-lambda-role"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume_role.json
}

resource "aws_iam_role_policy_attachment" "lambda_policy_attachment" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaSQSQueueExecutionRole"
  role       = aws_iam_role.lambda_execution_role.name
}

resource "aws_iam_policy" "lambda_dynamodb_full_access" {
  name        = "${var.naming_prefix}-lambda-dynamodb-full-access"
  description = "Policy that grants full access to DynamoDB"
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect   = "Allow",
        Action   = "dynamodb:*",
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_dynamodb_full_access_attachment" {
  policy_arn = aws_iam_policy.lambda_dynamodb_full_access.arn
  role       = aws_iam_role.lambda_execution_role.name
}

resource "aws_iam_role" "scheduler_execution_role" {
  name               = "${var.naming_prefix}-scheduler-role"
  assume_role_policy = data.aws_iam_policy_document.scheduler_assume_role.json
}

resource "aws_iam_policy" "lambda_invocation_policy" {
  name        = "${var.naming_prefix}-lambda-invocation-policy"
  description = "Policy for invoking lambda functions"
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect   = "Allow",
        Action   = "lambda:InvokeFunction",
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "scheduler_policy_attachment" {
  policy_arn = aws_iam_policy.lambda_invocation_policy.arn
  role       = aws_iam_role.scheduler_execution_role.name
}

resource "aws_scheduler_schedule_group" "lambda_schedule_group" {
  name = "${var.naming_prefix}-events-scheduler-group"
}

resource "aws_scheduler_schedule" "lambda_schedules" {
  for_each   = { for index, config in var.lambda_schedule_config : index => config }
  name       = "${each.value.name}-scheduler"
  group_name = aws_scheduler_schedule_group.lambda_schedule_group.name

  flexible_time_window {
    mode = "OFF"
  }

  schedule_expression = "rate(${each.value.rate_expression})"

  target {
    arn      = each.value.lambda_invoke_arn
    role_arn = aws_iam_role.scheduler_execution_role.arn
  }
}