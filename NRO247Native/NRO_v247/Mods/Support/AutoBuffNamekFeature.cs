using System;
using System.Collections.Generic;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Support
{
    public class AutoBuffNamekFeature : NRO_v247.Mods.HotReloadFeatureBase<BuffNamekSettings>, IAutoFeature
    {
        private sealed class HelpRequest
        {
            public string JobId = string.Empty;
            public int DeadAccountId = -1;
            public int MapId = -1;
            public int ZoneId = -1;
            public long RequestedAtMs;
            public bool AckSent;
        }

        private readonly HashSet<string> _targetNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, long> _targetReviveWaitTimes = new Dictionary<int, long>();
        private readonly Random _random = new Random();
        private long _lastCastMs;
        private long _lastStateReportMs;

        private int _currentMapId = -1;
        private int _currentZoneId = -1;
        private long _mapEnterTimeMs;
        private long _lastAntiAfkJumpMs;
        private int _nextAntiAfkDelayMs = 25000;

        private int _anchorX;
        private int _anchorY;
        private bool _hasAnchor;

        private long _waitToReturnAnchorMs;
        private bool _isReturningToAnchor;

        private string _runtimeState = "Idle";
        private string _lastTargetName = string.Empty;
        private HelpRequest _help;

        public bool IsActive => _settings.Enabled;
        public bool IsUtilityTask => false;
        public int Priority => 70;
        public string Name => "AutoBuffNamek";
        public string CurrentState => _runtimeState;

        public void Initialize()
        {
        }

        public void ApplySettingsFromPanel(
            bool enabled,
            int mapId,
            bool requireZone,
            int zoneId,
            bool requirePosition,
            int posX,
            int posY,
            int skillId,
            int buffTargetMode = 0,
            int buffCondition = 0,
            int hpThreshold = 50,
            int buffRangeMode = 0,
            string targetNames = "")
        {
            UpdateSettings(new BuffNamekSettings
            {
                Enabled = enabled,
                MapId = mapId,
                RequireZone = requireZone,
                ZoneId = zoneId,
                RequirePosition = requirePosition,
                PosX = posX,
                PosY = posY,
                SkillId = skillId > 0 ? skillId : 7,
                BuffTargetMode = Clamp(buffTargetMode, 0, 2),
                BuffCondition = Clamp(buffCondition, 0, 2),
                HpThreshold = Clamp(hpThreshold, 1, 99),
                BuffRangeMode = Clamp(buffRangeMode, 0, 1),
                TargetNames = targetNames
            });
            ApplyPendingSettingsImmediately();
        }

        public void HandleReducePowerHelpRequest(string jobId, int deadAccountId, int mapId, int zoneId, int targetX, int targetY, string deadCharName)
        {
            if (!_settings.Enabled) return;

            _help = new HelpRequest
            {
                JobId = string.IsNullOrWhiteSpace(jobId) ? "legacy" : jobId,
                DeadAccountId = deadAccountId,
                MapId = mapId,
                ZoneId = zoneId,
                RequestedAtMs = mSystem.currentTimeMillis(),
                AckSent = false
            };
        }

        public void Update()
        {
            EnsureSettingsApplied();

            Char me = Char.myCharz();
            if (me == null) return;

            long now = mSystem.currentTimeMillis();

            if (_currentMapId != TileMap.mapID || _currentZoneId != TileMap.zoneID)
            {
                _currentMapId = TileMap.mapID;
                _currentZoneId = TileMap.zoneID;
                _mapEnterTimeMs = now;
                _targetReviveWaitTimes.Clear();
            }

            if (_settings.Enabled)
            {
                if (now - _lastAntiAfkJumpMs > _nextAntiAfkDelayMs)
                {
                    _lastAntiAfkJumpMs = now;
                    _nextAntiAfkDelayMs = new Random().Next(25000, 30000);
                    if (!me.meDead && !me.isDie && me.cHP > 0 && me.statusMe != 14 && me.statusMe != 10)
                    {
                        try { GameScr.gI().setCharJump(0); } catch { }
                    }
                }
            }

            if (now - _mapEnterTimeMs < 2000 && _settings.Enabled)
            {
                _runtimeState = "Buff Namek: chờ đồng bộ map";
                ReportState(me, null, null);
                return;
            }

            HelpRequest help = _help;
            bool isHelping = help != null && (now - help.RequestedAtMs < 30000);

            if (!_settings.Enabled)
            {
                if (isHelping && help != null)
                {
                    SendHelpAck(help, false, "feature_disabled");
                    ClearHelp();
                }

                _runtimeState = string.Empty;
                ReportState(me, null, null);
                return;
            }

            // Xóa đoạn check BuffTargetMode != 2 khi isHelping để cho phép hồi sinh nhóm hoặc theo lệnh.

            int targetMapId = _settings.MapId;
            int targetZoneId = _settings.ZoneId;
            bool requireZone = _settings.RequireZone;
            bool requirePosition = !isHelping && _settings.RequirePosition;
            int targetX = _settings.PosX;
            int targetY = _settings.PosY;

            if (!isHelping && targetMapId > 0 && TileMap.mapID != targetMapId)
            {
                _runtimeState = "Buff Namek: về đúng map";
                NavigationController.TryGoToMap(targetMapId);
                ReportState(me, null, null);
                return;
            }

            if (!isHelping && requireZone && targetMapId > 0 && TileMap.mapID == targetMapId)
            {
                if (TileMap.zoneID != targetZoneId && targetZoneId >= 0)
                {
                    _runtimeState = "Buff Namek: đổi khu";
                    NavigationController.TryChangeZone(targetZoneId);
                    ReportState(me, null, null);
                    return;
                }
            }

            if (requirePosition && (targetMapId <= 0 || TileMap.mapID == targetMapId))
            {
                if (!NavigationController.TryMoveTo(me, targetX, targetY, 20, false, 400))
                {
                    _runtimeState = "Buff Namek: về đúng tọa độ";
                    ReportState(me, null, null);
                    return;
                }
            }

            Skill healSkill = FindBuffSkill(me, _settings.SkillId);
            if (healSkill == null)
            {
                _runtimeState = "Buff Namek: thiếu skill";
                if (isHelping && help != null)
                {
                    SendHelpAck(help, false, "missing_skill");
                    ClearHelp();
                }
                ReportState(me, null, null);
                return;
            }

            if (_hasAnchor && _isReturningToAnchor)
            {
                now = mSystem.currentTimeMillis();
                if (now > _waitToReturnAnchorMs)
                {
                    int distToAnchor = Res.distance(me.cx, me.cy, _anchorX, _anchorY);
                    if (distToAnchor <= 5)
                    {
                        _hasAnchor = false;
                        _isReturningToAnchor = false;
                    }
                    else if (NavigationController.TryMoveTo(me, _anchorX, _anchorY, 5, false, 500))
                    {
                        _hasAnchor = false;
                        _isReturningToAnchor = false;
                    }
                }
                _runtimeState = "Buff Namek: chờ về chỗ cũ";
                ReportState(me, healSkill, null);
                return;
            }

            Char target = ResolveTarget(me);
            if (target == null)
            {
                _runtimeState = isHelping
                    ? "Buff Namek: chờ mục tiêu cứu"
                    : (_settings.BuffTargetMode == 2 ? $"Buff Namek: {BuildSkillRuntimeStatus(healSkill, now)}" : "Buff Namek: không có mục tiêu");
                ReportState(me, healSkill, null);
                return;
            }

            if (_settings.BuffRangeMode == 1 && target != me)
            {
                if (!_hasAnchor)
                {
                    _anchorX = me.cx;
                    _anchorY = me.cy;
                    _hasAnchor = true;
                }

                int dist = Res.distance(me.cx, me.cy, target.cx, target.cy);
                if (dist > 60)
                {
                    if (!NavigationController.TryMoveTo(me, target.cx, target.cy, 20, false, 400))
                    {
                        _runtimeState = "Buff Namek: chờ dịch chuyển";
                        ReportState(me, healSkill, target);
                        return;
                    }
                }
            }

            long castNow = mSystem.currentTimeMillis();
            if (castNow - _lastCastMs < 120)
            {
                _runtimeState = "Buff Namek: chờ cast";
                ReportState(me, healSkill, target);
                return;
            }

            if (castNow - healSkill.lastTimeUseThisSkill < healSkill.coolDown)
            {
                long remainMs = healSkill.coolDown - (castNow - healSkill.lastTimeUseThisSkill);
                if (remainMs < 0) remainMs = 0;
                int remainSec = (int)Math.Ceiling(remainMs / 1000.0);
                _runtimeState = $"Buff Namek: skill hồi còn {remainSec}s";
                ReportState(me, healSkill, target);
                return;
            }

            if (target != null && target != me && _settings.BuffCondition == 1)
            {
                if (!_targetReviveWaitTimes.TryGetValue(target.charID, out long waitTime))
                {
                    waitTime = castNow + _random.Next(100, 700);
                    _targetReviveWaitTimes[target.charID] = waitTime;
                }

                if (castNow < waitTime)
                {
                    _runtimeState = "Buff Namek: chờ random chống trùng";
                    ReportState(me, healSkill, target);
                    return;
                }
            }

            ExecuteBuffSkill(me, healSkill, target);
            if (target != null && target != me)
            {
                _targetReviveWaitTimes.Remove(target.charID);
            }
            healSkill.lastTimeUseThisSkill = castNow;
            _lastCastMs = castNow;
            _runtimeState = "Buff Namek: Đang buff";
            _lastTargetName = target.cNameClear;

            if (isHelping && help != null)
            {
                SendHelpAck(help, true, "cast_sent");
                ClearHelp();
            }

            if (_settings.BuffRangeMode == 1 && target != me && _hasAnchor)
            {
                _isReturningToAnchor = true;
                _waitToReturnAnchorMs = castNow + 1000;
            }

            ReportState(me, healSkill, target);
        }

        public void Draw(mGraphics g)
        {
        }

        public void Dispose()
        {
        }

        protected override void OnSettingsHotReload()
        {
            RebuildTargetNames();
            _lastCastMs = 0;
            _anchorX = 0;
            _anchorY = 0;
            _hasAnchor = false;
            _isReturningToAnchor = false;
            _lastTargetName = string.Empty;
            _runtimeState = _settings.Enabled ? "Buff Namek: áp dụng cài đặt mới" : string.Empty;
        }

        private Char ResolveTarget(Char me)
        {
            bool isHelping = _help != null && (mSystem.currentTimeMillis() - _help.RequestedAtMs < 20000);

            if (isHelping)
            {
                _lastTargetName = me.cNameClear;
                return me;
            }

            if (_settings.BuffTargetMode == 2)
            {
                _lastTargetName = string.Empty;
                return null;
            }

            if (_settings.BuffTargetMode == 0)
            {
                _lastTargetName = me.cNameClear;
                return me;
            }

            if (GameScr.vCharInMap == null)
            {
                _lastTargetName = string.Empty;
                return null;
            }

            Char picked = null;
            int bestDistance = int.MaxValue;

            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                Char c = (Char)GameScr.vCharInMap.elementAt(i);
                if (c == null) continue;

                string clearName = (c.cNameClear ?? string.Empty).Trim();
                if (clearName.Length == 0) continue;

                if (_targetNameSet.Count > 0 && !_targetNameSet.Contains(clearName)) continue;
                if (!CheckTargetCondition(c)) continue;

                int dist = Res.distance(me.cx, me.cy, c.cx, c.cy);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    picked = c;
                }
            }

            _lastTargetName = picked?.cNameClear ?? string.Empty;
            return picked;
        }

        private bool CheckTargetCondition(Char target)
        {
            switch (_settings.BuffCondition)
            {
                case 1:
                    return target.meDead || target.isDie || target.statusMe == 14;
                case 2:
                    if (target.cHPFull <= 0) return false;
                    long hpPct = target.cHP * 100 / target.cHPFull;
                    return hpPct <= _settings.HpThreshold;
                default:
                    return true;
            }
        }

        private void RebuildTargetNames()
        {
            _targetNameSet.Clear();
            if (string.IsNullOrWhiteSpace(_settings.TargetNames)) return;

            string normalized = _settings.TargetNames.Replace("\\n", "\n");
            string[] names = normalized.Split(new[] { '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < names.Length; i++)
            {
                string n = names[i].Trim();
                if (n.Length > 0) _targetNameSet.Add(n);
            }
        }

        private static string BuildSkillRuntimeStatus(Skill skill, long now)
        {
            if (skill == null) return "thiếu skill";

            long elapsed = now - skill.lastTimeUseThisSkill;
            long remainMs = skill.coolDown - elapsed;
            if (remainMs <= 0) return "skill sẵn sàng";

            int remainSec = (int)Math.Ceiling(remainMs / 1000.0);
            return $"skill hồi còn {remainSec}s";
        }

        private void ReportState(Char me, Skill skill, Char target)
        {
            long now = mSystem.currentTimeMillis();
            if (now - _lastStateReportMs < 1000) return;
            _lastStateReportMs = now;

            long cdTotal = skill?.coolDown ?? 0;
            long cdRemain = 0;
            if (skill != null)
            {
                long elapsed = now - skill.lastTimeUseThisSkill;
                cdRemain = skill.coolDown - elapsed;
                if (cdRemain < 0) cdRemain = 0;
            }

            string state = SafeField(_runtimeState);
            string targetName = SafeField(target?.cNameClear ?? _lastTargetName);

            SocketGame.SendMessage(
                $"BUFF_NAMEK_STATE|{AutoLogin.idClientSocket}|{TileMap.mapID}|{TileMap.zoneID}|{me.cx}|{me.cy}|{_settings.SkillId}|{cdTotal}|{cdRemain}|{_lastCastMs}|{state}|{targetName}");
        }

        private static Skill FindBuffSkill(Char me, int skillId)
        {
            return SkillHelper.GetSkill(me, skillId);
        }

        private static void ExecuteBuffSkill(Char me, Skill skill, Char target)
        {
            if (me == null || skill == null) return;

            Char buffTarget = target ?? me;
            int previousSkillId = me.myskill?.template != null ? me.myskill.template.id : skill.template.id;

            Service.gI().selectSkill(skill.template.id);
            SendBuffToChar(buffTarget);
            Service.gI().selectSkill(previousSkillId);
        }

        private static void SendBuffToChar(Char target)
        {
            if (target == null) return;
            try
            {
                MyVector vChar = new MyVector();
                vChar.addElement(target);
                Service.gI().sendPlayerAttack(new MyVector(), vChar, -1);
            }
            catch
            {
            }
        }

        private void SendHelpAck(HelpRequest help, bool ok, string reason)
        {
            if (help == null || help.AckSent) return;
            help.AckSent = true;
            long castAt = mSystem.currentTimeMillis();
            string safeReason = SafeField(reason);
            SocketGame.SendMessage(
                $"REDUCE_POWER_HELP_ACK|{AutoLogin.idClientSocket}|{help.DeadAccountId}|{SafeField(help.JobId)}|{(ok ? 1 : 0)}|{castAt}|{safeReason}");
        }

        private void ClearHelp()
        {
            _help = null;
        }

        private static string SafeField(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("|", " ").Replace("\n", " ").Replace("\r", " ");
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            return value > max ? max : value;
        }
    }
}
