import json
import boto3
import uuid
from datetime import datetime
from boto3.dynamodb.conditions import Key, Attr
from botocore.exceptions import ClientError
from decimal import Decimal
import random
import time

# --- DynamoDB ---
dynamodb = boto3.resource('dynamodb')
ACCOUNT_TABLE = dynamodb.Table('AccountData')
MATCH_TABLE = dynamodb.Table('MatchHistory')

# --- SNS (dùng để gửi mã qua email, nhớ sửa ARN thật) ---
sns = boto3.client('sns')
SNS_TOPIC_ARN = "arn:aws:sns:ap-southeast-1:123456789012:YOUR_TOPIC_NAME"

# --- JSON helper ---
class DecimalEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, Decimal):
            return int(obj) if obj % 1 == 0 else float(obj)
        return super().default(obj)

def create_response(statusCode, body):
    return {
        "statusCode": statusCode,
        "headers": {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*"
        },
        "body": json.dumps(body, cls=DecimalEncoder)
    }

# ============================
# Email mã reset
# ============================
def send_reset_code_email(email, username, code):
    subject = "Mã đặt lại mật khẩu - Plane Fighting Super Start"
    message = (
        f"Xin chào {username},\n\n"
        f"Mã đặt lại mật khẩu của bạn là: {code}\n"
        f"Mã này có hiệu lực trong 10 phút.\n\n"
        "Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này."
    )
    sns.publish(TopicArn=SNS_TOPIC_ARN, Subject=subject, Message=message)

# ============================
# 1) Đăng ký
# ============================
def handle_register(body):
    username = body.get("Username", "").strip()
    password = body.get("Password", "").strip()
    email = body.get("Email", "").strip()

    if not username or not password or not email:
        return create_response(400, {"message": "Thiếu Username, Password hoặc Email"})
    if "@" not in email or "." not in email:
        return create_response(400, {"message": "Email không hợp lệ"})

    try:
        ACCOUNT_TABLE.put_item(
            Item={
                "Username": username,
                "Password": password,
                "Email": email,

                "Gold": 0,
                "Level": 1,
                "UpgradeHP": 100,
                "UpgradeDamage": 0,

                "RewardLv10Claimed": False,
                "RewardLv50Claimed": False,
                "RewardLv100Claimed": False,

                "Online": False   # <-- trạng thái mặc định
            },
            ConditionExpression="attribute_not_exists(Username)"
        )
        return create_response(200, {"message": "Đăng ký thành công"})
    except ClientError as e:
        if e.response['Error']['Code'] == 'ConditionalCheckFailedException':
            return create_response(409, {"message": "Tên đăng nhập đã tồn tại"})
        print("Lỗi ghi DynamoDB:", e)
        return create_response(500, {"message": "Lỗi hệ thống khi đăng ký"})

# ============================
# 2) Đăng nhập
# ============================
def handle_login(body):
    username = body.get("Username", "").strip()
    password = body.get("Password", "").strip()
    if not username or not password:
        return create_response(400, {"message": "Thiếu Username hoặc Password"})

    resp = ACCOUNT_TABLE.get_item(Key={'Username': username})
    acc = resp.get('Item')
    if not acc:
        return create_response(401, {"message": "Username không tồn tại"})
    if str(acc.get("Password", "")) != password:
        return create_response(401, {"message": "Password không đúng"})

    # kiểm tra online hiện tại
    current_online = acc.get("Online", False)

    if isinstance(current_online, bool) and current_online:
        return create_response(
            409,
            {"message": "Tài khoản đang được đăng nhập ở nơi khác. Vui lòng thử lại sau."}
        )

    # cập nhật online = true
    try:
        ACCOUNT_TABLE.update_item(
            Key={"Username": username},
            UpdateExpression="SET #O = :true",
            ExpressionAttributeValues={":true": True},
            ExpressionAttributeNames={"#O": "Online"}
        )
    except ClientError as e:
        print("Lỗi update Online khi login:", e)
        return create_response(500, {"message": "Lỗi hệ thống khi đăng nhập"})

    # trả về account (ẩn password)
    acc.pop("Password", None)
    acc.setdefault("RewardLv10Claimed", False)
    acc.setdefault("RewardLv50Claimed", False)
    acc.setdefault("RewardLv100Claimed", False)
    acc["Online"] = True

    return create_response(200, acc)

