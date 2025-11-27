import json
import os
import base64
import boto3

# ===== KẾT NỐI DYNAMODB =====
TABLE_NAME = os.environ.get("TABLE_NAME", "BangXepHang")

dynamodb = boto3.resource("dynamodb")
table = dynamodb.Table(TABLE_NAME)


def make_response(status_code, body_obj):
    """Tạo response trả về HTTP API."""
    return {
        "statusCode": status_code,
        "headers": {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Headers": "*",
            "Access-Control-Allow-Methods": "GET,POST,OPTIONS",
        },
        "body": json.dumps(body_obj, ensure_ascii=False),
    }


# ========== HÀM LÕI ==========

def save_level(username: str, level: int) -> int:
    """
    Lưu level cao nhất của 1 người chơi.
    Nếu level mới <= level cũ thì KHÔNG ghi đè.
    Trả về level hiện tại sau khi xử lý.
    """
    resp = table.get_item(Key={"Username": username})
    item = resp.get("Item")

    if item:
        current_level = int(item.get("Level", 0))
        if level <= current_level:
            # đã có kỷ lục cao hơn rồi -> giữ nguyên
            return current_level

    # chưa có hoặc level mới cao hơn -> ghi lại
    table.put_item(Item={"Username": username, "Level": level})
    return level


def get_ranking(limit: int = 10):
    """
    Đọc toàn bộ bảng, sắp xếp và gán Rank trong RAM.
    Trả về list [{Username, Level, Rank}, ...] đã giới hạn theo limit.
    """
    resp = table.scan()
    items = resp.get("Items", [])

    # Chuẩn hoá dữ liệu
    players = []
    for item in items:
        username = item.get("Username", "")
        level = int(item.get("Level", 0))
        players.append({"Username": username, "Level": level})

    # Sắp xếp: level giảm dần, cùng level thì theo Username
    players.sort(key=lambda p: (-p["Level"], p["Username"]))

    # Gán Rank (dense ranking)
    ranking = []
    current_rank = 0
    previous_level = None

    for p in players:
        if previous_level is None or p["Level"] < previous_level:
            current_rank += 1
        previous_level = p["Level"]

        ranking.append(
            {
                "Username": p["Username"],
                "Level": p["Level"],
                "Rank": current_rank,
            }
        )

        if len(ranking) >= limit:
            break

    return ranking


def persist_rank(ranking, max_write: int = 100):
    """
    Ghi Rank vào DynamoDB để xem được trong console.
    Chỉ ghi tối đa max_write user (ví dụ: TOP 10).
    LƯU Ý: put_item sẽ overwrite toàn bộ item, nên item của bạn
    chỉ nên có các field Username, Level, Rank.
    """
    with table.batch_writer(overwrite_by_pkeys=["Username"]) as batch:
        for i, row in enumerate(ranking):
            if i >= max_write:
                break

            batch.put_item(
                Item={
                    "Username": row["Username"],
                    "Level": row["Level"],
                    "Rank": row["Rank"],
                }
            )


# ========== HÀM CHÍNH LAMBDA ==========

def lambda_handler(event, context):
    """
    Lambda cho HTTP API (payload v2):
    - POST /post : lưu level cao nhất + trả TOP 10 (và ghi Rank vào bảng)
    - GET  /get  : trả bảng xếp hạng theo limit
    """
    http_info = event.get("requestContext", {}).get("http", {})
    method = (http_info.get("method") or "").upper()

    raw_path = (event.get("rawPath") or http_info.get("path") or "").lower().rstrip("/")

    # 0) CORS preflight
    if method == "OPTIONS":
        return make_response(200, {"ok": True})

    # 1) POST /post  -> client gửi level lên
    if method == "POST" and raw_path.endswith("/post"):
        # Xử lý body (có thể là base64)
        raw_body = event.get("body") or ""
        if event.get("isBase64Encoded"):
            raw_body = base64.b64decode(raw_body).decode("utf-8", errors="ignore")

        try:
            data = json.loads(raw_body or "{}")
        except json.JSONDecodeError:
            return make_response(400, {"error": "Body không phải JSON"})

        # Hỗ trợ cả "username" lẫn "Username"
        username = str(
            data.get("username") or data.get("Username") or ""
        ).strip()

        # Hỗ trợ cả "level" lẫn "Level"
        level = data.get("level")
        if level is None:
            level = data.get("Level")

        # Kiểm tra dữ liệu đầu vào
        if not username or not isinstance(level, int) or level < 0:
            return make_response(
                400,
                {"error": "username phải khác rỗng và level là số nguyên >= 0"},
            )

        # Lưu level
        current_level = save_level(username, level)

        # Tính lại TOP 10 + Rank
        ranking = get_ranking(limit=10)

        # Ghi Rank vào bảng để xem được trong DynamoDB console
        persist_rank(ranking, max_write=10)

        return make_response(
            200,
            {
                "ok": True,
                "message": "Đã lưu level (chỉ cập nhật nếu cao hơn kỷ lục cũ)",
                "currentLevel": current_level,
                "ranking": ranking,
            },
        )

    # 2) GET /get  -> lấy bảng xếp hạng
    if method == "GET" and raw_path.endswith("/get"):
        qs = event.get("queryStringParameters") or {}
        limit = 10
        if "limit" in qs:
            try:
                n = int(qs["limit"])
                if 0 < n <= 100:
                    limit = n
            except ValueError:
                pass

        ranking = get_ranking(limit)
        

        return make_response(200, {"ok": True, "ranking": ranking})

    # 3) Không khớp route nào
    return make_response(404, {"error": "Không tìm thấy đường dẫn (path)"})
