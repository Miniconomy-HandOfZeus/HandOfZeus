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

resource "aws_s3_bucket_policy" "mtls" {
  bucket = aws_s3_bucket.mtls.bucket
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Sid       = "AllowOrganizationToReadBucket",
      Effect    = "Allow",
      Principal = "*",
      Action = [
        "s3:GetObject",
        "s3:ListBucket"
      ],
      Resource : [
        aws_s3_bucket.mtls.arn,
        "${aws_s3_bucket.mtls.arn}/*"
      ],
      "Condition" : {
        "StringEquals" : { "aws:PrincipalOrgID" : ["ou-t4b2-wl5wlvk1"] }
      }
    }]
  })
}