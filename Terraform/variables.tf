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

variable "bucket_name" {
  type        = string
  description = "Name of the bucket."
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