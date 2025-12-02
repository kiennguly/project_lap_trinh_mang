import json
import os
import boto3
from datetime import datetime

TABLE_NAME = os.getenv("ROOM_TABLE", "sololan")

dynamodb = boto3.resource("dynamodb")
table = dynamodb.Table(TABLE_NAME)


def resp(code, body):
    return {
        "statusCode": code,
        "headers": {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*",
        },
        "body": json.dumps(body),
    }


def lambda_handler(event, context):
    # HTTP API v2
    http = event.get("requestContext", {}).get("http", {})
    method = http.get("method", "GET").upper()

    # Ta dùng POST cho tất cả action
    if method != "POST":
        return resp(405, {"error": "Only POST allowed"})

    try:
        body = json.loads(event.get("body") or "{}")
    except Exception:
        return resp(400, {"error": "invalid json"})

    action = body.get("action")
    room_id = str(body.get("roomId") or "").strip()

    # ===== LIST PHÒNG: không cần roomId =====
    if action == "list":
        return handle_list()

    if not action or not room_id:
        return resp(400, {"error": "action & roomId required"})

    # ========== Host tạo phòng ==========
    if action == "create":
        host = (body.get("host") or "").strip()

        table.put_item(
            Item={
                "RoomID": room_id,
                "Host": host,
                "Guest": "",
                "Status": "CREATED",
                "CreatedAt": datetime.utcnow().isoformat(),
            }
        )
        return resp(200, {"ok": True, "msg": "room created"})

    # ========== Client tham gia ==========
    if action == "join":
        guest = (body.get("guest") or "").strip()

        table.update_item(
            Key={"RoomID": room_id},
            UpdateExpression="SET Guest = :g, #st = :s",
            ExpressionAttributeNames={"#st": "Status"},
            ExpressionAttributeValues={
                ":g": guest,
                ":s": "READY",   # phòng đủ 2 người nhưng chưa start
            },
        )
        return resp(200, {"ok": True, "msg": "guest joined"})

    # ========== Host bấm BẮT ĐẦU ==========
    if action == "start":
        table.update_item(
            Key={"RoomID": room_id},
            UpdateExpression="SET #st = :s, StartedAt = :t",
            ExpressionAttributeNames={"#st": "Status"},
            ExpressionAttributeValues={
                ":s": "STARTED",
                ":t": datetime.utcnow().isoformat(),
            },
        )
        return resp(200, {"ok": True, "msg": "room started"})

    # ========== KHI CLIENT B OUT -> QUAY VỀ CREATED ==========
    if action == "back_to_created":
        # Guest rời phòng, ta reset Guest = "" và Status = CREATED
        table.update_item(
            Key={"RoomID": room_id},
            UpdateExpression="SET Guest = :g, #st = :s",
            ExpressionAttributeNames={
                "#st": "Status"
            },
            ExpressionAttributeValues={
                ":g": "",
                ":s": "CREATED",
            },
        )
        return resp(200, {"ok": True, "msg": "room back to CREATED"})

    # ========== HỦY PHÒNG (HOST THOÁT TRƯỚC KHI START) ==========
    if action == "cancel":
        table.update_item(
            Key={"RoomID": room_id},
            UpdateExpression="SET #st = :s, EndedAt = :t",
            ExpressionAttributeNames={"#st": "Status"},
            ExpressionAttributeValues={
                ":s": "CANCELLED",
                ":t": datetime.utcnow().isoformat(),
            },
        )
        return resp(200, {"ok": True, "msg": "room cancelled"})

    # ========== KẾT THÚC PHÒNG (END) ==========
    if action == "end":
        table.update_item(
            Key={"RoomID": room_id},
            UpdateExpression="SET #st = :s, EndedAt = :t",
            ExpressionAttributeNames={"#st": "Status"},
            ExpressionAttributeValues={
                ":s": "END",
                ":t": datetime.utcnow().isoformat(),
            },
        )
        return resp(200, {"ok": True, "msg": "room ended"})

    return resp(400, {"error": "unknown action"})


def handle_list():
    """
    Trả về danh sách phòng CREATED:
    {
      "ok": true,
      "rooms": [
        {"roomId":"123456","host":"nhat","playerCount":1,"status":"CREATED"},
        ...
      ]
    }
    """

    resp_scan = table.scan()
    items = resp_scan.get("Items", [])

    rooms = []
    for it in items:
        status = it.get("Status", "")

        # CHỈ lấy phòng CREATED
        if status != "CREATED":
            continue

        room_id = it.get("RoomID", "")
        host = it.get("Host", "")
        guest = (it.get("Guest") or "").strip()

        if host and guest:
            player_count = 2
        elif host:
            player_count = 1
        else:
            player_count = 0

        rooms.append(
            {
                "roomId": room_id,
                "host": host,
                "playerCount": player_count,
                "status": status,
            }
        )

    return resp(200, {"ok": True, "rooms": rooms})