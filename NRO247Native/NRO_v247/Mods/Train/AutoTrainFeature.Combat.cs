using System;
using System.Collections.Generic;
using NRO_v247.Mods.Xmap;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods
{
    public partial class AutoTrainFeature
    {
        // ──────────────────────────────────────────────────────────────────
        // Di chuyển + tung skill
        // ──────────────────────────────────────────────────────────────────
        private void ResolveMovementCombat(Char me, Mob target, bool usingTDLT)
        {
            if (IsCombatGuardBlocked(me)) return;

            if (_lastFocusMobId != target.mobId)
            {
                _lastFocusMobId = target.mobId;
                AstarTele.gI().Reset();
            }

            // Đã bỏ chặn animation ở đây vì thỉnh thoảng biến state bị kẹt trên client
            // Gây ra lỗi "đứng im nhìn quái". Việc điều tiết tốc độ đánh đã được
            // quản lý bởi AttackCooldownMs ở hàm ExecuteAttack bên dưới.

            if (UseTDLT) TdltController.Update(true);

            int distance = Res.distance(me.cx, me.cy, target.x, target.y);
            if (distance > MoveDistanceThreshold) TeleportDirect(target.x, target.y);
            
            Skill chosenSkill = ResolveSkill(me);
            if (chosenSkill != null) ExecuteAttack(me, target, chosenSkill);
        }

        private static void TeleportDirect(int x, int y)
        {
            Char me = Char.myCharz();
            if (me == null) return;
            me.currentMovePoint = null;
            me.cx = x;
            me.cy = y;
            Service.gI().charMove();
        }

        // ──────────────────────────────────────────────────────────────────
        // Attack & Skills
        // ──────────────────────────────────────────────────────────────────
        private void ExecuteAttack(Char me, Mob target, Skill chosenSkill)
        {
            if (me == null || target == null || chosenSkill == null) return;

            long now = mSystem.currentTimeMillis();

            // Uỷ quyền cho phân lớp KsVangController quản lý
            KsVangController.ApplyFirstHitOptimization(this, me, target, ref _lastAttackAtMs);

            long cooldown = AttackCooldownTdltMs;
            if (now - _lastAttackAtMs < cooldown) return;

            int skillId = chosenSkill.template.id;
            
            int attackType = GameScr.canAutoPlay ? 1 : -1;

            if (NoFocusSkillIds.Contains(skillId))
            {
                if (!ReferenceEquals(me.myskill, chosenSkill) && !EnsureSkillSelected(me, chosenSkill, false))
                    return;

                Service.gI().skill_not_focus(MapNoFocusSkillStatus(skillId));
                if (me.myskill != null) me.myskill.lastTimeUseThisSkill = now;
                _lastAttackAtMs = now;
                return;
            }

            if (!ReferenceEquals(me.mobFocus, target)) me.mobFocus = target;

            if (CanSendPacketAttack(me, target, chosenSkill))
            {
                if (SendAttackPacket(target, attackType))
                {
                    _lastAttackAtMs = now;
                    return;
                }
            }

            GameScr.gI().doDoubleClickToObj(target);
            _lastAttackAtMs = now;
        }

        private bool CanSendPacketAttack(Char me, Mob target, Skill skill)
        {
            if (me == null || target == null || skill?.template == null) return false;
            if (!ReferenceEquals(me.myskill, skill)) return false;
            if (!PacketAttackSkillIds.Contains(skill.template.id)) return false;
            
            // Xoá check client-side isMeCanAttackMob (game gốc) để trị triệt để lỗi thỉnh thoảng đứng im nhìn quái
            // Vì game thỉnh thoảng bị lỗi state do ta dùng Astar tự di chuyển
            return true;
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
                if (me?.myskill != null) me.myskill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
                return true;
            }
            catch { return false; }
        }

        private Skill ResolveSkill(Char me)
        {
            if (me == null) return null;

            Skill desired = ChooseSkill(me);
            if (desired == null) return null;

            if (ReferenceEquals(me.myskill, desired)) return desired;

            // Không ép đổi skill (forceSwitch = false) để tránh giật lag khi đang đánh
            if (!EnsureSkillSelected(me, desired, false))
            {
                if (IsSkillUsable(me, me.myskill))
                    return me.myskill;
                return null;
            }

            return desired;
        }

        private bool IsSkillChecked(int skillId, int gender)
        {
            if (Skills == null || Skills.Length < 17) return true; // Hỗ trợ fallback

            if (gender == 0) // Trái Đất
            {
                if (skillId == 0) return Skills[0];
                if (skillId == 1) return Skills[1];
                if (skillId == 6) return Skills[2];
                if (skillId == 8) return Skills[3];
                if (skillId == 20 || skillId == 21) return Skills[4];
                if (skillId == 19) return Skills[5];
                if (skillId == 9) return Skills[6];
            }
            else if (gender == 1) // Namếc
            {
                if (skillId == 17) return Skills[7];
                if (skillId == 2) return Skills[8];
                if (skillId == 3) return Skills[9];
                if (skillId == 12) return Skills[10];
                if (skillId == 19) return Skills[11];
            }
            else if (gender == 2) // Xayda
            {
                if (skillId == 4) return Skills[12];
                if (skillId == 5) return Skills[13];
                if (skillId == 13) return Skills[14];
                if (skillId == 8 || skillId == 7) return Skills[15]; // TTNL
                if (skillId == 19) return Skills[16];
            }
            return false;
        }

        private Skill ChooseSkill(Char me)
        {
            if (me == null) return null;

            bool isKaiokenChecked = IsSkillChecked(9, me.cgender);
            bool isLienHoanChecked = IsSkillChecked(17, me.cgender);

            bool blockDragon = isKaiokenChecked;
            bool blockDemon = isLienHoanChecked;

            if (OnlyUsePunch)
            {
                foreach (int id in GetPunchSkillPriority(me))
                {
                    if (id == 0 && blockDragon) continue;
                    if (id == 2 && blockDemon) continue;

                    Skill s = FindFirstUsableSkillById(me, id);
                    if (s != null) return s;
                }
                return null;
            }

            foreach (int id in GetKaiokenLienHoanPriority(me))
            {
                Skill s = FindFirstUsableSkillById(me, id);
                if (s != null) return s;
            }

            for (int i = 0; i < PrioritySkillIds.Length; i++)
            {
                int skillId = PrioritySkillIds[i];
                if (skillId == 0 && blockDragon) continue;
                if (skillId == 2 && blockDemon) continue;

                Skill skill = FindFirstUsableSkillById(me, skillId);
                if (skill != null) return skill;
            }

            return FindFirstUsableAttackSkill(me, blockDemon, blockDragon);
        }

        private static int[] GetKaiokenLienHoanPriority(Char me)
        {
            if (me == null) return new[] { 9, 17 };
            return me.cgender switch { 0 => new[] { 9, 17 }, 1 => new[] { 17, 9 }, _ => new[] { 9, 17 } };
        }

        private static int[] GetPunchSkillPriority(Char me)
        {
            if (me == null) return new[] { 9, 17, 0, 2, 4 };
            return me.cgender switch
            {
                0 => new[] { 9, 0, 2, 4 },
                1 => new[] { 17, 2, 0, 4 },
                2 => new[] { 4, 0, 2 },
                _ => new[] { 9, 17, 0, 2, 4 }
            };
        }

        private Skill FindFirstUsableSkillById(Char me, int skillId)
        {
            Skill skill = SkillHelper.GetSkill(me, skillId);
            if (skill != null && IsSkillUsable(me, skill)) return skill;
            return null;
        }

        private Skill FindFirstUsableAttackSkill(Char me, bool blockDemon, bool blockDragon)
        {
            if (me?.vSkill == null) return null;
            for (int i = 0; i < me.vSkill.size(); i++)
            {
                Skill skill = (Skill)me.vSkill.elementAt(i);
                if (skill?.template == null) continue;
                if (!skill.template.isAttackSkill()) continue;

                if (blockDemon && skill.template.id == 2) continue;
                if (blockDragon && skill.template.id == 0) continue;

                if (IsSkillUsable(me, skill)) return skill;
            }
            return null;
        }

        private bool IsSkillUsable(Char me, Skill skill)
        {
            if (me == null || skill?.template == null) return false;

            int skillId = skill.template.id;

            if (skillId == 19)
            {
                if (ItemTime.isExistItem(3784)) return false;
                
                if (UseShieldUnderHp && me.cHP * 100L / Math.Max(1, me.cHPFull) > ShieldHpPercent)
                {
                    return false;
                }
                
                if (!IsSkillChecked(skillId, me.cgender))
                {
                    return false;
                }
            }
            else
            {
                if (OnlyUsePunch)
                {
                    if (skillId != 0 && skillId != 2 && skillId != 4) return false;
                }
                else
                {
                    if (!IsSkillChecked(skillId, me.cgender)) return false;
                }
            }

            if (skillId == 10 || skillId == 11 || skillId == 14 || skillId == 23 || skillId == 7)
                return false;

            // Tránh đệ quy vô hạn khi chính nó đang check skill 17.
            // Chỉ cần chặn skill 2 nếu tồn tại một skill 17 đang dùng được.
            if (skillId == 2 && HasUsableSkill17(me)) return false;

            if (!skill.template.isAttackSkill() && !NoFocusSkillIds.Contains(skillId))
                return false;

            if (skill.paintCanNotUseSkill) return false;
            if (me.isMonkey == 1 && skillId == 13) return false;

            if (!SkillHelper.IsSkillReady(skill)) return false;

            return me.cMP >= GetManaNeed(me, skill);
        }

        private static long GetManaNeed(Char me, Skill skill)
        {
            if (me == null || skill?.template == null) return long.MaxValue;
            if (skill.template.manaUseType == 2) return 1;
            if (skill.template.manaUseType == 1)
                return skill.manaUse * me.cMPFull / 100;
            return skill.manaUse;
        }

        private bool HasUsableSkill17(Char me)
        {
            Skill skill = SkillHelper.GetSkill(me, 17);
            if (skill != null && CanUseSkill17(me, skill)) return true;
            return false;
        }

        private bool CanUseSkill17(Char me, Skill skill)
        {
            if (me == null || skill?.template == null || skill.template.id != 17) return false;
            if (OnlyUsePunch) return false;
            if (!IsSkillChecked(17, me.cgender)) return false;
            if (skill.paintCanNotUseSkill) return false;

            if (!SkillHelper.IsSkillReady(skill)) return false;

            return me.cMP >= GetManaNeed(me, skill);
        }

        public static void ApplyFreezePunchSkill(Char me, bool freeze)
        {
            if (me == null) return;
            foreach (int id in PunchSkillIds)
            {
                Skill skill = SkillHelper.GetSkill(me, id);
                if (skill == null) continue;

                if (freeze)
                {
                    skill.coolDown = 0;
                    skill.manaUse = 0;
                }
                else
                {
                    if (skill.template.skills != null && skill.point > 0 && skill.point <= skill.template.skills.Length)
                    {
                        Skill original = skill.template.skills[skill.point - 1];
                        if (original != null)
                        {
                            skill.coolDown = original.coolDown;
                            skill.manaUse = original.manaUse;
                        }
                    }
                }
            }
        }

        private static bool HasSkill(Char me, int skillId)
        {
            return SkillHelper.GetSkill(me, skillId) != null;
        }

        private bool EnsureSkillSelected(Char me, Skill skill, bool forceSwitch)
        {
            if (me == null || skill?.template == null) return false;

            // So sánh theo ID để tránh việc chọn lại chính xác cùng một loại kỹ năng gây lag
            if (me.myskill?.template != null && me.myskill.template.id == skill.template.id) 
                return true;

            long now = mSystem.currentTimeMillis();
            
            // Ràng buộc thời gian tối thiểu 250ms giữa các lần đổi skill để chống giật lag UI/Sound
            if (!forceSwitch && now - _lastSkillSwitchAtMs < SkillSwitchCooldownMs) 
                return false;

            // Log: "Đổi skill sang {skill.template.name}" - có thể thêm nếu cần debug
            GameScr.gI().doSelectSkill(skill, isShortcut: true);
            _lastSkillSwitchAtMs = now;
            return true;
        }

        private static sbyte MapNoFocusSkillStatus(int skillId) => skillId switch
        {
            6 => 0, 8 => 1, 12 => 8, 13 => 6, 19 => 9, 21 => 10, _ => 0
        };

        private void TryUseNoFocusSkill(Char me)
        {
            if (me == null || me.isCharge || me.isWaitMonkey || OnlyUsePunch) return;

            long now = mSystem.currentTimeMillis();

            foreach (int skillId in NoFocusSkillIds)
            {
                Skill skill = SkillHelper.GetSkill(me, skillId);
                if (skill != null && IsSkillUsable(me, skill))
                {
                    GameScr.gI().doSelectSkill(skill, true);
                    switch (skillId)
                    {
                        case 8: Service.gI().selectSkill(8); Service.gI().skill_not_focus(1); break;
                        case 12: Service.gI().selectSkill(12); Service.gI().skill_not_focus(8); break;
                        case 13: Service.gI().selectSkill(13); Service.gI().skill_not_focus(6); break;
                        case 19: Service.gI().selectSkill(19); Service.gI().skill_not_focus(9); break;
                        case 21: Service.gI().selectSkill(21); Service.gI().skill_not_focus(10); break;
                    }
                    skill.lastTimeUseThisSkill = now;
                    break;
                }
            }
        }
    }
}
