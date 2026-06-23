using System;

namespace NRO_v247.Mods.Utils
{
    public static class SkillHelper
    {
        /// <summary>
        /// Tìm kỹ năng theo ID template trong danh sách rút gọn hoặc tất cả danh sách
        /// Ưu tiên tìm trong vSkillFight (chiêu đưa vào shortcut) trước, sau đó tìm trong toàn bộ vSkill.
        /// </summary>
        public static Skill GetSkill(Char me, int templateId)
        {
            if (me == null) return null;

            // Tìm trong vSkillFight (Kỹ năng trên thanh shortcut)
            if (me.vSkillFight != null)
            {
                for (int i = 0; i < me.vSkillFight.size(); i++)
                {
                    Skill skill = (Skill)me.vSkillFight.elementAt(i);
                    if (skill?.template != null && skill.template.id == templateId)
                    {
                        return skill;
                    }
                }
            }

            // Tìm trong toàn bộ danh sách kỹ năng vSkill đã học
            if (me.vSkill != null)
            {
                for (int i = 0; i < me.vSkill.size(); i++)
                {
                    Skill skill = (Skill)me.vSkill.elementAt(i);
                    if (skill?.template != null && skill.template.id == templateId)
                    {
                        return skill;
                    }
                }
            }

            // Kiểm tra myskill nếu được cài mặc định
            if (me.myskill?.template != null && me.myskill.template.id == templateId)
            {
                return me.myskill;
            }

            return null;
        }

        /// <summary>
        /// Kiểm tra xem một kỹ năng đã sẵn sàng để xuất chiêu chưa (đã kết thúc thời gian hồi chiêu)
        /// </summary>
        public static bool IsSkillReady(Skill skill)
        {
            if (skill == null) return false;
            long elapsed = mSystem.currentTimeMillis() - skill.lastTimeUseThisSkill;
            return elapsed >= skill.coolDown;
        }

        /// <summary>
        /// Lấy số mili-giây còn lại để hồi chiêu
        /// </summary>
        public static long GetSkillCooldownRemain(Skill skill)
        {
            if (skill == null) return 0;
            long elapsed = mSystem.currentTimeMillis() - skill.lastTimeUseThisSkill;
            long remain = skill.coolDown - elapsed;
            return remain < 0 ? 0 : remain;
        }

        /// <summary>
        /// Nhân vật có đang bị tê liệt, đóng băng hoặc không thể ra chiêu hay không
        /// </summary>
        public static bool CanCharUseSkill(Char me)
        {
            return me != null && !me.blindEff && !me.sleepEff && !me.isFreez && me.statusMe != 14 && me.statusMe != 5;
        }

        /// <summary>
        /// Tìm kỹ năng có hiệu ứng trói/choáng (Thái Dương Hạ San, Thôi Miên, Ma Phong Ba, Trói)
        /// </summary>
        public static Skill GetTieSkill(Char me)
        {
            if (me?.vSkill == null) return null;
            for (int i = 0; i < me.vSkill.size(); i++)
            {
                Skill sk = (Skill)me.vSkill.elementAt(i);
                if (sk?.template == null) continue;
                string skName = (sk.template.name ?? "").ToLower();
                if (sk.template.id == 23 || skName.Contains("trói") || skName.Contains("ha san") 
                    || skName.Contains("hạ san") || skName.Contains("thôi miên"))
                {
                    return sk;
                }
            }
            return null;
        }
    }
}
