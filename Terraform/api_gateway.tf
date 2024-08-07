# Cloudwatch logs
resource "aws_api_gateway_account" "api-gateway" {
  cloudwatch_role_arn = aws_iam_role.api_gateway.arn
}

resource "aws_iam_role" "api_gateway" {
  name               = "${var.naming_prefix}-api-gateway-role"
  assume_role_policy = data.aws_iam_policy_document.gateway_assume_role.json
}

resource "aws_iam_role_policy_attachment" "api_gateway" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonAPIGatewayPushToCloudWatchLogs"
  role       = aws_iam_role.api_gateway.name
}

resource "aws_cloudwatch_log_group" "service_log_group" {
  name              = "/aws/apigateway/${aws_apigatewayv2_api.service_api.name}"
  retention_in_days = 7
}

resource "aws_cloudwatch_log_group" "user_log_group" {
  name              = "/aws/apigateway/${aws_apigatewayv2_api.user_api.name}"
  retention_in_days = 7
}

# SERVICE API
resource "aws_apigatewayv2_api" "service_api" {
  name          = "${var.naming_prefix}-service-api"
  description   = "API Gateway for the service endpoints"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_domain_name" "service_api" {
  domain_name = "api.zeus.projects.bbdgrad.com"

  domain_name_configuration {
    certificate_arn = "arn:aws:acm:eu-west-1:625366111301:certificate/91d22213-b9f0-4f3b-852e-b6bfc5054969"
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_stage" "service_api" {
  api_id      = aws_apigatewayv2_api.service_api.id
  name        = "$default"
  auto_deploy = true

  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.service_log_group.arn
    format          = "$context.requestId $context.identity.sourceIp $context.identity.caller $context.identity.user $context.requestTime $context.httpMethod $context.resourcePath $context.status $context.protocol $context.responseLength $context.error.message"
  }
}

resource "aws_apigatewayv2_api_mapping" "service_api" {
  api_id      = aws_apigatewayv2_api.service_api.id
  domain_name = aws_apigatewayv2_domain_name.service_api.domain_name
  stage       = aws_apigatewayv2_stage.service_api.id
}

resource "aws_apigatewayv2_integration" "service_api" {
  for_each         = var.service_lambda_endpoint_config
  api_id           = aws_apigatewayv2_api.service_api.id
  integration_type = "AWS_PROXY"

  connection_type    = "INTERNET"
  description        = each.value.description
  integration_method = each.value.method
  integration_uri    = each.value.lambda_invoke_arn

  request_parameters = {
    "overwrite:querystring.key"              = each.value.db_key,
    "overwrite:querystring.allowed_services" = join(",", each.value.allowed_services)
  }
}

resource "aws_apigatewayv2_route" "service_api" {
  for_each  = var.service_lambda_endpoint_config
  api_id    = aws_apigatewayv2_api.service_api.id
  route_key = each.key
  target    = "integrations/${aws_apigatewayv2_integration.service_api[each.key].id}"
}

resource "aws_apigatewayv2_integration" "service_options_integration" {
  api_id           = aws_apigatewayv2_api.service_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = "arn:aws:lambda:eu-west-1:625366111301:function:OptionsLambda"
}

resource "aws_apigatewayv2_route" "service_options_proxy_route" {
  api_id             = aws_apigatewayv2_api.service_api.id
  route_key          = "OPTIONS /{proxy+}"
  authorization_type = "NONE"
  target             = "integrations/${aws_apigatewayv2_integration.service_options_integration.id}"
}

#USER API
resource "aws_apigatewayv2_api" "user_api" {
  name          = "${var.naming_prefix}-user-api"
  description   = "API Gateway for the service endpoints"
  protocol_type = "HTTP"

  cors_configuration {
    allow_credentials = true
    allow_headers     = ["Content-Type", "Authorization"]
    allow_methods     = ["GET", "POST", "PUT", "DELETE", "OPTIONS"]
    allow_origins     = var.cors_allowed_origins
    max_age           = 3000
  }
}

resource "aws_apigatewayv2_domain_name" "user_api" {
  domain_name = "events.zeus.projects.bbdgrad.com"

  domain_name_configuration {
    certificate_arn = "arn:aws:acm:eu-west-1:625366111301:certificate/ab6814a1-dbb5-4a1b-a546-49c7ead5a0cf"
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_authorizer" "cognito_jwt_authorizer" {
  name            = "CognitoJWTAuthorizer"
  api_id          = aws_apigatewayv2_api.user_api.id
  authorizer_type = "JWT"

  identity_sources = ["$request.header.Authorization"]

  jwt_configuration {
    audience = [aws_cognito_user_pool_client.client.id]
    issuer   = "https://cognito-idp.${var.region}.amazonaws.com/${aws_cognito_user_pool.user_pool.id}"
  }
}

resource "aws_apigatewayv2_stage" "user_api" {
  api_id      = aws_apigatewayv2_api.user_api.id
  name        = "$default"
  auto_deploy = true

  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.user_log_group.arn
    format          = "$context.requestId $context.identity.sourceIp $context.identity.caller $context.identity.user $context.requestTime $context.httpMethod $context.resourcePath $context.status $context.protocol $context.responseLength $context.error.message"
  }
}

resource "aws_apigatewayv2_api_mapping" "user_api" {
  api_id      = aws_apigatewayv2_api.user_api.id
  domain_name = aws_apigatewayv2_domain_name.user_api.domain_name
  stage       = aws_apigatewayv2_stage.user_api.id
}

resource "aws_apigatewayv2_integration" "user_api" {
  for_each         = var.user_lambda_endpoint_config
  api_id           = aws_apigatewayv2_api.user_api.id
  integration_type = "AWS_PROXY"

  connection_type    = "INTERNET"
  description        = each.value.description
  integration_method = each.value.method
  integration_uri    = each.value.lambda_invoke_arn
}

resource "aws_apigatewayv2_route" "user_api" {
  for_each           = var.user_lambda_endpoint_config
  api_id             = aws_apigatewayv2_api.user_api.id
  route_key          = each.key
  authorization_type = "JWT"
  authorizer_id      = aws_apigatewayv2_authorizer.cognito_jwt_authorizer.id
  target             = "integrations/${aws_apigatewayv2_integration.user_api[each.key].id}"
}

resource "aws_apigatewayv2_integration" "user_options_integration" {
  api_id           = aws_apigatewayv2_api.user_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = "arn:aws:lambda:eu-west-1:625366111301:function:OptionsLambda"
}

resource "aws_apigatewayv2_route" "user_options_proxy_route" {
  api_id             = aws_apigatewayv2_api.user_api.id
  route_key          = "OPTIONS /{proxy+}"
  authorization_type = "NONE"
  target             = "integrations/${aws_apigatewayv2_integration.user_options_integration.id}"
}