using System;
using System.Collections.Generic;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods
{
    public partial class AutoTrainFeature
    {
        // ──────────────────────────────────────────────────────────────────
        // Lõi Target Mob 
        // ──────────────────────────────────────────────────────────────────
        private void FindAndAttack()
        {
            if (!IsSafeToMove())
            {
                if (_targetMob != null && _targetMob.hp > 0)
                {
                    Char meF2 = Char.myCharz();
                    if (meF2 != null) meF2.mobFocus = _targetMob;
                }
                return;
            }

            long now = mSystem.currentTimeMillis();

            // Fix #1: Khi có TDLT, bỏ qua hoàn toàn delay chờ nhặt đồ — chuyển target tức thì
            if (!_isTdltActiveThisFrame && _mobDeadWaitUntil > now)
            {
                return; // Đang chờ quái văng đồ để nhặt (chỉ khi KHÔNG có TDLT)
            }

            if (_targetMob != null && (_targetMob.hp <= 0 || _targetMob.status <= 1))
            {
                if (!_isTdltActiveThisFrame)
                {
                    _mobDeadWaitUntil = now + 300L;
                }
                // Khi TDLT: không đợi, xoá target rồi quét mới ngay frame này
                _targetMob = null;
                Char me2 = Char.myCharz();
                if (me2 != null) me2.mobFocus = null;
                AstarTele.gI().Reset();
                _lastFocusMobId = -1;
                if (!_isTdltActiveThisFrame) return; // Chỉ return nếu không có TDLT
            }

            // Làm mới target mỗi frame — GetClosestMob() chỉ gọi 1 lần duy nhất
            Char me = Char.myCharz();
            if (me != null)
            {
                Mob newTarget = GetClosestMob();
                if (newTarget != null) _lastTimeMobSeenMs = mSystem.currentTimeMillis();
                if (_targetMob != newTarget)
                {
                    _targetMob = newTarget;
                    AstarTele.gI().Reset();
                    _lastFocusMobId = -1;
                }
            }

            if (_targetMob != null)
            {
                bool hardInvalid = _targetMob.hp <= 0 || _targetMob.isMobMe || IsIgnoredLagMob(_targetMob);
                if (hardInvalid)
                {
                    _targetMob = null;
                    Char me2 = Char.myCharz();
                    if (me2 != null) me2.mobFocus = null;
                    AstarTele.gI().Reset();
                    _lastFocusMobId = -1;
                    return;
                }

                bool softInvalid = _targetMob.status == 0 || _targetMob.status == 1;
                if (softInvalid) return;
            }

            // Không gọi GetClosestMob() lần 2 — newTarget đã được gán vào _targetMob ở trên
            if (_targetMob == null)
            {
                Char me2 = Char.myCharz();
                if (me2 != null) me2.mobFocus = null;
                if (AstarTele.gI().IsMoving()) AstarTele.gI().Reset();

                long now2 = mSystem.currentTimeMillis();
                if (!RequireZone)
                {
                    long cooldown = _isTdltActiveThisFrame ? AutoZoneCooldownTdltMs : AutoZoneCooldownNoTdltMs;
                    if (now2 - _lastChangeZoneAtMs >= cooldown && !Char.ischangingMap && !Controller.isStopReadMessage)
                    {
                        if (RotateZone && _rotateZoneIds.Count > 0)
                        {
                            int currentZone = TileMap.zoneID;
                            int currentIndex = _rotateZoneIds.IndexOf(currentZone);
                            int nextZone = _rotateZoneIds[(currentIndex + 1) % _rotateZoneIds.Count];
                            Service.gI().requestChangeZone(nextZone, -1);
                            _lastChangeZoneAtMs = now2;
                        }
                        else if (ChangeLowPlayerZoneIfNoMob)
                        {
                            IsRequestingZoneList = true;
                            Service.gI().openUIZone();
                            _lastChangeZoneAtMs = now2;
                        }
                    }
                }

                // Reset khu nếu đứng im quá lâu không thấy quái (áp dụng cả khi Khóa khu)
                if (now2 - _lastTimeMobSeenMs >= NoMobResetIntervalMs && !Char.ischangingMap && !Controller.isStopReadMessage)
                {
                    TriggerZoneReset();
                    _lastTimeMobSeenMs = now2; // Tránh đổi liên tục nếu đổi xong vẫn k thấy
                }
                return;
            }

            if (me == null) return;

            // Fix lỗi 'Quái ma': Chỉ focus (hiện HP) khi đứng gần trong tầm 600px (Săn boss thì luôn giữ focus)
            if (_targetMob.templateId == 70 || _targetMob.templateId == 74 || Res.distance(me.cx, me.cy, _targetMob.x, _targetMob.y) <= 600)
            {
                me.mobFocus = _targetMob;
            }
            else if (me.mobFocus == _targetMob)
            {
                me.mobFocus = null; // Ở xa thì xóa focus nhưng vẫn là target nhắm đến
            }
            CheckAndHandleLagMob(_targetMob);

            ResolveMovementCombat(me, _targetMob, _isTdltActiveThisFrame);
        }

        private Mob GetClosestMob()
        {
            if (GameScr.vMob == null) return null;

            Char me = Char.myCharz();
            if (me == null) return null;

            Mob resultFresh = null;
            int minDistanceFresh = int.MaxValue;

            Mob resultDamaged = null;
            int minDistanceDamaged = int.MaxValue;

            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                Mob mob = (Mob)GameScr.vMob.elementAt(i);
                if (!IsValidMob(mob)) continue;

                // Luôn dùng Euclidean distance để chọn target — O(1)/mob, không tốn CPU
                int dist = Res.distance(me.cx, me.cy, mob.x, mob.y);
                
                if (OptimizeKsVang)
                {
                    // KS Vàng: Ưu tiên tuyệt đối quái có HP đầy (quái mới spawn) để ăn 'First Hit'
                    if (mob.hp == mob.maxHp)
                    {
                        if (dist < minDistanceFresh)
                        {
                            minDistanceFresh = dist;
                            resultFresh = mob;
                        }
                    }
                    else
                    {
                        if (dist < minDistanceDamaged)
                        {
                            minDistanceDamaged = dist;
                            resultDamaged = mob;
                        }
                    }
                }
                else
                {
                    // Không ưu tiên theo HP đầy, chỉ tìm quái gần nhất
                    if (dist < minDistanceFresh)
                    {
                        minDistanceFresh = dist;
                        resultFresh = mob;
                    }
                }
            }

            return resultFresh ?? resultDamaged;
        }

        // ──────────────────────────────────────────────────────────────────
        // Validator & Cờ Lag
        // ──────────────────────────────────────────────────────────────────
        private bool IsValidMob(Mob mob)
        {
            if (!MobHelper.IsMobValidToAttack(mob, true) || mob.isMobMe) return false;
            if (IsIgnoredLagMob(mob)) return false;

            if (IsUpZinOverride)
            {
                // Chỉ check cấu hình MaxHP của riêng UpZin
                return mob.maxHp >= UpZinMinHp && mob.maxHp <= UpZinMaxHp;
            }

            if (MobTargetType == 1 && _mobIdSet.Count > 0)
            {
                if (!_mobIdSet.Contains(mob.templateId) && !_mobIdSet.Contains(mob.mobId))
                    return false;
            }

            if (AttackHpAbove && mob.hp <= AttackHpAboveValue) return false;
            if (AttackHpBelow && mob.hp >= AttackHpBelowValue) return false;

            if (IsAvoidSuperMob && !_isTdltActiveThisFrame && mob.checkIsBoss())
                return false;

            return true;
        }

        private void CheckAndHandleLagMob(Mob mob)
        {
            try
            {
                if (mob == null || !IsCheckLagMob || mob.templateId == 70 || mob.templateId == 74) return;

                if (_currentAttackingMobId != mob.mobId)
                {
                    _currentAttackingMobId = mob.mobId;
                    _timeStartAttackMob = mSystem.currentTimeMillis();
                    _lastMobHP = mob.hp;
                    return;
                }

                long attackDuration = mSystem.currentTimeMillis() - _timeStartAttackMob;
                if (attackDuration > LagMobTimeoutMs)
                {
                    if ((double)mob.hp >= (double)_lastMobHP * LagMobHpThreshold)
                    {
                        _ignoredLagMobs[mob.mobId] = mSystem.currentTimeMillis();
                        Char me = Char.myCharz();
                        if (me != null) me.mobFocus = null;
                        _targetMob = null;
                        _currentAttackingMobId = -1;
                        _timeStartAttackMob = 0L;
                        _lastMobHP = -1L;
                        AstarTele.gI().Reset();
                    }
                    else
                    {
                        _timeStartAttackMob = mSystem.currentTimeMillis();
                        _lastMobHP = mob.hp;
                    }
                }
            }
            catch { }
        }

        private bool IsIgnoredLagMob(Mob mob)
        {
            return mob != null && _ignoredLagMobs.ContainsKey(mob.mobId);
        }

        private void ClearIgnoredLagMobs()
        {
            try
            {
                if (_lastMapIdForClearLag != TileMap.mapID)
                {
                    _ignoredLagMobs.Clear();
                    _lastMapIdForClearLag = TileMap.mapID;
                    return;
                }

                if (_ignoredLagMobs.Count > 0)
                {
                    long now = mSystem.currentTimeMillis();
                    var keysToRemove = new List<int>();
                    foreach (var pair in _ignoredLagMobs)
                    {
                        if (now - pair.Value > LagMobClearMs)
                            keysToRemove.Add(pair.Key);
                    }
                    foreach (var k in keysToRemove)
                    {
                        _ignoredLagMobs.Remove(k);
                    }
                }
            }
            catch { }
        }

        public void ClearFocus()
        {
            Char me = Char.myCharz();
            if (me != null) me.mobFocus = null;

            _targetMob = null;
            _lastFocusMobId = -1;
            _currentAttackingMobId = -1;
            _timeStartAttackMob = 0L;
            _lastMobHP = -1L;
            AstarTele.gI().Reset();
        }

        private static string ResolveMobName(Mob mob)
        {
            if (mob == null) return "Mob";
            MobTemplate template = mob.getTemplate();
            if (template != null && !string.IsNullOrEmpty(template.name)) return template.name;
            if (!string.IsNullOrEmpty(mob.mobName)) return mob.mobName;
            return $"Mob {mob.templateId}";
        }

        private void ParseMobIds(string raw)
        {
            _mobIdSet.Clear();
            ListMobIds.Clear();
            if (string.IsNullOrWhiteSpace(raw)) return;

            string[] parts = raw.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                if (int.TryParse(part.Trim(), out int id))
                {
                    _mobIdSet.Add(id);
                    ListMobIds.Add(id);
                }
            }
        }
    }
}
