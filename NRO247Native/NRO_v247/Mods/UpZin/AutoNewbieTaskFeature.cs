using System;
using Assets.src.g;
using NRO_v247.Mods.Xmap;

namespace NRO_v247.Mods.UpZin;

public class AutoNewbieTaskFeature : IAutoFeature
{
    protected bool _enabled;
    protected bool _isTanSat;
    private bool _isPicking;
    private long _lastTimePickedItem;
    protected bool _isHarvestingPean;
    private long _lastTimeEatPean;
    private long _lastTimeAutoPoint;
    private long _lastTimeCheckTN;
    private long _lastTN;
    private bool _isNhanBua;
    protected long _lastTimeRevive;
    private long _lastTimeCheckNV;
    private long _lastTimeHarvestMenu;
    
    protected int _myMinMP = 15;
    protected int _myMinHP = 15;
    protected int _minHPMob = 0;
    protected int _maxHPMob = int.MaxValue;
    protected int _minPeans = 0;

    // Tự mặc đồ tốt nhất khi up zin
    private long _lastAutoEquipMs;
    private const long AutoEquipIntervalMs = 2000L;
    private const sbyte BagBodyType = 4; // getItem(4, bagSlot) = mặc từ túi lên người

    protected bool _isNhapCodeTanThu;
    protected bool _isPKKarinSama;
    protected bool _isPKT77;
    protected bool _isTeleT77 = true;
    protected long _nv0MapReadyTime = 0L; // Thời điểm được phép xmap khi ở map 39-41 (chờ 2s)

    public virtual bool IsActive => _enabled;
    public virtual string CurrentState => _enabled ? BuildTaskProgressString() : "Tắt";

    private static string BuildTaskProgressString()
    {
        var task = Char.myCharz()?.taskMaint;
        if (task == null) return "Làm NV Tân thủ (...)";

        // Dạng: "NV Tân thủ: 0 - 1" hoặc "NV Tân thủ: 0 - 1 - 3" nếu counts có phần tử thứ 3
        string progress = $"{task.taskId} - {task.index}";
        if (task.counts != null && task.index < task.counts.Length)
            progress += $" - {task.count}/{task.counts[task.index]}";

        return $"NV Tân thủ: {progress}";
    }

    public bool IsUtilityTask => false;
    public virtual int Priority => 999;
    
    // Kích hoạt khi ở GameScr (hoặc ClientInput đang chờ nhập gifcode), có nhân vật, Task ID <= 11
    public virtual bool IsRequested => _enabled
        && (GameCanvas.currentScreen is GameScr || (_isNhapCodeTanThu && GameCanvas.currentScreen is ClientInput))
        && Char.myCharz() != null && Char.myCharz().taskMaint != null && ShouldRunTask();

    public void ApplySettingsFromPanel(bool enabled)
    {
        _enabled = enabled;
        if (!_enabled)
        {
            _isHarvestingPean = false;
            _isTanSat = false;
            ModBootstrap.TrainFeature.IsUpZinOverride = false;
            ModBootstrap.AutoPickFeature.IsUpZinOverride = false;
        }

    }

    public void DisableFromPanel()
    {
        _enabled = false;
        _isHarvestingPean = false;
        _isTanSat = false;
        ModBootstrap.TrainFeature.IsUpZinOverride = false;
        ModBootstrap.AutoPickFeature.IsUpZinOverride = false;
    }

    protected virtual bool ShouldRunTask()
    {
        var task = Char.myCharz().taskMaint;
        if (task == null) return false;
        
        // Dừng khi đã hoàn thành Nhiệm vụ 11
        if (task.taskId > 11) return false;
        
        return true;
    }

    protected bool IsXmapActing()
    {
        return ModBootstrap.XmapFeature != null && ModBootstrap.XmapFeature.IsXmaping();
    }

    protected void StartXmap(int mapId)
    {
        ModBootstrap.XmapFeature?.StartGoToMapFromPanel(mapId);
    }

    protected void TeleportMyChar(int x, int y)
    {
        var myChar = Char.myCharz();
        if (myChar == null) return;
        myChar.currentMovePoint = null;
        myChar.cx = x;
        myChar.cy = y;
        Service.gI().charMove();
        
        // Anti-cheat bypass (shake y)
        myChar.cy = y + 1;
        Service.gI().charMove();
        myChar.cy = y;
        Service.gI().charMove();
        
        myChar.cxSend = x;
        myChar.cySend = y;
    }

