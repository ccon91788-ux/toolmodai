# AGENTS.md — Debug Mode

This file provides guidance to agents when working with code in this repository.

## Xmap: Các điểm dễ gây lỗi âm thầm

### Ping-pong position với server
- Triệu chứng: char giật về giữa map, không qua được waypoint
- Nguyên nhân: `requestChangeMap()` gửi quá sớm sau `charMove()` (< 200ms) → server reject, char bị kéo về
- Chỗ kiểm tra: `XmapNavigator.FlushPendingMapChange()` — xem `_charMoveSentTime` có cách request < 200ms không

### `_mapChangeFailCount` tăng gấp đôi
- Triệu chứng: đổi khu liên tục dù chỉ fail 1 lần
- Nguyên nhân: trước đây có 2 chỗ tăng counter (timeout branch + `_wasChangingMap` branch)
- Chỗ kiểm tra: `AutoXmapFeature.Update()` — chỉ được có 1 chỗ tăng `_mapChangeFailCount`

### NPC chưa load nhưng đã set `_isTeleNpc = true`
- Triệu chứng: tele đến NPC nhưng không mở menu, bị stuck
- Nguyên nhân: `teleportToNpc()` gọi khi `findNPCInMap()` trả về null
- Chỗ kiểm tra: `NextMap.GotoMap()` — phải có guard `findNPCInMap(Npc) != null` trước khi set state

### `Char.ischangingMap` không reset
- Triệu chứng: xmap block mãi, không tiến được
- Nguyên nhân: timeout 5 giây trong `AutoXmapFeature.Update()` không reset `_wasChangingMap`
- Fix: sau timeout, giữ `_wasChangingMap = true` để nhánh `else` xử lý ngay tick tiếp theo

## Thứ tự debug khi xmap không chạy

1. Panel có replay command `XMAP_SETTING` sau `ONLINE`?
2. `AutoXmapFeature.Update()` có được gọi (check `_isXmaping`)?
3. `XmapNavigator.gotoMap()` có tìm được `NextMap` đúng?
4. `Enter()` / `GotoMap()` có bị stuck do delay flag?
5. Server có trả về map change hay đang bị reject?
