using System.Collections.Generic;
using NRO_v247.Mods.Utils;
using NRO_v247.Mods.Xmap;

namespace NRO_v247.Mods
{
    internal sealed class TrainCombatController
    {
        private const long AttackCooldownMs = 120;
        private const long SkillSwitchCooldownMs = 250;
        private const long FlyingMobTeleportCooldownMs = 500;
        private const int MoveDistanceThreshold = 50;
        private const int TileSize = 24;

        private static readonly int[] PrioritySkillIds =
        {
            17, 9,
            0, 2, 4,
            1, 5, 3,
            6, 8, 12, 13, 19, 21
        };

        private static readonly HashSet<int> NoFocusSkillIds = new HashSet<int> { 6, 8, 12, 13, 19, 21 };
        private static readonly HashSet<int> PacketAttackSkillIds = new HashSet<int> { 0, 1, 2, 3, 4, 5, 9, 17 };

        private long _lastAttackAtMs;
        private long _lastSkillSwitchAtMs;
        private bool _isHandlingFlyingMob;
        private long _lastFlyingMobTeleportAtMs;
        private int _lastFocusMobId = -1;

        public void UpdateCombat(TrainRuntimeSettings settings, Mob target)
        {
            if (settings == null || target == null) return;

            Char me = Char.myCharz();
            if (me == null || IsCombatGuardBlocked(me)) return;

            if (settings.UseTDLT)
                TdltController.Update(true);

            GameScr.canAutoPlay = true;

            if (me.mobFocus == null || !ReferenceEquals(me.mobFocus, target))
                me.mobFocus = target;

            if (me.mobFocus == null || me.mobFocus.hp <= 0
                || me.mobFocus.status == 0 || me.mobFocus.status == 1)
            {
                ClearFocus();
                return;
            }

            if (_lastFocusMobId != me.mobFocus.mobId)
            {
                _lastFocusMobId = me.mobFocus.mobId;
                _isHandlingFlyingMob = false;
                AstarTele.gI().Reset(); // reset path khi đổi mục tiêu
            }

            if (me.skillInfoPaint() != null
                && me.indexSkill < me.skillInfoPaint().Length
                && me.dart != null
                && me.arr != null)
            {
                return;
            }

            if (!ResolveMovement(me, me.mobFocus))
                return;

            Skill chosenSkill = ResolveSkill(me, settings.OnlyUsePunch, settings.UseKaiokenLienHoan);
            if (chosenSkill == null) return;

            ExecuteAttack(me, me.mobFocus, chosenSkill);
        }

        public void UpdateCombatAgainstPlayer(TrainRuntimeSettings settings, Char target)
        {
            if (settings == null || target == null) return;

            Char me = Char.myCharz();
            if (me == null || IsCombatGuardBlocked(me)) return;
            if (!me.isMeCanAttackOtherPlayer(target)) return;

            if (settings.UseTDLT)
            {
                TdltController.Update(true);
            }

            int distance = Res.distance(me.cx, me.cy, target.cx, target.cy);
            if (distance > MoveDistanceThreshold)
            {
                if (settings.UseTDLT)
                {
                    TeleportDirect(target.cx, target.cy);
                }
                else
                {
                    me.currentMovePoint = new MovePoint(target.cx, target.cy);
                    return;
                }
            }

            Skill chosenSkill = ResolveSkill(me, settings.OnlyUsePunch, settings.UseKaiokenLienHoan);
            if (chosenSkill == null) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastAttackAtMs < AttackCooldownMs) return;

            int skillId = chosenSkill.template.id;
            if (NoFocusSkillIds.Contains(skillId))
            {
                if (!ReferenceEquals(me.myskill, chosenSkill)
                    && !EnsureSkillSelected(me, chosenSkill, forceSwitch: false))
                {
                    return;
                }

                Service.gI().skill_not_focus(MapNoFocusSkillStatus(skillId));
                if (me.myskill != null)
                {
                    me.myskill.lastTimeUseThisSkill = now;
                }

                _lastAttackAtMs = now;
                return;
            }