    protected void TeleportMyChar(Char myChar)
    {
        if (myChar != null) TeleportMyChar(myChar.cx, myChar.cy);
    }

    protected void TeleportMyChar(Mob mob)
    {
        if (mob != null) TeleportMyChar(mob.x, mob.y);
    }

    public virtual void Update()
    {
        if (!IsRequested)
        {
            return;
        }
        
        var myChar = Char.myCharz();
            
            // Xử lý nhập gifcode tân thủ khi server mở ClientInput dialog
            if (_isNhapCodeTanThu && GameCanvas.currentScreen is ClientInput)
            {
                // Server đã mở ô nhập text — điền gifcode và submit
                ClientInput ci = (ClientInput)GameCanvas.currentScreen;
                if (ci.tf != null && ci.tf.Length > 0)
                    ci.tf[0].setText("tan thu nro");
                TField tField = new TField();
                tField.setText("tan thu nro");
                Service.gI().sendClientInput(new TField[1] { tField });
                GameScr.gI().switchToMe();
                ClientInput.instance = null;
                Char.chatPopup = null;
                _isNhapCodeTanThu = false;
                return; // Không làm gì thêm frame này
            }

            // Detect menu "Nhận quà" / "Từ chối" từ server → set flag chờ ClientInput
            if (!_isNhapCodeTanThu && GameCanvas.currentScreen is GameScr
                && GameCanvas.menu.showMenu && GameCanvas.menu.menuItems != null && GameCanvas.menu.menuItems.size() == 2)
            {
                Command command1 = (Command)GameCanvas.menu.menuItems.elementAt(0);
                Command command2 = (Command)GameCanvas.menu.menuItems.elementAt(1);
                if (command1 != null && command2 != null && command1.caption != null && command2.caption != null)
                {
                    if (command1.caption.ToLower().Contains("nhận quà") && command2.caption.ToLower().Contains("từ chối"))
                    {
                        GameCanvas.menu.menuSelectedItem = 0;
                        command1.performAction(); // Server sẽ gửi ClientInput sau packet này
                        _isNhapCodeTanThu = true;
                        // KHÔNG doCloseMenu() — để server tự xử lý
                    }
                }
            }
            
            // Tự ăn đậu
            if (GameScr.hpPotion <= 0 && (myChar.cMP < 15 || myChar.cHP < 15))
            {
                if (!_isHarvestingPean)
                    _isHarvestingPean = true;
                _isTanSat = false;
                _isPKKarinSama = false;
                _isPKT77 = false;
                if (TileMap.mapID != myChar.cgender + 21 && !IsXmapActing())
                    StartXmap(myChar.cgender + 21);
            }
            
            if ((myChar.cMP < _myMinMP || myChar.cHP < _myMinHP) && !_isHarvestingPean && (TileMap.mapID != myChar.cgender + 21 || myChar.taskMaint.taskId < 3) && mSystem.currentTimeMillis() - _lastTimeEatPean > 2000 && (_minPeans <= 0 || GameScr.hpPotion >= _minPeans))
            {
                _lastTimeEatPean = mSystem.currentTimeMillis();
                GameScr.gI().doUseHP();
            }
            
            // Hồi sinh
            if ((myChar.isDie || myChar.cHP <= 0) && mSystem.currentTimeMillis() - _lastTimeRevive > 1000)
            {
                _lastTimeRevive = mSystem.currentTimeMillis();
                Service.gI().returnTownFromDead();
            }
                
            // Xử lý thu hoạch / nhận đậu ở nhà
            if (TileMap.mapID == myChar.cgender + 21)
            {
                if (!_isHarvestingPean && _minPeans > 0 && GameScr.hpPotion < _minPeans)
                    _isHarvestingPean = true;
                    
                if (GameScr.vItemMap.size() > 0)
                {
                    ItemMap itemMap = (ItemMap)GameScr.vItemMap.elementAt(0);
                    if (mSystem.currentTimeMillis() - _lastTimePickedItem >= 550)
                    {
                        _lastTimePickedItem = mSystem.currentTimeMillis();
                        Service.gI().pickItem(itemMap.itemMapID);
                    }
                }
                if (_minPeans <= 0)
                {
                    if (GameScr.gI().magicTree.currPeas == 0 || myChar.cgender == 1 && GameScr.hpPotion >= 30 || myChar.cgender != 1 && GameScr.hpPotion >= 20)
                        _isHarvestingPean = false;
                }
                else if (GameScr.hpPotion >= _minPeans)
                    _isHarvestingPean = false;
                    
                if (myChar.taskMaint.taskId >= 2 && GameScr.gI().magicTree.currPeas > 0 && (myChar.cgender == 1 && GameScr.hpPotion < 30 || myChar.cgender != 1 && GameScr.hpPotion < 20) && mSystem.currentTimeMillis() - _lastTimeHarvestMenu > 500)
                {
                    _lastTimeHarvestMenu = mSystem.currentTimeMillis();
                    Service.gI().openMenu(4);
                    Service.gI().confirmMenu(4, 0);
                }
                
                if (myChar.xu >= 5000 && GameScr.gI().magicTree.level == 1 && (GameScr.gI().magicTree.strInfo != "đang nâng cấp" || !GameScr.gI().magicTree.isUpdate) && mSystem.currentTimeMillis() - _lastTimeHarvestMenu > 1000)
                {
                    _lastTimeHarvestMenu = mSystem.currentTimeMillis();
                    Service.gI().openMenu(4);
                    Service.gI().confirmMenu(4, 1);
                    Service.gI().confirmMenu(5, 0);
                    GameScr.gI().magicTree.strInfo = "đang nâng cấp";
                    GameScr.gI().magicTree.isUpdate = true;
                    _isHarvestingPean = false;
                }
                if (GameCanvas.menu.showMenu)
                    GameCanvas.menu.doCloseMenu();
            }

            // Tự mặc đồ tốt nhất (type 0-4 so sánh level, type 5/24 mặc khi trống)
            if (!_isHarvestingPean && !IsXmapActing())
            {
                long nowEquip = mSystem.currentTimeMillis();
                if (nowEquip - _lastAutoEquipMs >= AutoEquipIntervalMs)
                {
                    _lastAutoEquipMs = nowEquip;
                    TryAutoEquipBestItems(myChar);
                }
            }

            if (!_isNhapCodeTanThu && !_isHarvestingPean && mSystem.currentTimeMillis() - _lastTimeCheckNV > 500 && myChar.cHP > 1 && !GameScr.gI().isBagFull())
            {
                _lastTimeCheckNV = mSystem.currentTimeMillis();
                AutoNV();
            }
                
            if (myChar.taskMaint.taskId > 3)
                AutoPoint();
                
            if (!IsXmapActing() && !_isNhapCodeTanThu && !_isHarvestingPean && myChar.cHP > 1 && !GameScr.gI().isBagFull() && (_minPeans <= 0 || GameScr.hpPotion >= _minPeans))
            {
                ModBootstrap.TrainFeature.IsUpZinOverride = _isTanSat;
                if (_isTanSat)
                {
                    ModBootstrap.TrainFeature.UpZinMinHp = _minHPMob;
                    ModBootstrap.TrainFeature.UpZinMaxHp = _maxHPMob;
                    ModBootstrap.TrainFeature.Update(); // Bắt buộc gọi Update vì TrainFeature bị mất lượt chạy độc quyền bên AutoMod
                }
                ModBootstrap.AutoPickFeature.IsUpZinOverride = true;

                if (_isPKKarinSama)
                    PKThanMeo();
                else if (_isPKT77)
                    PKT77();
            }
            else
            {
                ModBootstrap.TrainFeature.IsUpZinOverride = false;
                ModBootstrap.AutoPickFeature.IsUpZinOverride = false;
            }
        }

