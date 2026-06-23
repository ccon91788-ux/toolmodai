using System;
using System.Collections.Generic;
using NRO_v247.Mods;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Support
{
    public class AutoReducePowerFeature : IAutoFeature
    {
        private bool _enabled;
        private int _mapId = -1;
        private int _zoneId = -1;
        private int _posX = -1;
        private int _posY = -1;
        private int _provokeMobCount = 1;
        private int _deadReportDelayMs = 1000;
        private bool _autoPunchBlackFlag;
        private bool _useHpPunch;
        private int _punchHpPercent = 10;
        private bool _useTdlt;

        private long _lastDeadReportMs;
        private long _deadStartedAtMs;
        private long _lastZoneRequestMs;
        private bool _wasDead;

        private int _currentMapId = -1;
        private int _currentZoneId = -1;
        private long _mapEnterTimeMs;
        private long _lastAntiAfkJumpMs;
        private int _nextAntiAfkDelayMs = 25000;
        private bool _deadReportStarted;
        private bool _isPostReviveProvoking;
        private bool _didInitialProvokeOnEnable;
        private long _postReviveStartMs;
        private long _lastProvokeAttackMs;
        private long _lastPositionLockMs;
        private int _lastLoggedMapId = -1;
        private int _lastLoggedZoneId = -1;
        private readonly HashSet<int> _provokedMobIds = new HashSet<int>();
        private readonly HashSet<int> _punchedPlayerIds = new HashSet<int>();

        private long _lastContinuousPunchMs;
        private const long ContinuousPunchCooldownMs = 100L;

        private const long DeadHeartbeatMs = 200L;
        private const long ZoneChangeCooldownMs = 2000L;
        private const long PositionLockCooldownMs = 700L;
        private const int PositionLockThreshold = 8;
        private const long PostReviveProvokeTimeoutMs = 6000L;
        private const long PostReviveAttackIntervalMs = 700L;

        public bool IsActive => _enabled;
        public int Priority => 65;
        public bool IsUtilityTask => false;
        public string Name => "AutoReducePower";
        public string CurrentState { get; private set; } = "IDLE";

        public void Initialize()
        {
        }

        public void ApplySettingsFromPanel(bool enabled, int mapId, int zoneId, int posX, int posY, int provokeMobCount = 1, int deadReportDelayMs = 1000, bool autoPunchBlackFlag = false, bool useHpPunch = false, int punchHpPercent = 10, bool useTdlt = false)
        {
            bool wasEnabled = _enabled;
            _enabled = enabled;
            _mapId = mapId;
            _zoneId = zoneId;
            _posX = posX;
            _posY = posY;
            _provokeMobCount = Math.Max(0, provokeMobCount);
            _deadReportDelayMs = Math.Max(0, deadReportDelayMs);
            _autoPunchBlackFlag = autoPunchBlackFlag;
            _useHpPunch = useHpPunch;
            _punchHpPercent = punchHpPercent;
            _useTdlt = useTdlt;

            GameLogger.SendLog(
                "REDUCE_POWER",
                $"Nhận cấu hình: bật={(_enabled ? 1 : 0)}, map={_mapId}, khu={_zoneId}, x={_posX}, y={_posY}, mob/turn={_provokeMobCount}, delay gọi cứu={_deadReportDelayMs}ms, đấm cờ đen={_autoPunchBlackFlag}, giới hạn HP={(_useHpPunch ? _punchHpPercent + "%" : "Không")}, dùng TDLT={(_useTdlt ? "Có" : "Không")}");

            if (!enabled)
            {
                _wasDead = false;
                _deadReportStarted = false;
                _deadStartedAtMs = 0L;
                _didInitialProvokeOnEnable = false;
                _lastLoggedMapId = -1;
                _lastLoggedZoneId = -1;
                ResetPostReviveProvoke();
                GameLogger.SendLog("REDUCE_POWER", "Đã tắt giảm sức mạnh.");
            }
            else if (!wasEnabled)
            {
                _deadReportStarted = false;
                _deadStartedAtMs = 0L;
                _didInitialProvokeOnEnable = false;
                _lastLoggedMapId = -1;
                _lastLoggedZoneId = -1;
                GameLogger.SendLog("REDUCE_POWER", "Đã bật giảm sức mạnh.");
            }
        }

        public bool ShouldOverrideOnDeath()
        {
            if (!_enabled) return false;
            Char me = Char.myCharz();
            if (me == null) return false;
            return me.meDead || me.statusMe == 14 || me.isDie || me.cHP <= 0;
        }

        public void Update()
        {
            if (!_enabled)
            {
                CurrentState = "Đang tắt";
                return;
            }

            Char charMe = Char.myCharz();
            if (charMe == null) return;

            long now = mSystem.currentTimeMillis();

            TdltController.Update(_useTdlt);

            if (_currentMapId != TileMap.mapID || _currentZoneId != TileMap.zoneID)
            {
                _currentMapId = TileMap.mapID;
                _currentZoneId = TileMap.zoneID;
                _mapEnterTimeMs = now;
            }

            if (now - _lastAntiAfkJumpMs > _nextAntiAfkDelayMs)
            {
                _lastAntiAfkJumpMs = now;
                _nextAntiAfkDelayMs = new Random().Next(25000, 30000);
                if (!charMe.meDead && !charMe.isDie && charMe.cHP > 0 && charMe.statusMe != 14 && charMe.statusMe != 10)
                {
                    try { GameScr.gI().setCharJump(0); } catch { }
                }
            }

            bool isDead = IsExhaustedState(charMe);
            if (isDead)
            {
                CurrentState = "Đã chết chờ cứu";
                ResetPostReviveProvoke();

                if (!_wasDead)
                {
                    _deadStartedAtMs = now;
                    _deadReportStarted = false;
                    _lastDeadReportMs = 0L;
                    GameLogger.SendLog(
                        "REDUCE_POWER",
                        $"HP=0, bắt đầu chờ {_deadReportDelayMs}ms trước khi gọi cứu tại map {TileMap.mapID} khu {TileMap.zoneID} ({charMe.cx},{charMe.cy}).");
                }

                if (!_deadReportStarted)
                {
                    long elapsedMs = now - _deadStartedAtMs;
                    if (elapsedMs < _deadReportDelayMs)
                    {
                        long remainMs = _deadReportDelayMs - elapsedMs;
                        CurrentState = $"Đã chết, chờ gọi cứu {remainMs}ms";
                        _wasDead = true;
                        return;
                    }

                    _deadReportStarted = true;
                    _lastDeadReportMs = now;
                    string firstDeadCmd = $"REDUCE_POWER_DEAD|{AutoLogin.idClientSocket}|{TileMap.mapID}|{TileMap.zoneID}|{charMe.cx}|{charMe.cy}";
                    SocketGame.SendMessage(firstDeadCmd);
                    GameLogger.SendLog(
                        "REDUCE_POWER",
                        $"Đã gửi yêu cầu cứu đầu tiên tại map {TileMap.mapID} khu {TileMap.zoneID} ({charMe.cx},{charMe.cy}).");
                }
                else if (now - _lastDeadReportMs >= DeadHeartbeatMs)
                {
                    _lastDeadReportMs = now;
                    string heartbeatCmd = $"REDUCE_POWER_DEAD|{AutoLogin.idClientSocket}|{TileMap.mapID}|{TileMap.zoneID}|{charMe.cx}|{charMe.cy}";
                    SocketGame.SendMessage(heartbeatCmd);
                    GameLogger.SendLog(
                        "REDUCE_POWER",
                        $"Heartbeat chết gửi lại tại map {TileMap.mapID} khu {TileMap.zoneID} ({charMe.cx},{charMe.cy}).");
                }

                _wasDead = true;
                return;
            }

            if (_wasDead)
            {
                SocketGame.SendMessage($"REDUCE_POWER_ALIVE|{AutoLogin.idClientSocket}|{TileMap.mapID}|{TileMap.zoneID}|{charMe.cx}|{charMe.cy}");
                GameLogger.SendLog(
                    "REDUCE_POWER",
                    $"Đã sống lại, báo ALIVE tại map {TileMap.mapID} khu {TileMap.zoneID} ({charMe.cx},{charMe.cy}).");
                _deadReportStarted = false;
                _deadStartedAtMs = 0L;
                BeginPostReviveProvoke(now);
            }
            _wasDead = false;

            if (!_didInitialProvokeOnEnable)
            {
                BeginPostReviveProvoke(now);
                _didInitialProvokeOnEnable = true;
            }

            if (_isPostReviveProvoking)
            {
                if (HandlePostReviveProvoke(charMe, now))
                {
                    // Vẫn xử lý các logic khác để đấm cờ đen có thể chạy song song.
                }
            }

            if (_autoPunchBlackFlag && now - _lastContinuousPunchMs >= ContinuousPunchCooldownMs)
            {
                int hpPercent = (int)(charMe.cHP * 100L / Math.Max(1, charMe.cHPFull));
                if (!_useHpPunch || hpPercent > _punchHpPercent)
                {
                    Char blackFlagTarget = FindNearestBlackFlagPlayer(charMe);
                    if (blackFlagTarget != null)
                    {
                        TrySelectBasicPunchSkill(charMe);
                        MyVector vChar = new MyVector();
                        vChar.addElement(blackFlagTarget);
                        Service.gI().sendPlayerAttack(new MyVector(), vChar, 2);
                        _lastContinuousPunchMs = now;

                        if (GameCanvas.gameTick % 20 == 0)
                        {
                            GameLogger.SendLog("REDUCE_POWER", $"Đang đấm siêu tốc cờ đen: {blackFlagTarget.cName} (HP {hpPercent}%)");
                        }
                    }
                }
            }

            CurrentState = "Đang vào vị trí chết";

            if (_mapId >= 0 && TileMap.mapID != _mapId)
            {
                CurrentState = $"Đang di chuyển đến map {_mapId} khu {_zoneId}";
                if (_lastLoggedMapId != TileMap.mapID || _lastLoggedZoneId != TileMap.zoneID)
                {
                    _lastLoggedMapId = TileMap.mapID;
                    _lastLoggedZoneId = TileMap.zoneID;
                    GameLogger.SendLog("REDUCE_POWER", $"Sai map hiện tại {TileMap.mapID}/{TileMap.zoneID}, gọi XMap đến {_mapId}/{_zoneId}.");
                }

                IXmapService xmap = ServiceLocator.Get<IXmapService>();
                if (xmap != null && !xmap.IsXmaping())
                {
                    xmap.StartGoToMapFromPanel(_mapId);
                }
                return;
            }

            if (_zoneId >= 0 && TileMap.mapID == _mapId && TileMap.zoneID != _zoneId)
            {
                CurrentState = $"Đang đổi khu {_zoneId}";
                if (now - _lastZoneRequestMs >= ZoneChangeCooldownMs)
                {
                    Service.gI().requestChangeZone(_zoneId, -1);
                    _lastZoneRequestMs = now;
                    GameLogger.SendLog("REDUCE_POWER", $"Đã gửi đổi khu sang {_zoneId} (map {_mapId}).");
                }
                return;
            }

            if (_posX >= 0 && _posY >= 0)
            {
                int distToSetup = Res.distance(charMe.cx, charMe.cy, _posX, _posY);
                if (distToSetup > PositionLockThreshold && now - _lastPositionLockMs >= PositionLockCooldownMs)
                {
                    _lastPositionLockMs = now;
                    int beforeX = charMe.cx;
                    int beforeY = charMe.cy;
                    charMe.currentMovePoint = null;
                    charMe.cxSend = 0;
                    charMe.cySend = 0;
                    charMe.cx = _posX;
                    charMe.cy = _posY;
                    Service.gI().charMove();
                    CurrentState = $"Đang về điểm chết ({_posX},{_posY})";
                    GameLogger.SendLog("REDUCE_POWER", $"Khóa tọa độ chết: kéo về ({_posX},{_posY}), vị trí trước kéo ({beforeX},{beforeY}).");
                    return;
                }
            }
        }

        public void NotifyCharStateChanged(int newState)
        {
        }

        private static bool IsExhaustedState(Char me)
        {
            if (me == null) return false;
            return me.statusMe == 14 || me.meDead || me.isDie;
        }

        private void BeginPostReviveProvoke(long now)
        {
            _isPostReviveProvoking = true;
            _postReviveStartMs = now;
            _lastProvokeAttackMs = 0L;
            _provokedMobIds.Clear();
            _punchedPlayerIds.Clear();
        }

        private void ResetPostReviveProvoke()
        {
            _isPostReviveProvoking = false;
            _postReviveStartMs = 0L;
            _lastProvokeAttackMs = 0L;
            _provokedMobIds.Clear();
            _punchedPlayerIds.Clear();
        }

        private bool HandlePostReviveProvoke(Char me, long now)
        {
            if (me == null)
            {
                ResetPostReviveProvoke();
                return false;
            }

            if (now - _postReviveStartMs >= PostReviveProvokeTimeoutMs)
            {
                ResetPostReviveProvoke();
                return false;
            }

            if (!_autoPunchBlackFlag && _provokedMobIds.Count >= _provokeMobCount)
            {
                ResetPostReviveProvoke();
                return false;
            }

            CurrentState = $"Sau hồi sinh: đấm {_punchedPlayerIds.Count} người, {_provokedMobIds.Count}/{_provokeMobCount} mob";

            if (now - _lastProvokeAttackMs < PostReviveAttackIntervalMs)
            {
                return true;
            }

            try
            {
                TrySelectBasicPunchSkill(me);

                if (_provokedMobIds.Count < _provokeMobCount)
                {
                    Mob target = FindNearestAliveMob(me, true) ?? FindNearestAliveMob(me, false);
                    if (target != null)
                    {
                        me.mobFocus = target;
                        MyVector vMob = new MyVector();
                        vMob.addElement(target);
                        Service.gI().sendPlayerAttack(vMob, new MyVector(), -1);
                        _provokedMobIds.Add(target.mobId);
                        _lastProvokeAttackMs = now;
                        GameLogger.SendLog("REDUCE_POWER", $"Khiêu khích mob sau hồi sinh: mobId={target.mobId}, tiến độ {_provokedMobIds.Count}/{_provokeMobCount}.");
                        return true;
                    }
                }

                if (_provokedMobIds.Count >= _provokeMobCount)
                {
                    ResetPostReviveProvoke();
                    return false;
                }
            }
            catch
            {
            }

            return true;
        }

        private static int GetBasicPunchSkillId(int gender)
        {
            return gender switch
            {
                1 => 2,
                2 => 4,
                _ => 0
            };
        }

        private void TrySelectBasicPunchSkill(Char me)
        {
            int desiredId = GetBasicPunchSkillId(me.cgender);
            Skill skill = SkillHelper.GetSkill(me, desiredId);
            if (skill != null && !ReferenceEquals(me.myskill, skill))
            {
                GameScr.gI().doSelectSkill(skill, true);
            }
        }

        private Mob FindNearestAliveMob(Char me, bool skipProvoked)
        {
            Mob nearest = null;
            int bestDistance = int.MaxValue;

            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                Mob mob = (Mob)GameScr.vMob.elementAt(i);
                if (!MobHelper.IsMobValidToAttack(mob, true))
                {
                    continue;
                }

                if (skipProvoked && _provokedMobIds.Contains(mob.mobId))
                {
                    continue;
                }

                int distance = Res.distance(me.cx, me.cy, mob.x, mob.y);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = mob;
                }
            }

            return nearest;
        }

        private Char FindNearestBlackFlagPlayer(Char me)
        {
            Char nearest = null;
            int bestDistance = 50;

            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                Char c = (Char)GameScr.vCharInMap.elementAt(i);
                if (c == null || c.charID == me.charID || c.isDie || c.cHP <= 0 || c.isHide) continue;
                if (c.cFlag != 8) continue;

                int dist = Res.distance(me.cx, me.cy, c.cx, c.cy);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    nearest = c;
                }
            }

            return nearest;
        }
    }
}
