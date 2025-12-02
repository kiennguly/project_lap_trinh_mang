import json
import boto3
from decimal import Decimal

dynamodb = boto3.resource("dynamodb")

ACCOUNT_TABLE = "AccountData"
GIFTCODE_TABLE = "giftcode"

account_table = dynamodb.Table(ACCOUNT_TABLE)
giftcode_table = dynamodb.Table(GIFTCODE_TABLE)

# Định nghĩa phần thưởng cho từng giftcode
GIFT_REWARDS = {
    "NT106":  {"gold": 500,  "damage": 10},
    "VIP666": {"gold": 1000, "damage": 20},
    "10DIEMNT106": {"gold": 1000, "damage": 20},
    "MINHHOI": {"gold": 1000, "damage": 20},
    "KIENDEPTRAI": {"gold": 1000, "damage": 20},
    "NHATDEPTRAI": {"gold": 1000, "damage": 20},
}

def lambda_handler(event, context):
    try:
        body = json.loads(event.get("body", "{}"))
        username = body.get("username", "").strip()
        code = body.get("code", "").strip().upper()

        if not username or not code:
            return _resp(400, {"ok": False, "message": "Thiếu username hoặc code"})

        # 1. Kiểm tra code hợp lệ
        if code not in GIFT_REWARDS:
            return _resp(200, {"ok": False, "message": "Giftcode không hợp lệ"})

        # 2. Lấy tài khoản
        acc_res = account_table.get_item(Key={"Username": username})
        acc_item = acc_res.get("Item")
        if not acc_item:
            return _resp(200, {"ok": False, "message": "Tài khoản không tồn tại"})

        # 3. Lấy item giftcode theo Namecode
        gift_res = giftcode_table.get_item(Key={"Namecode": code})
        gift_item = gift_res.get("Item")

        if not gift_item:
            # Nếu chưa có hàng cho code này thì tạo mới
            gift_item = {"Namecode": code, "Username": ""}
            giftcode_table.put_item(Item=gift_item)

        # Username ở đây là 1 chuỗi kiểu: [kien][nhat]
        used_str = gift_item.get("Username", "") or ""

        pattern = f"[{username}]"

        # Nếu user đã dùng code này rồi
        if pattern in used_str:
            return _resp(200, {
                "ok": False,
                "message": "Bạn đã sử dụng giftcode này rồi"
            })

        # 4. Thêm username vào chuỗi
        new_used_str = used_str + pattern

        giftcode_table.update_item(
            Key={"Namecode": code},
            UpdateExpression="SET Username = :val",
            ExpressionAttributeValues={":val": new_used_str}
        )

        # 5. Cộng thưởng vào AccountData
        reward = GIFT_REWARDS[code]
        gold_add = reward["gold"]
        dmg_add = reward["damage"]

        account_table.update_item(
            Key={"Username": username},
            UpdateExpression="""
                SET Gold = if_not_exists(Gold, :zero) + :g,
                    UpgradeDamage = if_not_exists(UpgradeDamage, :zero) + :d
            """,
            ExpressionAttributeValues={
                ":zero": Decimal(0),
                ":g": Decimal(gold_add),
                ":d": Decimal(dmg_add),
            },
        )

        old_gold = int(acc_item.get("Gold", 0))
        new_gold = old_gold + gold_add

        return _resp(200, {
            "ok": True,
            "message": "Đổi giftcode thành công",
            "goldAdded": gold_add,
            "damageAdded": dmg_add,
            "newGold": new_gold
        })

    except Exception as e:
        return _resp(500, {
            "ok": False,
            "message": f"Lỗi server: {str(e)}"
        })


def _resp(status, body_dict):
    return {
        "statusCode": status,
        "headers": {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*",
        },
        "body": json.dumps(body_dict),
    }