            if (!ReferenceEquals(me.charFocus, target))
            {
                me.charFocus = target;
            }

            try
            {
                MyVector vChars = new MyVector();
                vChars.addElement(target);
                Service.gI().sendPlayerAttack(new MyVector(), vChars, -1);
                if (me.myskill != null)
                {
                    me.myskill.lastTimeUseThisSkill = now;
                }
                _lastAttackAtMs = now;
            }
            catch
            {
            }
        }

        public void ClearFocus()
        {
            Char me = Char.myCharz();
            if (me != null) me.mobFocus = null;

            _isHandlingFlyingMob = false;
            _lastFocusMobId = -1;
            AstarTele.gI().Reset();
        }

        public void ClearPlayerFocus()
        {
            Char me = Char.myCharz();
            if (me != null)
            {
                me.charFocus = null;
            }

            _lastAttackAtMs = 0L;
            _lastSkillSwitchAtMs = 0L;
            AstarTele.gI().Reset();
        }

        private bool ResolveMovement(Char me, Mob target)
        {
            if (me == null || target == null) return false;

            MobTemplate template = target.getTemplate();
            bool isFlyingMob = template != null && template.type == 4;

            return ResolveMovementTdlt(me, target, isFlyingMob);
        }

        // Di chuyển khi có TDLT (teleport trực tiếp)
        private bool ResolveMovementTdlt(Char me, Mob target, bool isFlyingMob)
        {
            if (!GameScr.canAutoPlay && isFlyingMob)
            {
                long now = mSystem.currentTimeMillis();
                return ResolveFlyingMobMovement(target, now);
            }

            if (!isFlyingMob)
                _isHandlingFlyingMob = false;

            int tx = GameScr.canAutoPlay ? target.x : target.xFirst;
            int ty = GameScr.canAutoPlay ? target.y : target.yFirst;
            int distance = Res.distance(me.cx, me.cy, tx, ty);

            if (distance <= MoveDistanceThreshold) return true;

            TeleportDirect(tx, ty);
            return GameScr.canAutoPlay;
        }

        // Di chuyển khi không có TDLT (dùng A*)
        private bool ResolveMovementAstar(Char me, Mob target, bool isFlyingMob)
        {
            // Quái bay không đi theo A*, xử lý riêng
            if (!GameScr.canAutoPlay && isFlyingMob)
            {
                long now = mSystem.currentTimeMillis();
                return ResolveFlyingMobMovement(target, now);
            }

            if (!isFlyingMob)
                _isHandlingFlyingMob = false;

            int tx = target.xFirst;
            int ty = target.yFirst;
            int distance = Res.distance(me.cx, me.cy, tx, ty);

            if (distance <= MoveDistanceThreshold)
            {
                // Đã đến nơi, dừng A*
                if (AstarTele.gI().IsMoving())
                    AstarTele.gI().Reset();
                return true;
            }

            // Đang di chuyển bằng A* → tiếp tục, chưa thể attack
            if (AstarTele.gI().IsMoving())
                return false;

            // Bắt đầu path mới
            var startTile = new Astar.Point(me.cx / TileSize, me.cy / TileSize);
            var goalTile = new Astar.Point(tx / TileSize, ty / TileSize);
            AstarTele.gI().StartMovement(startTile, goalTile);
            return false;
        }

        private bool ResolveFlyingMobMovement(Mob target, long now)
        {
            if (target == null) return false;

            if (_isHandlingFlyingMob
                && now - _lastFlyingMobTeleportAtMs <= FlyingMobTeleportCooldownMs)
            {
                return false;
            }

            if (!_isHandlingFlyingMob)
            {
                int groundY = XmapNavigator.getYGround(target.x);
                TeleportDirect(target.x, groundY);
                _isHandlingFlyingMob = true;
                _lastFlyingMobTeleportAtMs = now;
                return false;
            }

            TeleportDirect(target.x, target.y);
            _lastFlyingMobTeleportAtMs = now;
            return true;
        }

