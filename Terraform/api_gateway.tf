resource "aws_apigatewayv2_api" "service_endpoints" {
  name          = "${var.naming_prefix}-service-api"
  description   = "API Gateway for the service endpoints"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "lambda_integrations" {
  for_each         = { for index, config in var.lambda_endpoint_config : index => config }
  api_id           = aws_apigatewayv2_api.service_endpoints.id
  integration_type = "AWS_PROXY"

  connection_type    = "INTERNET"
  description        = each.value.description
  integration_method = each.value.method
  integration_uri    = each.value.lambda_invoke_arn
}