# ============================
# 3) Cập nhật account
# ============================
def handle_update_account(body):
    username = body.get("Username", "").strip()
    if not username:
        return create_response(400, {"message": "Thiếu Username"})

    gold = int(body.get("Gold", 0))
    upgrade_hp = int(body.get("UpgradeHP", 100))
    upgrade_damage = int(body.get("UpgradeDamage", 0))
    level = int(body.get("Level", 1))

    reward10 = bool(body.get("RewardLv10Claimed", False))
    reward50 = bool(body.get("RewardLv50Claimed", False))
    reward100 = bool(body.get("RewardLv100Claimed", False))

    try:
        ACCOUNT_TABLE.update_item(
            Key={"Username": username},
            UpdateExpression=(
                "SET Gold = :g, "
                "UpgradeHP = :h, "
                "UpgradeDamage = :d, "
                "#L = :l, "
                "RewardLv10Claimed = :r10, "
                "RewardLv50Claimed = :r50, "
                "RewardLv100Claimed = :r100"
            ),
            ExpressionAttributeValues={
                ":g": gold,
                ":h": upgrade_hp,
                ":d": upgrade_damage,
                ":l": level,
                ":r10": reward10,
                ":r50": reward50,
                ":r100": reward100,
            },
            ExpressionAttributeNames={"#L": "Level"},
            ConditionExpression="attribute_exists(Username)"
        )
        return create_response(200, {"message": "Cập nhật thành công"})
    except Exception as e:
        print("Lỗi update account:", e)
        return create_response(500, {"message": "Lỗi hệ thống khi cập nhật"})

# ============================
# 4) Lịch sử đấu (ghi)
# ============================
def handle_record_match(body):
    winner = body.get("WinnerUsername", "").strip()
    loser  = body.get("LoserUsername", "").strip()
    if not winner or not loser:
        return create_response(400, {"message": "Thiếu Winner/Loser"})

    match_id = body.get("Id") or f"match-{uuid.uuid4().hex}"
    match_date = body.get("MatchDate") or datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ")

    item = {
        "Id": match_id,
        "WinnerUsername": winner,
        "LoserUsername": loser,
        "MatchDate": match_date
    }

    try:
        MATCH_TABLE.put_item(Item=item)
        return create_response(200, {"message": "Lưu lịch sử thành công", "item": item})
    except Exception as e:
        print("Lỗi ghi MatchHistory:", e)
        return create_response(500, {"message": "Lỗi hệ thống khi ghi lịch sử"})

# ============================
# 5) Lịch sử đấu (lấy)
# ============================
def handle_get_match_history(username):
    username = (username or "").strip()
    if not username:
        return create_response(400, {"message": "Thiếu username"})

    try:
        collected = []

        try:
            resp_w = MATCH_TABLE.query(
                IndexName="WinnerUsername-index",
                KeyConditionExpression=Key("WinnerUsername").eq(username)
            )
            collected.extend(resp_w.get("Items", []))
        except:
            pass

        try:
            resp_l = MATCH_TABLE.query(
                IndexName="LoserUsername-index",
                KeyConditionExpression=Key("LoserUsername").eq(username)
            )
            collected.extend(resp_l.get("Items", []))
        except:
            pass

        if not collected:
            resp_scan = MATCH_TABLE.scan(
                FilterExpression=Attr("WinnerUsername").eq(username) |
                                 Attr("LoserUsername").eq(username)
            )
            collected.extend(resp_scan.get("Items", []))

        uniq = {item["Id"]: item for item in collected}
        items = list(uniq.values())
        items.sort(key=lambda x: x.get("MatchDate", ""), reverse=True)

        return create_response(200, items)
    except Exception as e:
        print("Lỗi lấy lịch sử:", e)
        return create_response(500, {"message": "Lỗi hệ thống khi lấy lịch sử"})

# ============================
# 6) Quên mật khẩu – gửi mã
# ============================
def handle_request_reset(body):
    username = body.get("Username", "").strip()
    email = body.get("Email", "").strip()
    if not username or not email:
        return create_response(400, {"message": "Thiếu Username hoặc Email"})

    resp = ACCOUNT_TABLE.get_item(Key={"Username": username})
    acc = resp.get("Item")
    if not acc:
        return create_response(404, {"message": "Tài khoản không tồn tại"})

    if acc.get("Email", "").lower() != email.lower():
        return create_response(400, {"message": "Email không khớp với tài khoản"})

    code = f"{random.randint(100000, 999999)}"
    expiry = int(time.time()) + 10 * 60

    try:
        ACCOUNT_TABLE.update_item(
            Key={"Username": username},
            UpdateExpression="SET ResetCode=:c, ResetCodeExpiry=:e",
            ExpressionAttributeValues={":c": code, ":e": expiry}
        )
        send_reset_code_email(email, username, code)
        return create_response(200, {"message": "Đã gửi mã đặt lại mật khẩu"})
    except:
        return create_response(500, {"message": "Lỗi hệ thống khi gửi mã"})