    protected void AutoPoint()
    {
        var myChar = Char.myCharz();
        if (mSystem.currentTimeMillis() - _lastTimeAutoPoint >= 1000)
        {
            _lastTimeAutoPoint = mSystem.currentTimeMillis();
            if ((myChar.cHPGoc < 400 || myChar.cDamGoc >= 40 && myChar.cHPGoc < 500) && myChar.cTiemNang > myChar.cHPGoc + 1000)
                Service.gI().upPotential(0, 1);
            else if (myChar.cMPGoc < 300 && myChar.cDamGoc >= 25 && myChar.cTiemNang > myChar.cMPGoc + 1000)
                Service.gI().upPotential(1, 1);
            else if (myChar.cDamGoc < 70 && myChar.cTiemNang > myChar.cDamGoc * 100)
                Service.gI().upPotential(2, 1);
        }
    }



    protected void TrainUntilMeStrongEnough(int maxHPmob)
    {
        var myChar = Char.myCharz();
        if (_myMinHP != 15) _myMinHP = 15;
        if (_myMinMP != 15) _myMinMP = 15;
        
        if (_isNhanBua && GameCanvas.menu.showMenu && TileMap.mapID == myChar.cgender + 42)
            GameCanvas.menu.doCloseMenu();
            
        if (!_isNhanBua)
        {
            if (TileMap.mapID != myChar.cgender + 42)
            {
                if (!IsXmapActing())
                {
                    _isTanSat = false;
                    StartXmap(myChar.cgender + 42);
                }
            }
            else
            {
                Npc npc = (Npc)GameScr.vNpc.elementAt(0);
                TeleportMyChar(npc.cx, npc.cy);
                if (Res.distance(myChar.cx, myChar.cy, npc.cx, npc.cy) <= 50)
                {
                    if (!GameCanvas.menu.showMenu)
                        Service.gI().openMenu(21);
                    else
                    {
                        Command command = (Command)GameCanvas.menu.menuItems.elementAt(0);
                        if (command.caption != null && command.caption.ToLower().Contains("miễn phí"))
                            Service.gI().confirmMenu(21, 0);
                        _isNhanBua = true;
                    }
                }
            }
        }
        else if (TileMap.mapID == 9 || TileMap.mapID == 3 || TileMap.mapID == 17)
        {
            if (_maxHPMob != maxHPmob) _maxHPMob = maxHPmob;
            if (_minHPMob != 0) _minHPMob = 0;
            if (!_isTanSat) _isTanSat = true;
        }
        else if (!IsXmapActing())
        {
            _isTanSat = false;
            if (myChar.cgender == 0) StartXmap(3);
            if (myChar.cgender == 1) StartXmap(9);
            if (myChar.cgender == 2) StartXmap(17);
        }
    }



