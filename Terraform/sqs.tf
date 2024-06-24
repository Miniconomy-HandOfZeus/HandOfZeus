resource "aws_sqs_queue" "daily_broadcast_persona" {
  name                        = "DailyBroadcast_Persona.fifo"
  fifo_queue                  = true
  content_based_deduplication = true
}

data "aws_iam_policy_document" "daily_broadcast_persona" {
  statement {
    sid    = "DailyBroadcastPersonaPolicy"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = ["179530787873"]
    }

    actions   = ["sqs:*"]
    resources = [aws_sqs_queue.daily_broadcast_persona.arn]
  }
}

resource "aws_sqs_queue_policy" "daily_broadcast_persona" {
  queue_url = aws_sqs_queue.daily_broadcast_persona.id
  policy    = data.aws_iam_policy_document.daily_broadcast_persona.json
}