# ============================
# 7) Quên mật khẩu – xác nhận
# ============================
def handle_confirm_reset(body):
    username = body.get("Username", "").strip()
    email = body.get("Email", "").strip()
    code = body.get("Code", "").strip()
    new_password = body.get("NewPassword", "").strip()

    if not username or not email or not code or not new_password:
        return create_response(400, {"message": "Thiếu thông tin"})

    resp = ACCOUNT_TABLE.get_item(Key={"Username": username})
    acc = resp.get("Item")
    if not acc:
        return create_response(404, {"message": "Tài khoản không tồn tại"})

    if acc.get("Email", "").lower() != email.lower():
        return create_response(400, {"message": "Email không khớp với tài khoản"})

    if str(acc.get("ResetCode", "")) != code:
        return create_response(400, {"message": "Mã xác minh không đúng"})

    if int(time.time()) > int(acc.get("ResetCodeExpiry", 0)):
        return create_response(400, {"message": "Mã xác minh đã hết hạn"})

    try:
        ACCOUNT_TABLE.update_item(
            Key={"Username": username},
            UpdateExpression="SET Password=:p REMOVE ResetCode, ResetCodeExpiry",
            ExpressionAttributeValues={":p": new_password}
        )
        return create_response(200, {"message": "Đổi mật khẩu thành công"})
    except:
        return create_response(500, {"message": "Lỗi khi đổi mật khẩu"})

# ============================
# 8) Đổi mật khẩu trực tiếp
# ============================
def handle_change_password(body):
    username = body.get("Username", "").strip()
    new_password = body.get("NewPassword", "").strip()
    if not username or not new_password:
        return create_response(400, {"message": "Thiếu Username hoặc NewPassword"})

    resp = ACCOUNT_TABLE.get_item(Key={"Username": username})
    if "Item" not in resp:
        return create_response(404, {"message": "Tài khoản không tồn tại"})

    try:
        ACCOUNT_TABLE.update_item(
            Key={"Username": username},
            UpdateExpression="SET Password=:p",
            ExpressionAttributeValues={":p": new_password}
        )
        return create_response(200, {"message": "Đổi mật khẩu thành công"})
    except:
        return create_response(500, {"message": "Lỗi khi đổi mật khẩu"})

# ============================
# 9) Cập nhật trạng thái Online / Offline
# ============================
def handle_set_online_status(body):
    username = body.get("Username", "").strip()
    online = bool(body.get("Online", False))

    if not username:
        return create_response(400, {"message": "Thiếu Username"})

    try:
        ACCOUNT_TABLE.update_item(
            Key={"Username": username},
            UpdateExpression="SET #O = :o",
            ExpressionAttributeValues={":o": online},
            ExpressionAttributeNames={"#O": "Online"},
            ConditionExpression="attribute_exists(Username)"
        )
        return create_response(200, {"message": "Đã cập nhật trạng thái", "Online": online})
    except Exception as e:
        print("Lỗi update Online:", e)
        return create_response(500, {"message": "Lỗi hệ thống khi cập nhật Online"})

# ============================
# Entry point
# ============================
def lambda_handler(event, context):
    http = (event.get("requestContext") or {}).get("http", {}) or {}
    method = http.get("method", "")
    raw_path = http.get("path", "")

    body = {}
    if method in ["POST", "PUT"] and event.get("body"):
        try:
            body = json.loads(event["body"])
        except:
            return create_response(400, {"message": "Invalid JSON body"})

    if method == "POST":
        if "/account/register" in raw_path:
            return handle_register(body)
        if "/account/login" in raw_path:
            return handle_login(body)
        if "/account/change-password" in raw_path:
            return handle_change_password(body)
        if "/account/request-reset" in raw_path:
            return handle_request_reset(body)
        if "/account/confirm-reset" in raw_path:
            return handle_confirm_reset(body)
        if "/account/set-status" in raw_path:
            return handle_set_online_status(body)
        if "/matchhistory/add" in raw_path:
            return handle_record_match(body)

    if method == "PUT":
        if "/account/update" in raw_path:
            return handle_update_account(body)

    if method == "GET":
        params = event.get("pathParameters") or {}
        raw_username = params.get("username")

        if "/account/" in raw_path and raw_username:
            resp = ACCOUNT_TABLE.get_item(Key={'Username': raw_username})
            acc = resp.get("Item")
            if not acc:
                return create_response(404, {"message": "Tài khoản không tồn tại"})
            acc.pop("Password", None)
            acc.setdefault("Online", False)
            return create_response(200, acc)

        if "/matchhistory/" in raw_path and raw_username:
            return handle_get_match_history(raw_username)

    return create_response(404, {"message": f"Endpoint not found: {raw_path}"})
