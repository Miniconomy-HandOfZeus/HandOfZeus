resource "aws_apigatewayv2_api" "api" {
  name          = "${var.naming_prefix}-service-api"
  description   = "API Gateway for the service endpoints"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_domain_name" "api" {
  domain_name = "api.zeus.projects.bbdgrad.com"

  domain_name_configuration {
    certificate_arn = "arn:aws:acm:us-east-1:625366111301:certificate/584d9cbb-d67d-4acf-aa23-5b4faaa99ca2"
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_api_mapping" "api" {
  api_id      = aws_apigatewayv2_api.api.id
  domain_name = aws_apigatewayv2_domain_name.api.domain_name
  stage       = "$default"
}

resource "aws_apigatewayv2_integration" "lambda_integrations" {
  for_each         = { for index, config in var.lambda_endpoint_config : index => config }
  api_id           = aws_apigatewayv2_api.api.id
  integration_type = "AWS_PROXY"

  connection_type    = "INTERNET"
  description        = each.value.description
  integration_method = each.value.method
  integration_uri    = each.value.lambda_invoke_arn
}