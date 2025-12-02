import json
import base64
import boto3
import os

s3 = boto3.client("s3")
BUCKET = os.getenv("BUCKET_NAME", "avatargame")

def resp(status, body):
    return {
        "statusCode": status,
        "headers": {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*"   # cho WinForms gọi thoải mái
        },
        "body": json.dumps(body)
    }

def handle_upload(body):
    file_name = body.get("fileName")
    image_base64 = body.get("imageBase64")
    content_type = body.get("contentType", "image/png")

    if not file_name or not image_base64:
        return resp(400, {"error": "Missing fileName or imageBase64"})

    try:
        image_bytes = base64.b64decode(image_base64)
    except Exception:
        return resp(400, {"error": "Invalid base64"})

    # bạn có thể đổi "avatars/" thành thư mục khác
    key = f"avatars/{file_name}"

    s3.put_object(
        Bucket=BUCKET,
        Key=key,
        Body=image_bytes,
        ContentType=content_type
    )

    return resp(200, {"message": "Uploaded", "key": key})

def handle_get_url(body):
    key = body.get("key")  # vd: "avatars/user1.png"
    if not key:
        return resp(400, {"error": "Missing key"})

    url = s3.generate_presigned_url(
        "get_object",
        Params={"Bucket": BUCKET, "Key": key},
        ExpiresIn=3600  # 1h
    )

    return resp(200, {"downloadUrl": url})

def lambda_handler(event, context):
    try:
        body = json.loads(event.get("body") or "{}")
    except Exception:
        return resp(400, {"error": "Invalid JSON"})

    action = body.get("action")

    if action == "upload":
        return handle_upload(body)
    elif action == "getUrl":
        return handle_get_url(body)
    else:
        return resp(400, {"error": "Unknown action"})
