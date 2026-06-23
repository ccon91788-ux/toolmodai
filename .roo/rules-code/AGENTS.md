# AGENTS.md — Code Mode

This file provides guidance to agents when working with code in this repository.

## Xmap: Quy tắc bắt buộc khi sửa chuyển map

### Thứ tự packet quan trọng
- `charMove()` → delay tối thiểu **200ms** → `requestChangeMap()` (không gửi đồng thời)
- `XmapNavigator.loadMap()` KHÔNG gọi `requestChangeMap()` trực tiếp nữa — dùng `FlushPendingMapChange()` trong `Update()` của `AutoXmapFeature`
- Pattern đúng: set `_pendingChangeMap = true` → `FlushPendingMapChange()` flush sau 200ms

### Delay chống ping-pong với server
- Sau `TeleportTo()` phải đợi **500ms** trước khi gọi lại (server RTT ~100-300ms, cần buffer)
- Delay khởi động NPC: **150ms** sau `teleportToNpc()` trước khi `openMenu()`
- Timeout NPC không phản hồi: **8 giây** thì `ResetConfirmState()`

### Guard quan trọng
- Trước khi set `_isTeleNpc = true` phải check `GameScr.findNPCInMap(Npc) != null` — NPC có thể chưa load
- `_mapChangeFailCount` chỉ được tăng ở **một chỗ duy nhất** (nhánh `_wasChangingMap` trong `Update()`) — tránh double-count
- Ngưỡng đổi khu: **3 lần fail** (không phải 2) để tránh đổi khu sớm do lag nhất thời

### State machine Enter()
- `_hasTeleported` dùng 2 mục đích trong `Enter()` isWideGate: (1) đánh dấu đã `charMove`, (2) guard 200ms delay trước `requestChangeMap`
- `ResetEnterState()` phải gọi sau khi `requestChangeMap()` thành công, không gọi trước

## Xmap: Cấu trúc file

| File | Vai trò |
|------|---------|
| `AutoXmapFeature.cs` | State machine chính, `Update()` loop, failCount, zone change |
| `NextMap.cs` | Logic từng bước di chuyển/NPC/waypoint cho 1 hop |
| `XmapNavigator.cs` | Teleport đến waypoint, flush pending `requestChangeMap` |
| `XmapPathfinder.cs` | BFS tìm đường ngắn nhất qua `DataXmap.linkMaps` |
| `DataXmap.cs` | Dữ liệu tĩnh: linkMaps, NPCLinkMaps, planet list |

## Pattern chung

- Feature mới = 1 command socket duy nhất, 1 hàm `SendXxxSettingsCommand()`, replay sau `ONLINE`
- Mọi feature phải tự phục hồi trong `Update()` sau login/reconnect (idempotent)
- `_wasChangingMap` flag: detect edge `ischangingMap: true → false` để đo kết quả chuyển map
