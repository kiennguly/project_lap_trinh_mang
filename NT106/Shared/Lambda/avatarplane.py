import json
import boto3
import os

s3 = boto3.client("s3")
BUCKET = os.getenv("BUCKET_NAME", "avatarplane-minh")

def resp(status, body):
    return {
        "statusCode": status,
        "headers": {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*"   # cho WinForms / web gọi thoải mái
        },
        "body": json.dumps(body)
    }

def lambda_handler(event, context):
    # HTTP API v2: body là string JSON
    body_str = event.get("body") or "{}"

    try:
        body = json.loads(body_str)
    except Exception:
        return resp(400, {"error": "invalid json"})

    # ví dụ: { "plane": 1 }
    plane_index = body.get("plane", 1)

    # map số → key trong S3
    key_map = {
        1: "planes/plane1.png",
        2: "planes/plane2.png",
        3: "planes/plane3.png",
        4: "planes/plane4.png",
        5: "planes/plane5.png",
    }

    key = key_map.get(int(plane_index), "planes/plane1.png")

    try:
        url = s3.generate_presigned_url(
            ClientMethod="get_object",
            Params={"Bucket": BUCKET, "Key": key},
            ExpiresIn=300  # 5 phút
        )

        return resp(200, {
            "planeIndex": plane_index,
            "key": key,
            "downloadUrl": url
        })
    except Exception as e:
        return resp(500, {"error": str(e)})