        private void ExecuteAttack(Char me, Mob target, Skill chosenSkill)
        {
            if (me == null || target == null || chosenSkill == null) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastAttackAtMs < AttackCooldownMs) return;

            int skillId = chosenSkill.template.id;
            if (NoFocusSkillIds.Contains(skillId))
            {
                if (!ReferenceEquals(me.myskill, chosenSkill)
                    && !EnsureSkillSelected(me, chosenSkill, forceSwitch: false))
                {
                    return;
                }

                Service.gI().skill_not_focus(MapNoFocusSkillStatus(skillId));
                if (me.myskill != null)
                    me.myskill.lastTimeUseThisSkill = now;

                _lastAttackAtMs = now;
                return;
            }

            if (!ReferenceEquals(me.mobFocus, target))
                me.mobFocus = target;

            if (CanSendPacketAttack(me, target, chosenSkill))
            {
                if (SendAttackPacket(target, GameScr.canAutoPlay ? 1 : -1))
                {
                    _lastAttackAtMs = now;
                    return;
                }
            }

            GameScr.gI().doDoubleClickToObj(target);
            _lastAttackAtMs = now;
        }

        private bool CanSendPacketAttack(Char me, Mob target, Skill chosenSkill)
        {
            if (me == null || target == null || chosenSkill == null
                || chosenSkill.template == null)
                return false;

            if (!ReferenceEquals(me.myskill, chosenSkill)) return false;
            if (!PacketAttackSkillIds.Contains(chosenSkill.template.id)) return false;

            return GameScr.gI().isMeCanAttackMob(target);
        }

        private bool SendAttackPacket(Mob target, int type = -1)
        {
            if (target == null) return false;
            try
            {
                MyVector vMob = new MyVector();
                vMob.addElement(target);
                Service.gI().sendPlayerAttack(vMob, new MyVector(), type);

                Char me = Char.myCharz();
                if (me?.myskill != null)
                    me.myskill.lastTimeUseThisSkill = mSystem.currentTimeMillis();

                return true;
            }
            catch { return false; }
        }

        private Skill ResolveSkill(Char me, bool onlyUsePunch, bool useKaiokenLienHoan)
        {
            if (me == null) return null;

            Skill desiredSkill = ChooseSkill(me, onlyUsePunch, useKaiokenLienHoan);
            if (desiredSkill == null) return null;

            if (ReferenceEquals(me.myskill, desiredSkill)) return desiredSkill;

            if (!EnsureSkillSelected(me, desiredSkill, forceSwitch: onlyUsePunch))
            {
                if (IsSkillUsable(me, me.myskill, hasSkill17: false))
                    return me.myskill;
                return null;
            }

            return desiredSkill;
        }

        private Skill ChooseSkill(Char me, bool onlyUsePunch, bool useKaiokenLienHoan)
        {
            if (onlyUsePunch)
            {
                if (useKaiokenLienHoan)
                {
                    foreach (int id in GetKaiokenLienHoanPriority(me))
                    {
                        Skill s = FindFirstUsableSkillById(me, id, hasSkill17: false);
                        if (s != null) return s;
                    }
                }

                foreach (int id in GetPunchSkillPriority(me))
                {
                    Skill s = FindFirstUsableSkillById(me, id, hasSkill17: false);
                    if (s != null) return s;
                }
                return null;
            }

            if (useKaiokenLienHoan)
            {
                foreach (int id in GetKaiokenLienHoanPriority(me))
                {
                    Skill s = FindFirstUsableSkillById(me, id, hasSkill17: false);
                    if (s != null) return s;
                }
            }

            bool hasSkill17 = FindFirstUsableSkillById(me, 17, hasSkill17: false) != null;

            for (int i = 0; i < PrioritySkillIds.Length; i++)
            {
                int skillId = PrioritySkillIds[i];
                if (hasSkill17 && skillId == 2) continue;

                Skill skill = FindFirstUsableSkillById(me, skillId, hasSkill17);
                if (skill != null) return skill;
            }

            return FindFirstUsableAttackSkill(me, hasSkill17);
        }

        private static int[] GetKaiokenLienHoanPriority(Char me)
        {
            if (me == null) return new[] { 9, 17 };
            return me.cgender switch
            {
                0 => new[] { 9, 17 },
                1 => new[] { 17, 9 },
                _ => new[] { 9, 17 }
            };
        }

        private static int[] GetPunchSkillPriority(Char me)
        {
            if (me == null) return new[] { 0, 2, 4 };
            return me.cgender switch
            {
                0 => new[] { 0, 2, 4 },
                1 => new[] { 2, 0, 4 },
                2 => new[] { 4, 0, 2 },
                _ => new[] { 0, 2, 4 }
            };
        }

        private Skill FindFirstUsableSkillById(Char me, int skillId, bool hasSkill17)
        {
            Skill skill = SkillHelper.GetSkill(me, skillId);
            if (skill != null && IsSkillUsable(me, skill, hasSkill17))
            {
                return skill;
            }
            return null;
        }

        private Skill FindFirstUsableAttackSkill(Char me, bool hasSkill17)
        {
            if (me?.vSkill == null) return null;
            for (int i = 0; i < me.vSkill.size(); i++)
            {
                Skill skill = (Skill)me.vSkill.elementAt(i);
                if (skill?.template == null) continue;
                if (!skill.template.isAttackSkill()) continue;
                if (IsSkillUsable(me, skill, hasSkill17)) return skill;
            }
            return null;
        }

        private bool IsSkillUsable(Char me, Skill skill, bool hasSkill17)
        {
            if (me == null || skill?.template == null) return false;

            int skillId = skill.template.id;
            if (skillId == 10 || skillId == 11 || skillId == 14
                || skillId == 23 || skillId == 7)
                return false;

            if (hasSkill17 && skillId == 2) return false;

            if (!skill.template.isAttackSkill() && !NoFocusSkillIds.Contains(skillId))
                return false;

            if (skill.paintCanNotUseSkill) return false;

            if (me.isMonkey == 1 && skillId == 13) return false;

            if (skillId == 19 && ItemTime.isExistItem(3784)) return false;

            if (!SkillHelper.IsSkillReady(skill)) return false;

            return me.cMP >= GetManaNeed(me, skill);
        }

        private bool EnsureSkillSelected(Char me, Skill skill, bool forceSwitch)
        {
            if (me == null || skill?.template == null) return false;
            if (ReferenceEquals(me.myskill, skill)) return true;

            long now = mSystem.currentTimeMillis();
            if (!forceSwitch && now - _lastSkillSwitchAtMs < SkillSwitchCooldownMs)
                return false;

            GameScr.gI().doSelectSkill(skill, isShortcut: true);
            _lastSkillSwitchAtMs = now;
            return true;
        }

        private long GetManaNeed(Char me, Skill skill)
        {
            if (me == null || skill?.template == null) return long.MaxValue;
            if (skill.template.manaUseType == 2) return 1;
            if (skill.template.manaUseType == 1)
                return skill.manaUse * me.cMPFull / 100;
            return skill.manaUse;
        }

        private static sbyte MapNoFocusSkillStatus(int skillId) => skillId switch
        {
            6 => 0,
            8 => 1,
            12 => 8,
            13 => 6,
            19 => 9,
            21 => 10,
            _ => 0
        };

        private bool IsCombatGuardBlocked(Char me)
        {
            if (me == null) return true;
            if (me.meDead || me.cHP <= 0 || me.statusMe == 14 || me.statusMe == 5)
                return true;
            if (me.isWaitMonkey || me.isCharge || me.isFlyAndCharge || me.isUseChargeSkill())
                return true;
            return GameScr.gI().isCharging();
        }

        private static void TeleportDirect(int x, int y)
        {
            Char me = Char.myCharz();
            if (me == null) return;

            me.cx = x;
            me.cy = y;
            Service.gI().charMove();

            if (!GameScr.canAutoPlay)
            {
                me.cx = x;
                me.cy = y + 1;
                Service.gI().charMove();

                me.cx = x;
                me.cy = y;
                Service.gI().charMove();
            }
        }

    }
}
