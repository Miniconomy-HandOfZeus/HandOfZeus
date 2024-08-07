variable "common_tags" {
  type        = map(string)
  description = "Common tags applied to all resources"
}

variable "region" {
  type        = string
  description = "The region where the resources will be deployed."
}

variable "naming_prefix" {
  type        = string
  description = "The prefix to use for naming resources."
}

variable "client_id" {
  type        = string
  description = "The client ID for the identity provider."
}

variable "client_secret" {
  type        = string
  description = "The client secret for the identity provider."
  sensitive   = true
}

variable "callback_urls" {
  type        = list(string)
  description = "The callback URLs for the identity provider."
}

variable "logout_urls" {
  type        = list(string)
  description = "The logout URLs for the identity provider."
}

variable "service_lambda_endpoint_config" {
  type = map(object({ # The map key should be the route key eg: GET /helloworld
    method            = string
    description       = string
    lambda_invoke_arn = string
    db_key            = string
    allowed_services  = list(string)
  }))
  description = "The configuration for the integration of Lambda functions into API gateway."

  default = {}
}

variable "user_lambda_endpoint_config" {
  type = map(object({ # The map key should be the route key eg: GET /helloworld
    method            = string
    description       = string
    lambda_invoke_arn = string
  }))
  description = "The configuration for the integration of Lambda functions into API gateway."

  default = {}
}

variable "lambda_schedule_config" {
  type = list(object({
    name              = string
    rate_expression   = string # eg: 2 minutes
    lambda_invoke_arn = string
  }))
  description = "The configuration for the scheduled Lambda functions such as random events."

  default = []
}

variable "trusted_account_ids" {
  type        = list(string)
  description = "The account IDs that are trusted to access the trust store."
}

variable "cors_allowed_origins" {
  type        = list(string)
  description = "The allowed origins for CORS."
}