using System;
using NRO_v247.Mods;
using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods.Pet
{
    public class AutoPetFeature : HotReloadFeatureBase<PetSettings>, IAutoFeature
    {
        public bool IsActive => _settings.EnableAutoPet; 
        public bool IsUtilityTask => false;
        int IAutoFeature.Priority => 60;
        public string Name => "AutoPet";
        public string CurrentState => "Đang up đệ";

        private long _lastTimeAutoPem = 0;
        private long _lastTimeTTNL = 0;
        private long _lastTimeHealing = 0;
        private long _lastTimeJump = 0;
        private long _lastTimeUsePetBuff = 0;

        public void Initialize()
        {
        }

        public void ApplySettingsFromPanel(bool enableAutoPet, bool autoPemWhenPetCall, bool autoKOK, bool autoTTNL, int ttnlPercent, bool autoHealing, bool autoFocusPet,
            bool autoGobackMap, int targetMapId, bool autoGobackZone, int targetZoneId, bool autoGobackPosition, int targetX, int targetY,
            bool autoStopAtPower = false, long targetPower = 150000000, bool autoJump = false, bool autoUsePetBuff = false)
        {
            UpdateSettings(new PetSettings
            {
                EnableAutoPet = enableAutoPet,
                AutoPemWhenPetCall = autoPemWhenPetCall,
                AutoKOK = autoKOK,
                AutoTTNL = autoTTNL,
                TTNLPercent = ttnlPercent,
                AutoHealing = autoHealing,
                AutoFocusPet = autoFocusPet,
                AutoGobackMap = autoGobackMap,
                TargetMapId = targetMapId,
                AutoGobackZone = autoGobackZone,
                TargetZoneId = targetZoneId,
                AutoGobackPosition = autoGobackPosition,
                TargetX = targetX,
                TargetY = targetY,
                AutoStopAtPower = autoStopAtPower,
                TargetPower = targetPower,
                AutoJump = autoJump,
                AutoUsePetBuff = autoUsePetBuff,
            });
            ApplyPendingSettingsImmediately();
        }

        public void Update()
        {
            EnsureSettingsApplied();

            if (Char.myCharz() == null) return;
            Char me = Char.myCharz();

            // Chỉ block khi chết hoặc chưa ở màn hình game.
            // statusMe == 14 vẫn cho phép chạy Goback để giữ TDLT liên tục đến đúng tọa độ đích.
            if (me.isDie || GameCanvas.currentScreen != GameScr.gI())
                return;

            if (!NavigationController.ProcessDumbGoback(me, 
                _settings.AutoGobackMap ? _settings.TargetMapId : -1,
                _settings.AutoGobackZone, _settings.TargetZoneId,
                _settings.AutoGobackPosition, _settings.TargetX, _settings.TargetY))
            {
                return;
            }

            // Sau khi đã về đúng đích mới chạy các logic quản lý đệ/combat.
            if (me.statusMe == 14)
                return;

            if (_settings.AutoStopAtPower && _settings.TargetPower > 0 && me.havePet)
            {
                Char pet = Char.myPetz();
                if (pet != null && pet.cPower >= _settings.TargetPower)
                {
                    // Chỉ đổi trạng thái đệ tử thành "Về nhà"
                    if (pet.petStatus != 3) 
                    {
                        Service.gI().petStatus(3);
                    }
                    
                    return; // Ngừng tiếp tục loop để đệ ở nhà
                }
            }

            HandleAutoFocusPet();
            HandleAutoTTNL(me);
            HandleAutoHealing(me);
            HandleAutoKOK(me);
            HandleAutoPemWhenPetCall();
            HandleAutoJump(me);
            HandleAutoUsePetBuff(me);

        }

        public void Draw(mGraphics g)
        {
        }

        public void Dispose()
        {
        }

        protected override void OnSettingsHotReload()
        {
            _isPetCalling = false;
            _petCallTime = 0;
        }

        // --- Logic Methods ---

        private bool _isPetCalling = false;
        private long _petCallTime = 0;

        public void CheckPetChat(string info, int charId)
        {
            if (!_settings.AutoPemWhenPetCall) return;
            if (Char.myCharz() == null) return;
            if (charId != Char.myCharz().charID * -1) return;

            if (info.ToLower().Contains("sao sư phụ không đánh đi?"))
            {
                _isPetCalling = true;
                _petCallTime = mSystem.currentTimeMillis();
            }
        }

        private void HandleAutoPemWhenPetCall()
        {
            if (!_settings.AutoPemWhenPetCall || !_isPetCalling) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastTimeAutoPem < 1000) return;

            Mob targetMob = null;
            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                Mob mob = (Mob)GameScr.vMob.elementAt(i);
                if (mob == null || mob.status == 0 || mob.status == 1 || mob.hp <= 0 || mob.isHide) continue; // Dead or hiding
                if (targetMob == null || Res.distance(Char.myCharz().cx, Char.myCharz().cy, mob.x, mob.y) < Res.distance(Char.myCharz().cx, Char.myCharz().cy, targetMob.x, targetMob.y))
                {
                    targetMob = mob;
                }
            }

            if (targetMob != null)
            {
                Char.myCharz().mobFocus = targetMob;
                Skill skill = Char.myCharz().myskill;
                if (skill != null && now - skill.lastTimeUseThisSkill > skill.coolDown)
                {
                    GameScr.gI().doSelectSkill(skill, true);
                    MyVector vMobObj = new MyVector();
                    vMobObj.addElement(targetMob);
                    Service.gI().sendPlayerAttack(vMobObj, new MyVector(), -1);
                    skill.lastTimeUseThisSkill = now;
                    _lastTimeAutoPem = now;
                }
            }

            // Reset after 5 seconds
            if (now - _petCallTime > 5000)
            {
                _isPetCalling = false;
            }
        }

        private int _xOld = 0;
        private long _lastTimeKOK = 0;

        private void HandleAutoKOK(Char me)
        {
            if (!_settings.AutoKOK) return;

            // Restrict maps
            if (TileMap.mapID == me.cgender + 21 || TileMap.mapID > 111 || TileMap.mapID == 47 || me.isDie || !me.havePet)
            {
                return;
            }

            long now = mSystem.currentTimeMillis();
            if (now - _lastTimeKOK < 1000) return; // run roughly every second

            try
            {
                int currentX = me.cx;
                int currentY = me.cy;
                if (currentX < _xOld)
                {
                    me.cxSend = 0;
                    me.cySend = 0;
                    me.cx = currentX - 50;
                    me.cy = currentY;
                    me.cdir = -1;
                    Service.gI().charMove();
                    
                    me.cxSend = 0;
                    me.cySend = 0;
                    me.cx = currentX;
                    me.cy = currentY;
                    me.cdir = 1;
                    Service.gI().charMove();
                }
                else
                {
                    me.cxSend = 0;
                    me.cySend = 0;
                    me.cx = currentX + 50;
                    me.cy = currentY;
                    me.cdir = 1;
                    Service.gI().charMove();
                    
                    me.cxSend = 0;
                    me.cySend = 0;
                    me.cx = currentX;
                    me.cy = currentY;
                    me.cdir = -1;
                    Service.gI().charMove();
                }
                _xOld = me.cx;
                _lastTimeKOK = now;
            }
            catch { }
        }

        private void HandleAutoTTNL(Char me)
        {
            if (!_settings.AutoTTNL) return;

            long now = mSystem.currentTimeMillis();
            if (now - _lastTimeTTNL < 2000) return; // limit check rate

            long maxHp = me.cHPFull;
            long hp = me.cHP;
            long maxKi = me.cMPFull;
            long ki = me.cMP;

            if (maxHp <= 0 || maxKi <= 0) return;

            int hpPct = (int)(hp * 100 / maxHp);
            int kiPct = (int)(ki * 100 / maxKi);

            if (hpPct <= _settings.TTNLPercent || kiPct <= _settings.TTNLPercent)
            {
                Skill ttnlSkill = SkillHelper.GetSkill(me, 8); // skill TDHS/TTNL

                if (ttnlSkill != null && now - ttnlSkill.lastTimeUseThisSkill > ttnlSkill.coolDown)
                {
                    Service.gI().selectSkill(8);
                    Service.gI().skill_not_focus(1);
                    ttnlSkill.lastTimeUseThisSkill = now;
                    _lastTimeTTNL = now;
                }
            }
        }

        private void HandleAutoHealing(Char me)
        {
            if (!_settings.AutoHealing) return;
            
            long now = mSystem.currentTimeMillis();
            if (now - _lastTimeHealing < 2000) return;

            Skill hsSkill = SkillHelper.GetSkill(me, 7); // Trị thương

            if (hsSkill != null && now - hsSkill.lastTimeUseThisSkill > hsSkill.coolDown)
            {
                Service.gI().selectSkill(7);
                Service.gI().skill_not_focus(1);
                hsSkill.lastTimeUseThisSkill = now;
                _lastTimeHealing = now;
            }
        }

        private void HandleAutoFocusPet()
        {
            if (!_settings.AutoFocusPet) return;
            
            for (int i = 0; i < GameScr.vCharInMap.size(); i++)
            {
                Char c = (Char)GameScr.vCharInMap.elementAt(i);
                if (c != null && c.charID == Char.myCharz().charID * -1)
                {
                    Char.myCharz().charFocus = c;
                    break;
                }
            }
        }

        private void HandleAutoJump(Char me)
        {
            if (!_settings.AutoJump) return;
            
            long now = mSystem.currentTimeMillis();
            if (now - _lastTimeJump < 3000) return;

            try
            {
                if (!me.isLockMove && me.statusMe != 14 && !me.isDie)
                {
                    GameScr.gI().setCharJump(0);
                    _lastTimeJump = now;
                }
            }
            catch { }
        }

        private void HandleAutoUsePetBuff(Char me)
        {
            if (!_settings.AutoUsePetBuff || !me.havePet) return;
            
            long now = mSystem.currentTimeMillis();
            if (now - _lastTimeUsePetBuff < 5000) return;

            try
            {
                Char pet = Char.myPetz();
                if (pet == null) return;

                bool hasPetBuff = false;
                for (int i = 0; i < pet.vEff.size(); i++)
                {
                    EffectChar eff = (EffectChar)pet.vEff.elementAt(i);
                    if (eff != null && eff.template != null && eff.template.id == 33)
                    {
                        hasPetBuff = true;
                        break;
                    }
                }

                if (!hasPetBuff)
                {
                    for (int i = 0; i < me.arrItemBag.Length; i++)
                    {
                        Item item = me.arrItemBag[i];
                        if (item != null && item.template != null && item.template.id == 193)
                        {
                            Service.gI().useItem(0, 1, (sbyte)i, -1);
                            _lastTimeUsePetBuff = now;
                            break;
                        }
                    }
                }
            }
            catch { }
        }
    }
}
