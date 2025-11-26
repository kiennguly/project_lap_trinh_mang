#######################################
# Bucket avatar cho game chính
#######################################
resource "aws_s3_bucket" "avatargame" {
  bucket        = "avatargame"
  force_destroy = true

  tags = {
    Name = "avatargame"
  }
}

# Folder avatars/ trong bucket avatargame
resource "aws_s3_object" "avatargame_avatars" {
  bucket  = aws_s3_bucket.avatargame.id
  key     = "avatars/"
  content = ""
}

#######################################
# Bucket avatar máy bay (plane)
#######################################
resource "aws_s3_bucket" "avatarplane_minh" {
  bucket        = "avatarplane-minh"
  force_destroy = true

  tags = {
    Name = "avatarplane-minh"
  }
}

# Folder planes/ trong bucket avatarplane-minh
resource "aws_s3_object" "avatarplane_planes" {
  bucket  = aws_s3_bucket.avatarplane_minh.id
  key     = "planes/"
  content = ""
}

########################################
# IAM role tối thiểu cho Lambda
########################################
resource "aws_iam_role" "lambda_role" {
  name = "lambda-basic-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Action = "sts:AssumeRole",
      Effect = "Allow",
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_logs" {
  role       = aws_iam_role.lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

########################################
# Dùng chung 1 file empty.zip cho 6 Lambda
########################################
locals {
  lambda_zip = "${path.module}/empty.zip"
}

########################################
# 1. DOAN - Python 3.13
########################################
resource "aws_lambda_function" "doan" {
  function_name = "DOAN"
  role          = aws_iam_role.lambda_role.arn
  handler       = "empty_lambda.lambda_handler"
  runtime       = "python3.13"

  filename         = local.lambda_zip
  source_code_hash = filebase64sha256(local.lambda_zip)
}

########################################
# 2. soloqualan - Python 3.14
########################################
resource "aws_lambda_function" "soloqualan" {
  function_name = "soloqualan"
  role          = aws_iam_role.lambda_role.arn
  handler       = "empty_lambda.lambda_handler"
  runtime       = "python3.14"

  filename         = local.lambda_zip
  source_code_hash = filebase64sha256(local.lambda_zip)
}

########################################
# 3. anhuser - Python 3.14
########################################
resource "aws_lambda_function" "anhuser" {
  function_name = "anhuser"
  role          = aws_iam_role.lambda_role.arn
  handler       = "empty_lambda.lambda_handler"
  runtime       = "python3.14"

  filename         = local.lambda_zip
  source_code_hash = filebase64sha256(local.lambda_zip)
}

########################################
# 4. avatarplane - Python 3.14
########################################
resource "aws_lambda_function" "avatarplane" {
  function_name = "avatarplane"
  role          = aws_iam_role.lambda_role.arn
  handler       = "empty_lambda.lambda_handler"
  runtime       = "python3.14"

  filename         = local.lambda_zip
  source_code_hash = filebase64sha256(local.lambda_zip)
}

########################################
# 5. RedeemGiftcodeFunction - Python 3.12
########################################
resource "aws_lambda_function" "redeem" {
  function_name = "RedeemGiftcodeFunction"
  role          = aws_iam_role.lambda_role.arn
  handler       = "empty_lambda.lambda_handler"
  runtime       = "python3.12"

  filename         = local.lambda_zip
  source_code_hash = filebase64sha256(local.lambda_zip)
}

########################################
# 6. xephang - Python 3.13
########################################
resource "aws_lambda_function" "xephang" {
  function_name = "xephang"
  role          = aws_iam_role.lambda_role.arn
  handler       = "empty_lambda.lambda_handler"
  runtime       = "python3.13"

  filename         = local.lambda_zip
  source_code_hash = filebase64sha256(local.lambda_zip)
}

########################################
# DynamoDB tables
########################################

resource "aws_dynamodb_table" "account_data" {
  name         = "AccountData"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Username"

  attribute {
    name = "Username"
    type = "S"
  }

  tags = {
    Name = "AccountData"
  }
}

resource "aws_dynamodb_table" "bang_xep_hang" {
  name         = "BangXepHang"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Username"

  attribute {
    name = "Username"
    type = "S"
  }

  tags = {
    Name = "BangXepHang"
  }
}

resource "aws_dynamodb_table" "giftcode" {
  name         = "giftcode"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Namecode"

  attribute {
    name = "Namecode"
    type = "S"
  }

  tags = {
    Name = "giftcode"
  }
}

resource "aws_dynamodb_table" "match_history" {
  name         = "MatchHistory"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  tags = {
    Name = "MatchHistory"
  }
}

resource "aws_dynamodb_table" "sololan" {
  name         = "sololan"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "RoomID"

  attribute {
    name = "RoomID"
    type = "S"
  }

  tags = {
    Name = "sololan"
  }
}

###########################
# API: anhuser
###########################
resource "aws_apigatewayv2_api" "anhuser_api" {
  name          = "anhuser"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "anhuser_integration" {
  api_id           = aws_apigatewayv2_api.anhuser_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.anhuser.invoke_arn
}

resource "aws_apigatewayv2_route" "anhuser_image" {
  api_id    = aws_apigatewayv2_api.anhuser_api.id
  route_key = "POST /image"
  target    = "integrations/${aws_apigatewayv2_integration.anhuser_integration.id}"
}

resource "aws_apigatewayv2_stage" "anhuser_stage" {
  api_id      = aws_apigatewayv2_api.anhuser_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "anhuser_permission" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.anhuser.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.anhuser_api.execution_arn}/*/*"
}

###########################
# API: avatarplane
###########################
resource "aws_apigatewayv2_api" "avatarplane_api" {
  name          = "avatarplane"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "avatarplane_integration" {
  api_id           = aws_apigatewayv2_api.avatarplane_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.avatarplane.invoke_arn
}

resource "aws_apigatewayv2_route" "avatarplane_post" {
  api_id    = aws_apigatewayv2_api.avatarplane_api.id
  route_key = "POST /post/plane"
  target    = "integrations/${aws_apigatewayv2_integration.avatarplane_integration.id}"
}

resource "aws_apigatewayv2_stage" "avatarplane_stage" {
  api_id      = aws_apigatewayv2_api.avatarplane_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "avatarplane_permission" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.avatarplane.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.avatarplane_api.execution_arn}/*/*"
}

###########################
# API: gamesolo
###########################
resource "aws_apigatewayv2_api" "gamesolo_api" {
  name          = "gamesolo"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "gamesolo_integration" {
  api_id           = aws_apigatewayv2_api.gamesolo_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.soloqualan.invoke_arn
}

resource "aws_apigatewayv2_route" "gamesolo_layidroom" {
  api_id    = aws_apigatewayv2_api.gamesolo_api.id
  route_key = "GET /layidroom"
  target    = "integrations/${aws_apigatewayv2_integration.gamesolo_integration.id}"
}

resource "aws_apigatewayv2_route" "gamesolo_sololan" {
  api_id    = aws_apigatewayv2_api.gamesolo_api.id
  route_key = "POST /sololan"
  target    = "integrations/${aws_apigatewayv2_integration.gamesolo_integration.id}"
}

resource "aws_apigatewayv2_stage" "gamesolo_stage" {
  api_id      = aws_apigatewayv2_api.gamesolo_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "gamesolo_permission" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.soloqualan.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.gamesolo_api.execution_arn}/*/*"
}

###########################
# API: loginregister
###########################
resource "aws_apigatewayv2_api" "loginregister_api" {
  name          = "loginregister"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "loginregister_integration" {
  api_id           = aws_apigatewayv2_api.loginregister_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.doan.invoke_arn
}

locals {
  login_routes = [
    "POST /account/set-status",
    "POST /account/register",
    "PUT  /account/update",
    "POST /account/login",
    "GET  /account/{username}",
    "POST /account/change-password",
  ]
}

resource "aws_apigatewayv2_route" "login_routes" {
  for_each = toset(local.login_routes)

  api_id    = aws_apigatewayv2_api.loginregister_api.id
  route_key = each.value
  target    = "integrations/${aws_apigatewayv2_integration.loginregister_integration.id}"
}

resource "aws_apigatewayv2_stage" "loginregister_stage" {
  api_id      = aws_apigatewayv2_api.loginregister_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "loginregister_perm" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.doan.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.loginregister_api.execution_arn}/*/*"
}

###########################
# API: MatchHistory
###########################
resource "aws_apigatewayv2_api" "matchhistory_api" {
  name          = "MatchHistory"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "matchhistory_integration" {
  api_id           = aws_apigatewayv2_api.matchhistory_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.doan.invoke_arn
}

resource "aws_apigatewayv2_route" "matchhistory_add" {
  api_id    = aws_apigatewayv2_api.matchhistory_api.id
  route_key = "POST /matchhistory/add"
  target    = "integrations/${aws_apigatewayv2_integration.matchhistory_integration.id}"
}

resource "aws_apigatewayv2_route" "matchhistory_get" {
  api_id    = aws_apigatewayv2_api.matchhistory_api.id
  route_key = "GET /matchhistory/{username}"
  target    = "integrations/${aws_apigatewayv2_integration.matchhistory_integration.id}"
}

resource "aws_apigatewayv2_stage" "matchhistory_stage" {
  api_id      = aws_apigatewayv2_api.matchhistory_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "matchhistory_permission" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.doan.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.matchhistory_api.execution_arn}/*/*"
}

###########################
# API: RedeemGiftcodeFunction-API
###########################
resource "aws_apigatewayv2_api" "redeem_api" {
  name          = "RedeemGiftcodeFunction-API"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "redeem_integration" {
  api_id           = aws_apigatewayv2_api.redeem_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.redeem.invoke_arn
}

resource "aws_apigatewayv2_route" "redeem_route" {
  api_id    = aws_apigatewayv2_api.redeem_api.id
  route_key = "ANY /RedeemGiftcodeFunction"
  target    = "integrations/${aws_apigatewayv2_integration.redeem_integration.id}"
}

resource "aws_apigatewayv2_stage" "redeem_stage" {
  api_id      = aws_apigatewayv2_api.redeem_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "redeem_perm" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.redeem.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.redeem_api.execution_arn}/*/*"
}

###########################
# API: xephang
###########################
resource "aws_apigatewayv2_api" "xephang_api" {
  name          = "xephang"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "xephang_integration" {
  api_id           = aws_apigatewayv2_api.xephang_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.xephang.invoke_arn
}

resource "aws_apigatewayv2_route" "xephang_get" {
  api_id    = aws_apigatewayv2_api.xephang_api.id
  route_key = "GET /get"
  target    = "integrations/${aws_apigatewayv2_integration.xephang_integration.id}"
}

resource "aws_apigatewayv2_route" "xephang_post" {
  api_id    = aws_apigatewayv2_api.xephang_api.id
  route_key = "POST /post"
  target    = "integrations/${aws_apigatewayv2_integration.xephang_integration.id}"
}

resource "aws_apigatewayv2_stage" "xephang_stage" {
  api_id      = aws_apigatewayv2_api.xephang_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_lambda_permission" "xephang_perm" {
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.xephang.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.xephang_api.execution_arn}/*/*"
}
