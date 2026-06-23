# AGENTS.md — Ask Mode

This file provides guidance to agents when working with code in this repository.

## Tổ chức code không rõ ràng từ tên file

- `NRO247Native/` chứa **toàn bộ client game** (C#, port từ Java), không phải native lib
- `AutoXmapFeature.cs` nằm ở `Mods/` (không phải `Mods/Xmap/`) — đây là controller chính, còn logic con nằm trong `Mods/Xmap/`
- `NextMap.cs` không chỉ là data class — nó chứa toàn bộ state machine cho 1 hop di chuyển
- `CompatUtils.cs` tên gây hiểu nhầm — thực ra là `PathUtils` + `GameUtils`, không liên quan đến compatibility

## Thuật ngữ domain-specific

- **"hop"**: 1 bước chuyển map (từ map A sang map liền kề B)
- **"waypoint"**: cổng/điểm dịch chuyển trên map (object `Waypoint` trong `TileMap.vGo`)
- **"xmap"**: tính năng tự động đi qua nhiều map liên tiếp để đến đích
- **"khu" (zone)**: server shard của 1 map — `requestChangeZone()` đổi sang shard khác cùng map
- **`ischangingMap`**: flag trên `Char` class, server set khi đang xử lý map transition

## Cấu trúc Socket Panel → Client

- Format: `accountId|COMMAND|param1|param2|...`
- Parse ở: `NRO247Native/NRO_v247/SocketGame.cs`
- Replay sau login ở: tìm `SendAllSavedSettingsToClient` trong `Panel/`
