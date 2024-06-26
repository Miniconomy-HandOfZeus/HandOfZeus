resource "aws_apigatewayv2_api" "api" {
  name          = "${var.naming_prefix}-service-api"
  description   = "API Gateway for the service endpoints"
  protocol_type = "HTTP"

  cors_configuration {
    allow_headers = ["*"]
    allow_methods = ["*"]
    allow_origins = ["*"]
    max_age       = 3000
  }
}

resource "aws_apigatewayv2_domain_name" "api" {
  domain_name = "api.zeus.projects.bbdgrad.com"

  domain_name_configuration {
    certificate_arn = "arn:aws:acm:eu-west-1:625366111301:certificate/91d22213-b9f0-4f3b-852e-b6bfc5054969"
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_authorizer" "cognito_jwt_authorizer" {
  name            = "CognitoJWTAuthorizer"
  api_id          = aws_apigatewayv2_api.api.id
  authorizer_type = "JWT"

  identity_sources = ["$request.header.Authorization"]

  jwt_configuration {
    audience = [aws_cognito_user_pool_client.client.id]
    issuer   = "https://cognito-idp.${var.region}.amazonaws.com/${aws_cognito_user_pool.user_pool.id}"
  }
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_apigatewayv2_api_mapping" "api" {
  api_id      = aws_apigatewayv2_api.api.id
  domain_name = aws_apigatewayv2_domain_name.api.domain_name
  stage       = aws_apigatewayv2_stage.default.id
}

resource "aws_apigatewayv2_integration" "lambda_integrations" {
  for_each         = var.lambda_endpoint_config
  api_id           = aws_apigatewayv2_api.api.id
  integration_type = "AWS_PROXY"

  connection_type    = "INTERNET"
  description        = each.value.description
  integration_method = each.value.method
  integration_uri    = each.value.lambda_invoke_arn
}

resource "aws_apigatewayv2_route" "lambda_routes" {
  for_each      = var.lambda_endpoint_config
  api_id        = aws_apigatewayv2_api.api.id
  route_key     = each.key
  authorizer_id = each.value.authorizer_id
  target        = "integrations/${aws_apigatewayv2_integration.lambda_integrations[each.key].id}"
}