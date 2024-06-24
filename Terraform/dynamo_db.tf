resource "aws_dynamodb_table" "db" {
  name           = "${var.naming_prefix}-db"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "Key"
  attribute {
    name = "Key"
    type = "S"
  }
  attribute {
    name = "Description"
    type = "S"
  }
  attribute {
    name = "Date"
    type = "S"
  }
  attribute {
    name = "Type"
    type = "S"
  }
}