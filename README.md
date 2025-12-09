# 🎮 GAME BẮN MÁY BAY – PLANE FIGHTING  
## 📌 ĐỒ ÁN MÔN HỌC NT106.Q11 – LẬP TRÌNH MẠNG CĂN BẢN

### 👥 Thành viên nhóm
1. Nguyễn Minh Nhật  
2. Nguyễn Trung Kiên  
3. Nguyễn Ngọc Minh  

---

## 📝 GIỚI THIỆU

**Plane Fighting** là game bắn máy bay góc nhìn từ trên xuống với lối chơi nhanh, nhiều hiệu ứng và bảng xếp hạng cạnh tranh.  
Người chơi có thể:
- Chơi **offline** để luyện kỹ năng và đua top trên **bảng xếp hạng điểm số**.
- Chơi **online (solo 1v1)** để so tài cùng bạn bè qua mạng.
- **Trò chuyện (chat)** trực tiếp với đối thủ ngay trong lúc trận đấu đang diễn ra.

Ngay bây giờ, hãy:
1. Tải game  
2. Tạo tài khoản  
3. Ghi tên mình vào **bảng danh vọng** của Plane Fighting!  

---

## ⭐ TÍNH NĂNG NỔI BẬT

- 🎯 **Lối chơi đơn giản, dễ làm quen**, nhưng khó để master.  
- 💥 **Đồ họa bắt mắt**, hiệu ứng bắn – nổ – va chạm “giả chân thật” nhưng đầy vui nhộn.  
- 🏆 **Bảng xếp hạng (Leaderboard)** theo điểm số, lưu lại thành tích cho từng tài khoản.  
- 🌐 **Chế độ SOLO ONLINE**:
  - Kết nối 2 người chơi trong cùng một phòng.
  - Đồng bộ trạng thái máy bay, đạn, máu theo thời gian gần thực.
- 💬 **Chat trực tiếp trong phòng chơi**:
  - Gửi tin nhắn trước, trong hoặc sau trận.
  - Tạo cảm giác “so găng” đúng chất chiến trường.
- 👤 **Hệ thống tài khoản**:
  - Đăng ký / đăng nhập.
  - Lưu lịch sử trận đấu.
  - Tùy chỉnh avatar.

---

## ☁️ KIẾN TRÚC HỆ THỐNG TRÊN AWS

Đồ án áp dụng mô hình **ứng dụng game + backend trên nền tảng AWS Cloud**, tập trung vào tính **mở rộng, ít quản lý hạ tầng và chi phí thấp**.

### 1. Client – Game trên Windows (WinForms C#)
- Game được viết bằng **C# WinForms**.
- Gửi/nhận dữ liệu qua **REST API** và **WebSocket**.
- Xử lý logic hiển thị, hiệu ứng, điều khiển máy bay, va chạm…

### 2. Backend API – Serverless trên AWS
- Sử dụng **Amazon API Gateway (HTTP API)** làm lớp **entry point** cho client.
- Các API chính:
  - Đăng ký / đăng nhập tài khoản.
  - Ghi nhận kết quả trận đấu, cập nhật bảng xếp hạng.
  - Lấy danh sách top người chơi.
- Business logic được triển khai bằng **AWS Lambda**:
  - Không cần quản lý server (serverless).
  - Tự động scale theo lượng request.
  - Phù hợp với workload không quá lớn nhưng biến động.

### 3. Realtime & Chat – API Gateway WebSocket + Lambda
- **API Gateway (WebSocket)** dùng để:
  - Duy trì kết nối 2 chiều với client.
  - Gửi – nhận message realtime: vị trí máy bay, trạng thái game, tin nhắn chat…
- **Lambda WebSocket handler**:
  - Xử lý sự kiện khi client connect / disconnect.
  - Gửi tin nhắn tới đúng phòng / đúng người chơi.
  - Lưu thông tin kết nối vào database để định tuyến.

### 4. Lưu trữ dữ liệu – Amazon DynamoDB
- Dùng **Amazon DynamoDB** để lưu:
  - Bảng **AccountData**: thông tin tài khoản, mật khẩu đã băm, avatar…
  - Bảng **MatchHistory**: lịch sử trận đấu, thời gian, kết quả, điểm số…
  - Bảng **Ranking** hoặc tính toán từ MatchHistory để lấy top.
- Lý do chọn DynamoDB:
  - **Managed NoSQL**, không cần quản lý server, backup, scaling.
  - Độ trễ thấp, phù hợp đọc/ghi nhanh cho game.

### 5. Lưu trữ avatar & nội dung tĩnh – Amazon S3 (+ CloudFront nếu có)
- **Amazon S3**:
  - Lưu avatar người chơi (bucket `avatargame`, folder `avatars/`).
  - Có thể lưu thêm ảnh plane, tài liệu, file cấu hình…
- Có thể kết hợp **Amazon CloudFront** để:
  - Cache file tĩnh gần người dùng.
  - Tăng tốc tải ảnh / asset nếu triển khai cho nhiều khu vực.

### 6. Bảo mật & Quản lý truy cập – AWS IAM
- **IAM Role cho Lambda**:
  - Chỉ cho phép Lambda truy cập đúng các bảng DynamoDB, đúng bucket S3 cần thiết.
- **API key / JWT / token** (tùy cách triển khai trong đồ án):
  - Bảo vệ API, tránh gọi trái phép từ bên ngoài.

### 7. Giám sát – AWS CloudWatch (tùy chọn)
- Dùng **Amazon CloudWatch** để:
  - Ghi log request từ Lambda.
  - Theo dõi lỗi, độ trễ, số lượng request.

---

## 📸 HÌNH ẢNH MINH HỌA

<img width="768" height="512" alt="Plane Fighting Screenshot" src="https://github.com/user-attachments/assets/4573ba95-814d-49ce-b407-e1716e1f11b5" />

---

> **Tóm lại**, Plane Fighting không chỉ là một game bắn máy bay giải trí, mà còn là đồ án áp dụng các dịch vụ **AWS Cloud** vào thực tế: từ **serverless backend**, **realtime WebSocket**, đến **NoSQL database và object storage**