    protected virtual void AutoNV()
    {
        var task = Char.myCharz().taskMaint;
        if (task == null) return;
        
        switch (task.taskId)
        {
            case 0: AutoNV0(); break;
            case 1: AutoNV1(); break;
            case 2: AutoNV2(); break;
            case 3: AutoNV3(); break;
            case 4:
            case 5:
            case 6: AutoNV4to6(); break;
            case 7: AutoNV7(); break;
            case 8: AutoNV8(); break;
            case 9: AutoNV9(); break;
            case 10: AutoNV10(); break;
            case 11: AutoNV11(); break;
        }
    }

    protected void AutoNV0()
    {
        if (TileMap.mapID >= 39 && TileMap.mapID <= 41)
        {
            long now = mSystem.currentTimeMillis();

            // Lần đầu vào map → khởi động timer 2 giây
            if (_nv0MapReadyTime == 0L)
            {
                _nv0MapReadyTime = now + 2000L;
                return;
            }

            // Chưa đủ 2s → đứng im chờ
            if (now < _nv0MapReadyTime)
                return;

            var myChar = Char.myCharz();
            Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(0);
            myChar.cy -= 30;
            Service.gI().charMove();

            TeleportMyChar(waypoint.maxX - 20, waypoint.maxY);
        }
        else
        {
            // Ra khỏi map 39-41 → reset timer để lần sau vào lại vẫn chờ đủ 2s
            if (_nv0MapReadyTime != 0L)
                _nv0MapReadyTime = 0L;

            if (TileMap.mapID >= 21 && TileMap.mapID <= 23)
            {
                if (Char.myCharz().taskMaint.index == 2)
                {
                    if (Char.myCharz().cgender == 0)
                        Service.gI().openMenu(0);
                    if (Char.myCharz().cgender == 1)
                        Service.gI().openMenu(2);
                    if (Char.myCharz().cgender == 2)
                        Service.gI().openMenu(1);
                }
                else if (Char.myCharz().taskMaint.index == 3)
                {
                    if (Char.myCharz().cgender == 0)
                    {
                        if (Math.Abs(Char.myCharz().cx - 85) <= 10 && Math.Abs(Char.myCharz().cy - 336) <= 10)
                            Service.gI().getItem(0, 0);
                        else
                            TeleportMyChar(85, 336);
                    }
                    if (Char.myCharz().cgender == 2)
                    {
                        if (Math.Abs(Char.myCharz().cx - 94) <= 10 && Math.Abs(Char.myCharz().cy - 336) <= 10)
                            Service.gI().getItem(0, 0);
                        else
                            TeleportMyChar(94, 336);
                    }
                    if (Char.myCharz().cgender == 1)
                    {
                        if (Math.Abs(Char.myCharz().cx - 638) <= 10 && Math.Abs(Char.myCharz().cy - 336) <= 10)
                            Service.gI().getItem(0, 0);
                        else
                            TeleportMyChar(638, 336);
                    }
                }
                else if (Char.myCharz().taskMaint.index == 4)
                {
                    Service.gI().openMenu(4);
                    Service.gI().confirmMenu(4, 0);
                }
                else if (Char.myCharz().taskMaint.index == 5)
                {
                    if (GameCanvas.menu.showMenu)
                        GameCanvas.menu.doCloseMenu();
                    if (Char.myCharz().cgender == 0)
                        Service.gI().openMenu(0);
                    if (Char.myCharz().cgender == 1)
                        Service.gI().openMenu(2);
                    if (Char.myCharz().cgender == 2)
                        Service.gI().openMenu(1);
                }
            }
        }
    }

