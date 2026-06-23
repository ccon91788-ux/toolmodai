# AGENTS.md — Architect Mode

This file provides guidance to agents when working with code in this repository.

## Kiến trúc Xmap (chuyển map tự động)

### Luồng chính
```
AutoXmapFeature.Update()
  └─ XmapNavigator.FlushPendingMapChange()   ← flush charMove→requestChangeMap (200ms delay)
  └─ _wasChangingMap edge detection          ← đếm fail/success, đặt postMapLoadDelay
  └─ UpdateXmap(targetMapId)
       └─ XmapPathfinder.FindPath()          ← BFS qua DataXmap.linkMaps
       └─ XmapNavigator.gotoMap(nextHop)
            └─ NextMap.GotoMap()
                 ├─ waypoint path: Enter(waypoint) → charMove + pending requestChangeMap
                 └─ NPC path: teleportToNpc → openMenu → confirmMenu
```

### Ràng buộc kiến trúc không được phá vỡ
- `loadMap()` trong `XmapNavigator` **không được** gọi `requestChangeMap()` trực tiếp → phải dùng `_pendingChangeMap` + `FlushPendingMapChange()`
- `_mapChangeFailCount` chỉ tăng tại **1 điểm**: nhánh `_wasChangingMap = true → false` trong `Update()`
- `NextMap` object là stateful (giữ `_isTeleNpc`, `_isEntering`, v.v.) — **không share** giữa nhiều luồng

### Coupling ẩn giữa các file
- `AutoXmapFeature` phụ thuộc `XmapNavigator.FlushPendingMapChange()` — nếu bỏ call này thì `requestChangeMap` không bao giờ được gửi
- `NextMap.Enter()` dùng `_hasTeleported` cho **2 mục đích khác nhau** (teleport đã xong + delay trước changeMap) — đừng tách ra
- `DataXmap.linkMaps` được load 1 lần lúc static init — nếu cần reload phải gọi `DataXmap.LoadData()`
