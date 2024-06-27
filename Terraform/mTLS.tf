resource "aws_s3_bucket" "mtls" {
  bucket        = "miniconomy-trust-store-bucket"
  force_destroy = true
  tags          = { Name = "miniconomy-trust-store-bucket" }
}

resource "aws_s3_bucket_versioning" "mtls" {
  bucket = aws_s3_bucket.mtls.bucket
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_bucket_policy" "mtls" {
  bucket = aws_s3_bucket.mtls.bucket
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Principal = "*",
        Action = "s3:GetObject",
        Resource = [
          "${aws_s3_bucket.mtls.arn}/*",
        ],
        Condition = {
          StringEquals = {
            "aws:PrincipalAccount" = var.trusted_accounts_ids
          }
        }
      },
      {
        Effect = "Deny",
        Principal = "*",
        Action = "s3:*",
        Resource = [
          "${aws_s3_bucket.mtls.arn}/*",
        ],
        Condition = {
          StringNotEquals = {
            "aws:PrincipalAccount" = var.trusted_accounts_ids
          }
        }
      }
    ],
  })
}