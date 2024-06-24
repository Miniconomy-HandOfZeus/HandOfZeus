resource "aws_apigatewayv2_api" "service_endpoints" {
  name          = "${var.naming_prefix}-service-api"
  description   = "API Gateway for the service endpoints"
  protocol_type = "HTTP"

  cors_configuration = {
    allow_credentials = true
    allow_headers     = ["*"]
    allow_methods     = ["GET", "POST", "PUT", "DELETE", "OPTIONS"]
    allow_origins     = ["*"]
    expose_headers    = ["*"]
    max_age           = 3000
  }
}

resource "aws_apigatewayv2_integration" "example" {
  api_id           = aws_apigatewayv2_api.service_endpoints.id
  integration_type = "AWS_PROXY"

  connection_type           = "INTERNET"
  description               = "Lambda example"
  integration_method        = "GET"
  integration_uri           = aws_lambda_function.example.invoke_arn
}