    protected void AutoNV1()
    {
        var myChar = Char.myCharz();
        if (myChar.taskMaint.index == 0)
        {
            myChar.npcFocus = null;
            if (TileMap.mapID >= 21 && TileMap.mapID <= 23)
            {
                if (!IsXmapActing())
                    StartXmap(myChar.cgender * 7);
            }
            else if (TileMap.mapID == myChar.cgender * 7)
                _isTanSat = true;
        }
        else if (myChar.taskMaint.index == 1)
        {
            _isTanSat = false;
            myChar.mobFocus = null;
            myChar.itemFocus = null;
            myChar.charFocus = null;
            if (!IsXmapActing() && TileMap.mapID != myChar.cgender + 21)
                StartXmap(myChar.cgender + 21);
            else if (TileMap.mapID == myChar.cgender + 21)
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    protected void AutoNV2()
    {
        var myChar = Char.myCharz();
        if (myChar.taskMaint.index == 0)
        {
            if (TileMap.mapID != myChar.cgender * 7 + 1)
            {
                if (!IsXmapActing())
                {
                    _isTanSat = false;
                    StartXmap(myChar.cgender * 7 + 1);
                }
            }
            else if (!_isTanSat)
                _isTanSat = true;
        }
        else if (myChar.taskMaint.index == 1)
        {
            myChar.mobFocus = null;
            _isTanSat = false;
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing())
                    StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    protected void AutoNV3()
    {
        var myChar = Char.myCharz();
        if (myChar.taskMaint.index == 0)
            Service.gI().upPotential(2, 1);
        if (myChar.taskMaint.index == 1)
        {
            if (TileMap.mapID == myChar.cgender + 42)
            {
                if (myChar.cgender == 0) TeleportMyChar(149, 288);
                if (myChar.cgender == 1) TeleportMyChar(126, 264);
                if (myChar.cgender == 2) TeleportMyChar(156, 288);
            }
            else if (!IsXmapActing())
                StartXmap(myChar.cgender + 42);
        }
        else if (myChar.taskMaint.index == 2)
        {
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing())
                    StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    protected void AutoNV4to6()
    {
        var myChar = Char.myCharz();
        if (_myMinHP != 30) _myMinHP = 30;
        if (_myMinMP != 15) _myMinHP = 15;
        if (_maxHPMob != 500) _maxHPMob = 500;
        if (_minHPMob != 499) _minHPMob = 499;
        
        if (myChar.taskMaint.index < 3)
        {
            int mapID = 2 + myChar.cgender * 7;
            if (myChar.taskMaint.index == 1) 
            {
                if (myChar.cgender == 0) mapID = 9;
                else mapID = 2;
            }
            if (myChar.taskMaint.index == 2) 
            {
                if (myChar.cgender == 2) mapID = 9;
                else mapID = 16;
            } 
            if (TileMap.mapID == mapID)
                _isTanSat = true;
            else
            {
                _isTanSat = false;
                if (!IsXmapActing())
                    StartXmap(mapID);
            }
        }
        else
        {
            _isTanSat = false;
            _maxHPMob = int.MaxValue;
            _minHPMob = 0;
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing())
                    StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    protected void AutoNV7()
    {
        var myChar = Char.myCharz();
        if (myChar.cPower <= 78000 || myChar.taskMaint.index == 0)
            TrainUntilMeStrongEnough(200);
        else if (myChar.taskMaint.index == 1)
        {
            if (TileMap.mapID == 3 || TileMap.mapID == 11 || TileMap.mapID == 17)
            {
                if (_myMinHP != 45) _myMinHP = 45;
                if (_myMinMP != 15) _myMinHP = 15;
                if (_maxHPMob != 600) _maxHPMob = 600;
                if (_minHPMob != 599) _minHPMob = 599;
                _isTanSat = true;
            }
            else if (!IsXmapActing())
            {
                _isTanSat = false;
                if (myChar.cgender == 0) StartXmap(3);
                if (myChar.cgender == 1) StartXmap(11);
                if (myChar.cgender == 2) StartXmap(17);
            }
        }
        else if (myChar.taskMaint.index == 2)
        {
            _isTanSat = false;
            _maxHPMob = int.MaxValue;
            _minHPMob = 0;
            if (TileMap.mapID == myChar.cgender * 7)
                Service.gI().openMenu(myChar.cgender + 7);
            else if (!IsXmapActing())
                StartXmap(myChar.cgender * 7);
        }
        else if (myChar.taskMaint.index == 3)
        {
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing())
                    StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    protected void AutoNV8()
    {
        var myChar = Char.myCharz();
        if (myChar.cPower <= 140000 || myChar.taskMaint.index == 0)
            TrainUntilMeStrongEnough(500);
        else if (myChar.taskMaint.index == 1)
        {
            if (_myMinHP != 60) _myMinHP = 60;
            if (_myMinMP != 15) _myMinMP = 15;
            if (!IsXmapActing())
            {
                if (myChar.cgender == 0 && TileMap.mapID != 12)
                    StartXmap(12);
                else if (myChar.cgender == 1 && TileMap.mapID != 18)
                    StartXmap(18);
                else if (myChar.cgender == 2 && TileMap.mapID != 4)
                    StartXmap(4);
            }
            if (TileMap.mapID == 12 || TileMap.mapID == 18 || TileMap.mapID == 4)
            {
                _maxHPMob = 1000;
                _minHPMob = 999;
                if (!_isTanSat)
                    _isTanSat = true;
            }
        }
        else if (myChar.taskMaint.index == 2)
        {
            _isTanSat = false;
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing())
                    StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
        else if (myChar.taskMaint.index == 3 && TileMap.mapID != 47 && !IsXmapActing())
            StartXmap(47);
    }

    protected void AutoNV9()
    {
        var myChar = Char.myCharz();
        if (myChar.taskMaint.index <= 1)
        {
            if (TileMap.mapID == 47)
                Service.gI().openMenu(17);
            else if (!IsXmapActing())
                StartXmap(47);
        }
        else if (myChar.taskMaint.index == 2)
        {
            if (TileMap.mapID != 47)
            {
                if (!IsXmapActing())
                    StartXmap(47);
            }
            else
            {
                if (Math.Abs(myChar.cx - 600) >= 20)
                    TeleportMyChar(600, 336);
                else if (myChar.currentMovePoint == null || (myChar.currentMovePoint.xEnd != 600 && myChar.currentMovePoint.yEnd != 10))
                    myChar.currentMovePoint = new MovePoint(600, 10);
            }
        }
        else if (Char.myCharz().taskMaint.index == 3)
        {
            if (TileMap.mapID == 46)
            {

                if (GameCanvas.menu.showMenu && GameCanvas.menu.menuItems.size() == 1)
                {
                    Service.gI().openMenu(18);
                    GameCanvas.menu.doCloseMenu();
                }
                else
                    Service.gI().confirmMenu(18, 0);
            }
            else if (TileMap.mapID == 47)
            {
                if (Math.Abs(Char.myCharz().cx - 600) >= 20)
                    TeleportMyChar(600, 336);
                else if (Char.myCharz().currentMovePoint == null || (Char.myCharz().currentMovePoint.xEnd != 600 && Char.myCharz().currentMovePoint.yEnd != 10))
                    Char.myCharz().currentMovePoint = new MovePoint(600, 10);
            }
            else if (!IsXmapActing())
                StartXmap(47);
        }
    }

    protected void AutoNV10()
    {
        var myChar = Char.myCharz();
        _isPKKarinSama = false;
        _isPKT77 = false;
        _minPeans = myChar.taskMaint.index > 1 ? 0 : 7;
        
        if (myChar.taskMaint.index == 0)
        {
            if (TileMap.mapID == 46)
            {
                Npc karinSama = GameScr.findNPCInMap(18);
                if (karinSama == null || karinSama.isHide)
                    _isPKKarinSama = true;
                else
                {
                    if (myChar.cx != 421 || myChar.cy != 408)
                        TeleportMyChar(421, 408);
                    else
                    {
                        if (!GameCanvas.menu.showMenu)
                            Service.gI().openMenu(18);
                        else
                        {
                            bool foundAcc = false;
                            for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
                            {
                                Command cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                                if (cmd.caption != null && cmd.caption.ToLower().Contains("đồng ý"))
                                {
                                    Service.gI().confirmMenu(18, (sbyte)i);
                                    foundAcc = true;
                                    break;
                                }
                            }
                            if (!foundAcc)
                            {
                                for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
                                {
                                    Command cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                                    if (cmd.caption != null && cmd.caption.ToLower().Contains("thách đấu"))
                                    {
                                        Service.gI().confirmMenu(18, (sbyte)i);
                                        break;
                                    }
                                }
                            }
                            GameCanvas.menu.doCloseMenu();
                            Char.chatPopup = null;
                        }
                    }
                }
            }
            else
            {
                if (TileMap.mapID == 47)
                {
                    if (Math.Abs(myChar.cx - 600) >= 20)
                        TeleportMyChar(600, 336);
                    else if (myChar.currentMovePoint == null || (myChar.currentMovePoint.xEnd != 600 && myChar.currentMovePoint.yEnd != 10))
                        myChar.currentMovePoint = new MovePoint(600, 10);
                }
                else if (!IsXmapActing())
                    StartXmap(47);
            }
        }
        else if (myChar.taskMaint.index == 1)
        {
            if (TileMap.mapID == 46)
            {
                if (myChar.currentMovePoint == null || (myChar.currentMovePoint.xEnd != 576 && myChar.currentMovePoint.yEnd != 552))
                    myChar.currentMovePoint = new MovePoint(576, 552);
                _isTeleT77 = true;
            }
            else if (TileMap.mapID == 47)
            {
                if (_isTeleT77 && (myChar.cx != 371 || myChar.cy != 336))
                {
                    _isTeleT77 = false;
                    TeleportMyChar(371, 336);
                }
                else
                    _isPKT77 = true;
            }
            else
            {
                _isTeleT77 = true;
                if (!IsXmapActing())
                    StartXmap(47);
            }
        }
        else if (myChar.taskMaint.index == 2)
        {
            if (TileMap.mapID == 47)
                Service.gI().openMenu(17);
            else if (!IsXmapActing())
                StartXmap(47);
        }
        else if (myChar.taskMaint.index == 3)
        {
            if (TileMap.mapID != myChar.cgender + 21)
            {
                if (!IsXmapActing())
                    StartXmap(myChar.cgender + 21);
            }
            else
            {
                if (myChar.cgender == 0) Service.gI().openMenu(0);
                if (myChar.cgender == 1) Service.gI().openMenu(2);
                if (myChar.cgender == 2) Service.gI().openMenu(1);
            }
        }
    }

    protected void AutoNV11()
    {
        var myChar = Char.myCharz();
        if (IsXmapActing()) return;
        
        if (myChar.cgender == 0 && TileMap.mapID != 5)
            StartXmap(5);
        else if (myChar.cgender == 1 && TileMap.mapID != 13)
            StartXmap(13);
        else if (myChar.cgender == 2 && TileMap.mapID != 20)
            StartXmap(20);
        else if (TileMap.mapID == 5 || TileMap.mapID == 13 || TileMap.mapID == 20)
        {
            if (!GameCanvas.menu.showMenu)
                Service.gI().openMenu(13 + myChar.cgender);
            else
            {
                if (GameCanvas.menu.menuItems.size() > 0)
                {
                    Command command = (Command)GameCanvas.menu.menuItems.elementAt(0);
                    if (command.caption != null && (command.caption.ToLower().Contains("nói chuyện") || command.caption.ToLower().Contains("nhiệm vụ")))
                        command.performAction();
                }
                GameCanvas.menu.doCloseMenu();
                Char.chatPopup = null;
            }
        }
    }

    // ─── Auto Equip Best Item (chỉ dùng khi up zin) ─────────────────────
    /// <summary>
    /// Tự mặc đồ tốt nhất từ túi lên người trong lúc up zin:
    /// - Type 0–4 (áo/quần/găng/giày/rada): so sánh template.level — mặc nếu trống hoặc level trong túi cao hơn đang mặc.
    /// - Type 5 (tóc) và 24 (mount VIP): chỉ mặc khi slot body đang trống.
    /// Mỗi lần chỉ gửi 1 packet rồi return để tránh spam.
    /// </summary>
    private void TryAutoEquipBestItems(Char me)
    {
        if (me?.arrItemBag == null || me.arrItemBody == null) return;
        if (Char.ischangingMap || Controller.isStopReadMessage) return;

        // ── Type 0–4: so sánh template.level, ưu tiên level cao hơn ──────
        for (int targetType = 0; targetType <= 4; targetType++)
        {
            int bestBagSlot = -1;
            sbyte bestLevel = -1;

            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item bagItem = me.arrItemBag[i];
                if (bagItem?.template == null || bagItem.quantity <= 0) continue;
                if (bagItem.template.type != targetType) continue;
                if (bagItem.template.level > bestLevel)
                {
                    bestLevel = bagItem.template.level;
                    bestBagSlot = i;
                }
            }

            if (bestBagSlot < 0) continue;

            Item bodyItem = (targetType < me.arrItemBody.Length) ? me.arrItemBody[targetType] : null;

            if (bodyItem?.template == null)
            {
                // Slot trống → mặc ngay
                Service.gI().getItem(BagBodyType, (sbyte)bestBagSlot);
                return;
            }

            if (bestLevel > bodyItem.template.level)
            {
                // Level trong túi tốt hơn → đổi
                Service.gI().getItem(BagBodyType, (sbyte)bestBagSlot);
                return;
            }
        }

        // ── Type 5 (tóc) và 24 (mount VIP): chỉ mặc khi slot trống ──────
        int[] simpleTypes = { Item.TYPE_HAIR, Item.TYPE_MOUNT_VIP }; // 5, 24
        foreach (int equipType in simpleTypes)
        {
            Item bodySlot = (equipType < me.arrItemBody.Length) ? me.arrItemBody[equipType] : null;
            if (bodySlot?.template != null) continue;

            for (int i = 0; i < me.arrItemBag.Length; i++)
            {
                Item bagItem = me.arrItemBag[i];
                if (bagItem?.template == null || bagItem.quantity <= 0) continue;
                if (bagItem.template.type != equipType) continue;
                Service.gI().getItem(BagBodyType, (sbyte)i);
                return;
            }
        }
    }

    private void PKThanMeo()
    {
        var myChar = Char.myCharz();
        if (_myMinHP != 60) _myMinHP = 60;
        if (_myMinMP != 20) _myMinMP = 20;
        Skill mySkill = myChar.myskill;
        if (_isPicking || mSystem.currentTimeMillis() - mySkill.lastTimeUseThisSkill <= mySkill.coolDown + 100L)
            return;
            
        for (int i = 0; i < GameScr.vCharInMap.size(); i++)
        {
            Char ch = (Char)GameScr.vCharInMap.elementAt(i);
            if (ch.cName != "Karin" || ch.cTypePk != 3)
                continue;
                
            myChar.cx = ch.cx + Res.random(-5, 5);
            myChar.cy = ch.cy;
            Service.gI().charMove();
            if (Res.distance(myChar.cx, myChar.cy, ch.cx, ch.cy) <= 50)
            {
                mySkill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
                myChar.charFocus = ch;
                MyVector myVector = new MyVector();
                myVector.addElement(ch);
                Service.gI().sendPlayerAttack(new MyVector(), myVector, -1);
            }
            break;
        }
    }

    private void PKT77()
    {
        var myChar = Char.myCharz();
        if (_myMinHP != 100) _myMinHP = 100;
        if (_myMinMP != 20) _myMinMP = 20;
        Skill mySkill = myChar.myskill;
        if (_isPicking || mSystem.currentTimeMillis() - mySkill.lastTimeUseThisSkill <= mySkill.coolDown + 100L)
            return;
            
        for (int i = 0; i < GameScr.vCharInMap.size(); i++)
        {
            Char ch = (Char)GameScr.vCharInMap.elementAt(i);
            if (ch.cTypePk != 3) continue; 
            if (ch.cName == null || (!ch.cName.ToLower().Contains("tàu") && ch.cName != "Tàu Pảy Pảy")) continue;
                
            myChar.cx = ch.cx + Res.random(-5, 5);
            myChar.cy = ch.cy;
            Service.gI().charMove();
            if (Res.distance(myChar.cx, myChar.cy, ch.cx, ch.cy) <= 50)
            {
                mySkill.lastTimeUseThisSkill = mSystem.currentTimeMillis();
                myChar.charFocus = ch;
                MyVector myVector = new MyVector();
                myVector.addElement(ch);
                Service.gI().sendPlayerAttack(new MyVector(), myVector, -1);
            }
            break;
        }
    }
}
