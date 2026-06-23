# zfox-modvps / tool_dragonball_2.0

Repo này tập trung phát triển hệ sinh thái auto/mod cho NRO với kiến trúc tách lớp rõ ràng giữa Panel điều khiển, Client Console thực thi logic và Server mô phỏng backend.

## Mục tiêu

- Quản lý và điều phối nhiều account qua `Panel`.
- Thực thi auto feature theo frame update trong `NRO247Native`.
- Duy trì giao thức lệnh ổn định qua socket `accountId|COMMAND|params...`.
- Bảo toàn cơ chế hot-update cấu hình, không restart feature.

## Thành phần chính

- `Panel`: UI, cấu hình, DB, gửi command xuống client.
- `NRO247Native`: engine thực thi auto feature (`AutoMod` dispatcher).
- `Server`: backend custom emulation phục vụ test/phát triển.
- `mod_java`: nguồn tham khảo thuật toán/logic từ bản Java.
- `client_unity`: nguồn tham khảo hành vi client gốc.
- `docs`: tài liệu thiết kế và kế hoạch triển khai.

## Tài liệu bắt buộc đọc

- Kiến trúc + quy chuẩn codebase: [AGENTS.md](./AGENTS.md)
- Quy trình đóng góp và PR: [CONTRIBUTING.md](./CONTRIBUTING.md)

## Luồng kỹ thuật cốt lõi

- Hook chuẩn: `ModBootstrap -> ModManager -> AutoMod`.
- Mỗi feature có `Update()` chạy mỗi frame, không blocking.
- Panel thay đổi setting phải được apply tại tick kế tiếp (hot-update).
- `PanelSocket.HandleMessage()` chỉ decode/invoke, không xử lý game loop phức tạp.

## Bắt đầu nhanh cho contributor mới

1. Đọc `AGENTS.md` để nắm rule kiến trúc.
2. Xác định phạm vi thay đổi thuộc `Panel` hay `NRO247Native` hay `Server`.
3. Nếu có command mới, cập nhật cả 2 đầu: gửi từ Panel và decode ở Client.
4. Test hot-update: thay đổi setting liên tục khi auto đang chạy.
5. Mở PR theo template trong `.github/pull_request_template.md`.

## Lưu ý

- Ưu tiên giao tiếp/document tiếng Việt, nhưng định danh code (class/method/variable/file) dùng tiếng Anh rõ nghĩa.
- Không nhúng UI vào client console feature.
- Không nhúng network parsing vào feature logic.
