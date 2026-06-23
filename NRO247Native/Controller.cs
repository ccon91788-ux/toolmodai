#define DEBUG
using Assets.src.e;
using Assets.src.f;
using Assets.src.g;
using System;
using System.Diagnostics;

public class Controller : IMessageHandler
{
    protected static Controller me;

    protected static Controller me2;

    public Message messWait;

    public static bool isLoadingData = false;

    public static bool isConnectOK;

    public static bool isConnectionFail;

    public static bool isDisconnected;

    public static bool isMain;

    private float demCount;

    private int move;

    private int total;

    public static bool isStopReadMessage;

    public static bool isGet_CLIENT_INFO = false;

    public static MyHashTable frameHT_NEWBOSS = new MyHashTable();

    public const sbyte PHUBAN_TYPE_CHIENTRUONGNAMEK = 0;

    public const sbyte PHUBAN_START = 0;

    public const sbyte PHUBAN_UPDATE_POINT = 1;

    public const sbyte PHUBAN_END = 2;

    public const sbyte PHUBAN_LIFE = 4;

    public const sbyte PHUBAN_INFO = 5;

    public static bool isEXTRA_LINK = false;

    public static Controller gI()
    {
        if (me == null)
        {
            me = new Controller();
        }
        return me;
    }

    public static Controller gI2()
    {
        if (me2 == null)
        {
            me2 = new Controller();
        }
        return me2;
    }

    public void onConnectOK(bool isMain1)
    {
        isMain = isMain1;
        mSystem.onConnectOK();
    }

    public void onConnectionFail(bool isMain1)
    {
        isMain = isMain1;
        mSystem.onConnectionFail();
    }

    public void onDisconnected(bool isMain1)
    {
        isMain = isMain1;
        mSystem.onDisconnected();
    }

    public void requestItemPlayer(Message msg)
    {
        try
        {
            int num = msg.reader().readUnsignedByte();
            Item item = GameScr.currentCharViewInfo.arrItemBody[num];
            item.saleCoinLock = msg.reader().readInt();
            item.sys = msg.reader().readByte();
            item.options = new MyVector();
            try
            {
                while (true)
                {
                    ItemOption itemOption = readItemOption(msg);
                    if (itemOption != null)
                    {
                        item.options.addElement(itemOption);
                    }
                }
            }
            catch (Exception ex)
            {
                Cout.println("Loi tairequestItemPlayer 1" + ex.ToString());
            }
        }
        catch (Exception ex2)
        {
            Cout.println("Loi tairequestItemPlayer 2" + ex2.ToString());
        }
    }

    public void onMessage(Message msg)
    {
        GameCanvas.debugSession.removeAllElements();
        GameCanvas.debug("SA1", 2);
        try
        {
            if (msg.command != -74)
            {
                Res.outz("=========> [READ] cmd= " + msg.command);
            }
            Char @char = null;
            Mob mob = null;
            MyVector myVector = new MyVector();
            int num = 0;
            GameCanvas.timeLoading = 15;
            Controller2.readMessage(msg);
            switch (msg.command)
            {
                case 12:
                    read_cmdExtraBig(msg);
                    break;
                case 0:
                    readLogin(msg);
                    break;
                case 24:
                    read_cmdExtra(msg);
                    break;
                case 20:
                    phuban_Info(msg);
                    break;
                case 66:
                    readGetImgByName(msg);
                    break;
                case 65:
                    {
                        sbyte b61 = msg.reader().readSByte();
                        string text7 = msg.reader().readUTF();
                        short num67 = msg.reader().readShort();
                        // Disabled msg 65 (text time display)
                        break;
                    }
                case 112:
                    {
                        sbyte b52 = msg.reader().readByte();
                        Res.outz("spec type= " + b52);
                        switch (b52)
                        {
                            case 0:
                                PanelG.spearcialImage = msg.reader().readShort();
                                PanelG.specialInfo = msg.reader().readUTF();
                                break;
                            case 1:
                                {
                                    sbyte b53 = msg.reader().readByte();
                                    Char.myCharz().infoSpeacialSkill = new string[b53][];
                                    Char.myCharz().imgSpeacialSkill = new short[b53][];
                                    GameCanvas.panel.speacialTabName = new string[b53][];
                                    for (int num39 = 0; num39 < b53; num39++)
                                    {
                                        GameCanvas.panel.speacialTabName[num39] = new string[2];
                                        string[] array3 = Res.split(msg.reader().readUTF(), "\n", 0);
                                        if (array3.Length == 2)
                                        {
                                            GameCanvas.panel.speacialTabName[num39] = array3;
                                        }
                                        if (array3.Length == 1)
                                        {
                                            GameCanvas.panel.speacialTabName[num39][0] = array3[0];
                                            GameCanvas.panel.speacialTabName[num39][1] = string.Empty;
                                        }
                                        int num40 = msg.reader().readByte();
                                        Char.myCharz().infoSpeacialSkill[num39] = new string[num40];
                                        Char.myCharz().imgSpeacialSkill[num39] = new short[num40];
                                        for (int num41 = 0; num41 < num40; num41++)
                                        {
                                            Char.myCharz().imgSpeacialSkill[num39][num41] = msg.reader().readShort();
                                            Char.myCharz().infoSpeacialSkill[num39][num41] = msg.reader().readUTF();
                                        }
                                    }
                                    GameCanvas.panel.tabName[25] = GameCanvas.panel.speacialTabName;
                                    GameCanvas.panel.setTypeSpeacialSkill();
                                    GameCanvas.panel.show();
                                    break;
                                }
                        }
                        break;
                    }
                case -98:
                    {
                        sbyte b62 = msg.reader().readByte();
                        GameCanvas.menu.showMenu = false;
                        if (b62 == 0)
                        {
                            GameCanvas.startYesNoDlg(msg.reader().readUTF(), new Command(mResources.YES, GameCanvas.instance, 888397, msg.reader().readUTF()), new Command(mResources.NO, GameCanvas.instance, 888396, null));
                        }
                        break;
                    }
                case -97:
                    Char.myCharz().cNangdong = msg.reader().readInt();
                    break;
                case -96:
                    {
                        sbyte typeTop = msg.reader().readByte();
                        GameCanvas.panel.vTop.removeAllElements();
                        string topName = msg.reader().readUTF();
                        sbyte b41 = msg.reader().readByte();
                        for (int num20 = 0; num20 < b41; num20++)
                        {
                            int rank = msg.reader().readInt();
                            int pId = msg.reader().readInt();
                            short headID = msg.reader().readShort();
                            short headICON = msg.reader().readShort();
                            short body = msg.reader().readShort();
                            short leg = msg.reader().readShort();
                            string name = msg.reader().readUTF();
                            string info3 = msg.reader().readUTF();
                            TopInfo topInfo = new TopInfo();
                            topInfo.rank = rank;
                            topInfo.headID = headID;
                            topInfo.headICON = headICON;
                            topInfo.body = body;
                            topInfo.leg = leg;
                            topInfo.name = name;
                            topInfo.info = info3;
                            topInfo.info2 = msg.reader().readUTF();
                            topInfo.pId = pId;
                            GameCanvas.panel.vTop.addElement(topInfo);
                        }
                        GameCanvas.panel.topName = topName;
                        GameCanvas.panel.setTypeTop(typeTop);
                        GameCanvas.panel.show();
                        break;
                    }
                case -94:
                    while (msg.reader().available() > 0)
                    {
                        short num200 = msg.reader().readShort();
                        int num3 = msg.reader().readInt();
                        for (int num4 = 0; num4 < Char.myCharz().vSkill.size(); num4++)
                        {
                            Skill skill = (Skill)Char.myCharz().vSkill.elementAt(num4);
                            if (skill != null && skill.skillId == num200)
                            {
                                if (num3 < skill.coolDown)
                                {
                                    skill.lastTimeUseThisSkill = mSystem.currentTimeMillis() - (skill.coolDown - num3);
                                }
                                Res.outz("1 chieu id= " + skill.template.id + " cooldown= " + num3 + "curr cool down= " + skill.coolDown);
                            }
                        }
                    }
                    break;
                case -95:
                    {
                        sbyte b54 = msg.reader().readByte();
                        Res.outz("type= " + b54);
                        if (b54 == 0)
                        {
                            sbyte mobCount = msg.reader().readByte();
                            for (int i = 0; i < mobCount; i++)
                            {
                                int num190 = msg.reader().readInt();
                                short templateId = msg.reader().readShort();
                                long num2 = msg.reader().readLong();
                                SoundMn.gI().explode_1();
                                if (num190 == Char.myCharz().charID)
                                {
                                    Char.myCharz().mobMe = new Mob(num190, isDisable: false, isDontMove: false, isFire: false, isIce: false, isWind: false, templateId, 1, num2, 0, num2, (short)(Char.myCharz().cx + ((Char.myCharz().cdir != 1) ? (-40) : 40)), (short)Char.myCharz().cy, 4, 0);
                                    Char.myCharz().mobMe.isMobMe = true;
                                    EffecMn.addEff(new Effect(18, Char.myCharz().mobMe.x, Char.myCharz().mobMe.y, 2, 10, -1));
                                    Char.myCharz().tMobMeBorn = 30;
                                    GameScr.vMob.addElement(Char.myCharz().mobMe);
                                }
                                else
                                {
                                    @char = GameScr.findCharInMap(num190);
                                    if (@char != null)
                                    {
                                        Mob mob3 = new Mob(num190, isDisable: false, isDontMove: false, isFire: false, isIce: false, isWind: false, templateId, 1, num2, 0, num2, (short)@char.cx, (short)@char.cy, 4, 0);
                                        mob3.isMobMe = true;
                                        @char.mobMe = mob3;
                                        GameScr.vMob.addElement(@char.mobMe);
                                    }
                                    else
                                    {
                                        Mob mob4 = GameScr.findMobInMap(num190);
                                        if (mob4 == null)
                                        {
                                            mob4 = new Mob(num190, isDisable: false, isDontMove: false, isFire: false, isIce: false, isWind: false, templateId, 1, num2, 0, num2, -100, -100, 4, 0);
                                            mob4.isMobMe = true;
                                            GameScr.vMob.addElement(mob4);
                                        }
                                    }
                                }
                            }
                        }
                        if (b54 == 1)
                        {
                            int num13 = msg.reader().readInt();
                            int mobId = msg.reader().readByte();
                            Res.outz("mod attack id= " + num13);
                            if (num13 == Char.myCharz().charID)
                            {
                                if (GameScr.findMobInMap(mobId) != null)
                                {
                                    Char.myCharz().mobMe.attackOtherMob(GameScr.findMobInMap(mobId));
                                }
                            }
                            else
                            {
                                @char = GameScr.findCharInMap(num13);
                                if (@char != null && GameScr.findMobInMap(mobId) != null)
                                {
                                    @char.mobMe.attackOtherMob(GameScr.findMobInMap(mobId));
                                }
                            }
                        }
                        if (b54 == 2)
                        {
                            int num24 = msg.reader().readInt();
                            int num35 = msg.reader().readInt();
                            long num46 = msg.reader().readLong();
                            long cHPNew = msg.reader().readLong();
                            if (num24 == Char.myCharz().charID)
                            {
                                Res.outz("mob dame= " + num46);
                                @char = GameScr.findCharInMap(num35);
                                if (@char != null)
                                {
                                    @char.cHPNew = cHPNew;
                                    if (Char.myCharz().mobMe.isBusyAttackSomeOne)
                                    {
                                        @char.doInjure(num46, 0L, isCrit: false, isMob: true);
                                    }
                                    else
                                    {
                                        Char.myCharz().mobMe.dame = num46;
                                        Char.myCharz().mobMe.setAttack(@char);
                                    }
                                }
                            }
                            else
                            {
                                mob = GameScr.findMobInMap(num24);
                                if (mob != null)
                                {
                                    if (num35 == Char.myCharz().charID)
                                    {
                                        Char.myCharz().cHPNew = cHPNew;
                                        if (mob.isBusyAttackSomeOne)
                                        {
                                            Char.myCharz().doInjure(num46, 0L, isCrit: false, isMob: true);
                                        }
                                        else
                                        {
                                            mob.dame = num46;
                                            mob.setAttack(Char.myCharz());
                                        }
                                    }
                                    else
                                    {
                                        @char = GameScr.findCharInMap(num35);
                                        if (@char != null)
                                        {
                                            @char.cHPNew = cHPNew;
                                            if (mob.isBusyAttackSomeOne)
                                            {
                                                @char.doInjure(num46, 0L, isCrit: false, isMob: true);
                                            }
                                            else
                                            {
                                                mob.dame = num46;
                                                mob.setAttack(@char);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (b54 == 3)
                        {
                            int num57 = msg.reader().readInt();
                            int mobId2 = msg.reader().readInt();
                            long hp = msg.reader().readLong();
                            long num68 = msg.reader().readLong();
                            @char = null;
                            @char = ((Char.myCharz().charID != num57) ? GameScr.findCharInMap(num57) : Char.myCharz());
                            if (@char != null)
                            {
                                mob = GameScr.findMobInMap(mobId2);
                                if (@char.mobMe != null)
                                {
                                    @char.mobMe.attackOtherMob(mob);
                                }
                                if (mob != null)
                                {
                                    mob.hp = hp;
                                    mob.updateHp_bar();
                                    if (num68 == 0)
                                    {
                                        mob.x = mob.xFirst;
                                        mob.y = mob.yFirst;
                                        GameScr.startFlyText(mResources.miss, mob.x, mob.y - mob.h, 0, -2, mFont.MISS);
                                    }
                                    else
                                    {
                                        GameScr.startFlyText("-" + num68, mob.x, mob.y - mob.h, 0, -2, mFont.ORANGE);
                                    }
                                }
                            }
                        }
                        if (b54 == 4)
                        {
                        }
                        if (b54 == 5)
                        {
                            int num79 = msg.reader().readInt();
                            sbyte b64 = msg.reader().readByte();
                            int mobId3 = msg.reader().readInt();
                            long num91 = msg.reader().readLong();
                            long hp2 = msg.reader().readLong();
                            @char = null;
                            @char = ((num79 != Char.myCharz().charID) ? GameScr.findCharInMap(num79) : Char.myCharz());
                            if (@char == null)
                            {
                                return;
                            }
                            if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
                            {
                                @char.setSkillPaint(GameScr.sks[b64], 0);
                            }
                            else
                            {
                                @char.setSkillPaint(GameScr.sks[b64], 1);
                            }
                            Mob mob5 = GameScr.findMobInMap(mobId3);
                            if (@char.cx <= mob5.x)
                            {
                                @char.cdir = 1;
                            }
                            else
                            {
                                @char.cdir = -1;
                            }
                            @char.mobFocus = mob5;
                            mob5.hp = hp2;
                            mob5.updateHp_bar();
                            GameCanvas.debug("SA83v2", 2);
                            if (num91 == 0)
                            {
                                mob5.x = mob5.xFirst;
                                mob5.y = mob5.yFirst;
                                GameScr.startFlyText(mResources.miss, mob5.x, mob5.y - mob5.h, 0, -2, mFont.MISS);
                            }
                            else
                            {
                                GameScr.startFlyText("-" + num91, mob5.x, mob5.y - mob5.h, 0, -2, mFont.ORANGE);
                            }
                        }
                        if (b54 == 6)
                        {
                            int num102 = msg.reader().readInt();
                            if (num102 == Char.myCharz().charID)
                            {
                                Char.myCharz().mobMe.startDie();
                            }
                            else
                            {
                                GameScr.findCharInMap(num102)?.mobMe.startDie();
                            }
                        }
                        if (b54 != 7)
                        {
                            break;
                        }
                        int num109 = msg.reader().readInt();
                        if (num109 == Char.myCharz().charID)
                        {
                            Char.myCharz().mobMe = null;
                            for (int i = 0; i < GameScr.vMob.size(); i++)
                            {
                                if (((Mob)GameScr.vMob.elementAt(i)).mobId == num109)
                                {
                                    GameScr.vMob.removeElementAt(i);
                                }
                            }
                            break;
                        }
                        @char = GameScr.findCharInMap(num109);
                        for (int j = 0; j < GameScr.vMob.size(); j++)
                        {
                            if (((Mob)GameScr.vMob.elementAt(j)).mobId == num109)
                            {
                                GameScr.vMob.removeElementAt(j);
                            }
                        }
                        if (@char != null)
                        {
                            @char.mobMe = null;
                        }
                        break;
                    }
                case -92:
                    Main.typeClient = msg.reader().readByte();
                    if (Rms.loadRMSString("ResVersion") == null)
                    {
                        Rms.clearAll();
                    }
                    Rms.saveRMSInt("clienttype", Main.typeClient);
                    Rms.saveRMSInt("lastZoomlevel", 1);
                    if (Rms.loadRMSString("ResVersion") == null)
                    {
                        GameCanvas.startOK(mResources.plsRestartGame, 8885, null);
                    }
                    break;
                case -91:
                    {
                        sbyte b34 = msg.reader().readByte();
                        GameCanvas.panel.mapNames = new string[b34];
                        GameCanvas.panel.planetNames = new string[b34];
                        for (int num199 = 0; num199 < b34; num199++)
                        {
                            GameCanvas.panel.mapNames[num199] = msg.reader().readUTF();
                            GameCanvas.panel.planetNames[num199] = msg.reader().readUTF();
                        }
                        GameCanvas.panel.setTypeMapTrans();
                        GameCanvas.panel.show();
                        break;
                    }
                case -90:
                    {
                        sbyte b63 = msg.reader().readByte();
                        int num72 = msg.reader().readInt();
                        Res.outz("===> UPDATE_BODY:    type = " + b63);
                        @char = ((Char.myCharz().charID != num72) ? GameScr.findCharInMap(num72) : Char.myCharz());
                        if (b63 != -1)
                        {
                            short num73 = msg.reader().readShort();
                            short num74 = msg.reader().readShort();
                            short num75 = msg.reader().readShort();
                            sbyte isMonkey = msg.reader().readByte();
                            if (@char != null)
                            {
                                if (@char.charID == num72)
                                {
                                    @char.isMask = true;
                                    @char.isMonkey = isMonkey;
                                    if (@char.isMonkey != 0)
                                    {
                                        @char.isWaitMonkey = false;
                                        @char.isLockMove = false;
                                    }
                                }
                                else if (@char != null)
                                {
                                    @char.isMask = true;
                                    @char.isMonkey = isMonkey;
                                }
                                if (num73 != -1)
                                {
                                    @char.head = num73;
                                }
                                if (num74 != -1)
                                {
                                    @char.body = num74;
                                }
                                if (num75 != -1)
                                {
                                    @char.leg = num75;
                                }
                            }
                        }
                        if (b63 == -1 && @char != null)
                        {
                            @char.isMask = false;
                            @char.isMonkey = 0;
                        }
                        if (@char == null)
                        {
                            break;
                        }
                        for (int num76 = 0; num76 < 54; num76++)
                        {
                            @char.removeEffChar(0, 201 + num76);
                        }
                        if (@char.bag >= 201 && @char.bag < 255)
                        {
                            Effect effect2 = new Effect(@char.bag, @char, 2, -1, 10, 1);
                            effect2.typeEff = 5;
                            @char.addEffChar(effect2);
                        }
                        if (@char.bag == 30 && @char.me)
                        {
                            GameScr.isPickNgocRong = true;
                        }
                        if (!@char.me)
                        {
                            break;
                        }
                        GameScr.isudungCapsun4 = false;
                        GameScr.isudungCapsun3 = false;
                        for (int num77 = 0; num77 < Char.myCharz().arrItemBag.Length; num77++)
                        {
                            Item item4 = Char.myCharz().arrItemBag[num77];
                            if (item4 == null)
                            {
                                continue;
                            }
                            if (item4.template.id == 194)
                            {
                                GameScr.isudungCapsun4 = item4.quantity > 0;
                                if (GameScr.isudungCapsun4)
                                {
                                    break;
                                }
                            }
                            else if (item4.template.id == 193)
                            {
                                GameScr.isudungCapsun3 = item4.quantity > 0;
                            }
                        }
                        break;
                    }
                case -88:
                    GameCanvas.endDlg();
                    GameCanvas.serverScreen.switchToMe();
                    break;
                case -87:
                    {
                        Res.outz("GET UPDATE_DATA " + msg.reader().available() + " bytes");
                        msg.reader().mark(500000);
                        createData(msg.reader(), isSaveRMS: true);
                        msg.reader().reset();
                        sbyte[] data = new sbyte[msg.reader().available()];
                        msg.reader().readFully(ref data);
                        sbyte[] data2 = new sbyte[1] { GameScr.vcData };
                        Rms.saveRMS("NRdataVersion", data2);
                        LoginScr.isUpdateData = false;
                        GameScr.gI().readOk();
                        break;
                    }
                case -86:
                    {
                        sbyte b35 = msg.reader().readByte();
                        Res.outz("server gui ve giao dich action = " + b35);
                        if (b35 == 0)
                        {
                            int playerID = msg.reader().readInt();
                            GameScr.gI().giaodich(playerID);
                        }
                        if (b35 == 1)
                        {
                            int num11 = msg.reader().readInt();
                            Char char13 = GameScr.findCharInMap(num11);
                            if (char13 == null)
                            {
                                return;
                            }
                            GameCanvas.panel.setTypeGiaoDich(char13);
                            GameCanvas.panel.show();
                            Service.gI().getPlayerMenu(num11);
                        }
                        if (b35 == 2)
                        {
                            sbyte b36 = msg.reader().readByte();
                            for (int num12 = 0; num12 < GameCanvas.panel.vMyGD.size(); num12++)
                            {
                                Item item2 = (Item)GameCanvas.panel.vMyGD.elementAt(num12);
                                if (item2.indexUI == b36)
                                {
                                    GameCanvas.panel.vMyGD.removeElement(item2);
                                    break;
                                }
                            }
                        }
                        if (b35 == 5)
                        {
                        }
                        if (b35 == 6)
                        {
                            GameCanvas.panel.isFriendLock = true;
                            if (GameCanvas.panel2 != null)
                            {
                                GameCanvas.panel2.isFriendLock = true;
                            }
                            GameCanvas.panel.vFriendGD.removeAllElements();
                            if (GameCanvas.panel2 != null)
                            {
                                GameCanvas.panel2.vFriendGD.removeAllElements();
                            }
                            int friendMoneyGD = msg.reader().readInt();
                            sbyte b37 = msg.reader().readByte();
                            Res.outz("item size = " + b37);
                            for (int num14 = 0; num14 < b37; num14++)
                            {
                                Item item3 = new Item();
                                item3.template = ItemTemplates.get(msg.reader().readShort());
                                item3.quantity = msg.reader().readInt();
                                int num15 = msg.reader().readUnsignedByte();
                                if (num15 != 0)
                                {
                                    item3.itemOption = new ItemOption[num15];
                                    for (int num16 = 0; num16 < item3.itemOption.Length; num16++)
                                    {
                                        ItemOption itemOption5 = readItemOption(msg);
                                        if (itemOption5 != null)
                                        {
                                            item3.itemOption[num16] = itemOption5;
                                            item3.compare = GameCanvas.panel.getCompare(item3);
                                        }
                                    }
                                }
                                if (GameCanvas.panel2 != null)
                                {
                                    GameCanvas.panel2.vFriendGD.addElement(item3);
                                }
                                else
                                {
                                    GameCanvas.panel.vFriendGD.addElement(item3);
                                }
                            }
                            if (GameCanvas.panel2 != null)
                            {
                                GameCanvas.panel2.setTabGiaoDich(isMe: false);
                                GameCanvas.panel2.friendMoneyGD = friendMoneyGD;
                            }
                            else
                            {
                                GameCanvas.panel.friendMoneyGD = friendMoneyGD;
                                if (GameCanvas.panel.currentTabIndex == 2)
                                {
                                    GameCanvas.panel.setTabGiaoDich(isMe: false);
                                }
                            }
                        }
                        if (b35 == 7)
                        {
                            InfoDlg.hide();
                            if (GameCanvas.panel.isShow)
                            {
                                GameCanvas.panel.hide();
                            }
                        }
                        break;
                    }
                case -85:
                    {
                        Res.outz("CAP CHAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                        sbyte b24 = msg.reader().readByte();
                        if (b24 == 0)
                        {
                            int num164 = msg.reader().readUnsignedShort();
                            Res.outz("lent =" + num164);
                            sbyte[] data3 = new sbyte[num164];
                            msg.reader().read(ref data3, 0, num164);
                            GameScr.imgCapcha = Image.createImage(data3, 0, num164);
                            GameScr.gI().keyInput = "-----";
                            GameScr.gI().strCapcha = msg.reader().readUTF();
                            GameScr.gI().keyCapcha = new int[GameScr.gI().strCapcha.Length];
                            GameScr.gI().mobCapcha = new Mob();
                            GameScr.gI().right = null;
                        }
                        if (b24 == 1)
                        {
                            MobCapcha.isAttack = true;
                        }
                        if (b24 == 2)
                        {
                            MobCapcha.explode = true;
                            GameScr.gI().right = GameScr.gI().cmdFocus;
                        }
                        break;
                    }
                case -112:
                    {
                        sbyte b39 = msg.reader().readByte();
                        if (b39 == 0)
                        {
                            sbyte mobIndex = msg.reader().readByte();
                            GameScr.findMobInMap(mobIndex).clearBody();
                        }
                        if (b39 == 1)
                        {
                            sbyte mobIndex2 = msg.reader().readByte();
                            GameScr.findMobInMap(mobIndex2).setBody(msg.reader().readShort());
                        }
                        break;
                    }
                case -84:
                    {
                        int index = msg.reader().readUnsignedByte();
                        Mob mob6 = null;
                        try
                        {
                            mob6 = (Mob)GameScr.vMob.elementAt(index);
                        }
                        catch (Exception)
                        {
                        }
                        if (mob6 != null)
                        {
                            mob6.maxHp = msg.reader().readLong();
                        }
                        break;
                    }
                case -83:
                    {
                        sbyte b30 = msg.reader().readByte();
                        if (b30 == 0)
                        {
                            int num188 = msg.reader().readShort();
                            int bgRID = msg.reader().readShort();
                            int num189 = msg.reader().readUnsignedByte();
                            int num191 = msg.reader().readInt();
                            string text2 = msg.reader().readUTF();
                            int num192 = msg.reader().readShort();
                            int num193 = msg.reader().readShort();
                            sbyte b31 = msg.reader().readByte();
                            if (b31 == 1)
                            {
                                GameScr.gI().isRongNamek = true;
                            }
                            else
                            {
                                GameScr.gI().isRongNamek = false;
                            }
                            GameScr.gI().xR = num192;
                            GameScr.gI().yR = num193;
                            Res.outz("xR= " + num192 + " yR= " + num193 + " +++++++++++++++++++++++++++++++++++++++");
                            if (Char.myCharz().charID == num191)
                            {
                                GameCanvas.panel.hideNow();
                                GameScr.gI().activeRongThanEff(isMe: true);
                            }
                            else if (TileMap.mapID == num188 && TileMap.zoneID == num189)
                            {
                                GameScr.gI().activeRongThanEff(isMe: false);
                            }
                            GameScr.gI().mapRID = num188;
                            GameScr.gI().bgRID = bgRID;
                            GameScr.gI().zoneRID = num189;
                        }
                        if (b30 == 1)
                        {
                            Res.outz("map RID = " + GameScr.gI().mapRID + " zone RID= " + GameScr.gI().zoneRID);
                            Res.outz("map ID = " + TileMap.mapID + " zone ID= " + TileMap.zoneID);
                            if (TileMap.mapID == GameScr.gI().mapRID && TileMap.zoneID == GameScr.gI().zoneRID)
                            {
                                GameScr.gI().hideRongThanEff();
                            }
                            else
                            {
                                GameScr.gI().isRongThanXuatHien = false;
                                if (GameScr.gI().isRongNamek)
                                {
                                    GameScr.gI().isRongNamek = false;
                                }
                            }
                        }
                        if (b30 == 2)
                        {
                        }
                        break;
                    }
                case -82:
                    {
                        sbyte b20 = msg.reader().readByte();
                        TileMap.tileIndex = new int[b20][][];
                        TileMap.tileType = new int[b20][];
                        for (int num161 = 0; num161 < b20; num161++)
                        {
                            sbyte b21 = msg.reader().readByte();
                            TileMap.tileType[num161] = new int[b21];
                            TileMap.tileIndex[num161] = new int[b21][];
                            for (int num162 = 0; num162 < b21; num162++)
                            {
                                TileMap.tileType[num161][num162] = msg.reader().readInt();
                                sbyte b23 = msg.reader().readByte();
                                TileMap.tileIndex[num161][num162] = new int[b23];
                                for (int num163 = 0; num163 < b23; num163++)
                                {
                                    TileMap.tileIndex[num161][num162][num163] = msg.reader().readByte();
                                }
                            }
                        }
                        break;
                    }
                case -81:
                    {
                        sbyte b4 = msg.reader().readByte();
                        if (b4 == 0)
                        {
                            string src = msg.reader().readUTF();
                            string src2 = msg.reader().readUTF();
                            GameCanvas.panel.setTypeCombine();
                            GameCanvas.panel.combineInfo = mFont.tahoma_7b_blue.splitFontArray(src, PanelG.WIDTH_PANEL);
                            GameCanvas.panel.combineTopInfo = mFont.tahoma_7.splitFontArray(src2, PanelG.WIDTH_PANEL);
                            GameCanvas.panel.show();
                        }
                        if (b4 == 1)
                        {
                            GameCanvas.panel.vItemCombine.removeAllElements();
                            sbyte b5 = msg.reader().readByte();
                            for (int num117 = 0; num117 < b5; num117++)
                            {
                                sbyte b6 = msg.reader().readByte();
                                for (int num118 = 0; num118 < Char.myCharz().arrItemBag.Length; num118++)
                                {
                                    Item item = Char.myCharz().arrItemBag[num118];
                                    if (item != null && item.indexUI == b6)
                                    {
                                        item.isSelect = true;
                                        GameCanvas.panel.vItemCombine.addElement(item);
                                    }
                                }
                            }
                            if (GameCanvas.panel.isShow)
                            {
                                GameCanvas.panel.setTabCombine();
                            }
                        }
                        if (b4 == 2)
                        {
                            GameCanvas.panel.combineSuccess = 0;
                            GameCanvas.panel.setCombineEff(0);
                        }
                        if (b4 == 3)
                        {
                            GameCanvas.panel.combineSuccess = 1;
                            GameCanvas.panel.setCombineEff(0);
                        }
                        if (b4 == 4)
                        {
                            short iconID = msg.reader().readShort();
                            GameCanvas.panel.iconID3 = iconID;
                            GameCanvas.panel.combineSuccess = 0;
                            GameCanvas.panel.setCombineEff(1);
                        }
                        if (b4 == 5)
                        {
                            short iconID2 = msg.reader().readShort();
                            GameCanvas.panel.iconID3 = iconID2;
                            GameCanvas.panel.combineSuccess = 0;
                            GameCanvas.panel.setCombineEff(2);
                        }
                        if (b4 == 6)
                        {
                            short iconID3 = msg.reader().readShort();
                            short iconID4 = msg.reader().readShort();
                            GameCanvas.panel.combineSuccess = 0;
                            GameCanvas.panel.setCombineEff(3);
                            GameCanvas.panel.iconID1 = iconID3;
                            GameCanvas.panel.iconID3 = iconID4;
                        }
                        if (b4 == 7)
                        {
                            short iconID5 = msg.reader().readShort();
                            GameCanvas.panel.iconID3 = iconID5;
                            GameCanvas.panel.combineSuccess = 0;
                            GameCanvas.panel.setCombineEff(4);
                        }
                        if (b4 == 8)
                        {
                            GameCanvas.panel.iconID3 = -1;
                            GameCanvas.panel.combineSuccess = 1;
                            GameCanvas.panel.setCombineEff(4);
                        }
                        short num120 = 21;
                        int num121 = 0;
                        int num122 = 0;
                        try
                        {
                            num120 = msg.reader().readShort();
                            num121 = msg.reader().readShort();
                            num122 = msg.reader().readShort();
                            GameCanvas.panel.xS = num121 - GameScr.cmx;
                            GameCanvas.panel.yS = num122 - GameScr.cmy;
                        }
                        catch (Exception)
                        {
                        }
                        for (int num123 = 0; num123 < GameScr.vNpc.size(); num123++)
                        {
                            Npc npc = (Npc)GameScr.vNpc.elementAt(num123);
                            if (npc.template.npcTemplateId == num120)
                            {
                                GameCanvas.panel.xS = npc.cx - GameScr.cmx;
                                GameCanvas.panel.yS = npc.cy - GameScr.cmy;
                                GameCanvas.panel.idNPC = num120;
                                break;
                            }
                        }
                        break;
                    }
                case -80:
                    {
                        sbyte b42 = msg.reader().readByte();
                        InfoDlg.hide();
                        if (b42 == 0)
                        {
                            GameCanvas.panel.vFriend.removeAllElements();
                            int num23 = msg.reader().readUnsignedByte();
                            for (int num25 = 0; num25 < num23; num25++)
                            {
                                Char char15 = new Char();
                                char15.charID = msg.reader().readInt();
                                char15.head = msg.reader().readShort();
                                char15.headICON = msg.reader().readShort();
                                char15.body = msg.reader().readShort();
                                char15.leg = msg.reader().readShort();
                                char15.bag = msg.reader().readShort();
                                char15.cName = msg.reader().readUTF();
                                bool isOnline = msg.reader().readBoolean();
                                InfoItem infoItem = new InfoItem(mResources.power + ": " + msg.reader().readUTF());
                                infoItem.charInfo = char15;
                                infoItem.isOnline = isOnline;
                                GameCanvas.panel.vFriend.addElement(infoItem);
                            }
                            GameCanvas.panel.setTypeFriend();
                            GameCanvas.panel.show();
                        }
                        if (b42 == 3)
                        {
                            MyVector vFriend = GameCanvas.panel.vFriend;
                            int num26 = msg.reader().readInt();
                            Res.outz("online offline id=" + num26);
                            for (int num27 = 0; num27 < vFriend.size(); num27++)
                            {
                                InfoItem infoItem2 = (InfoItem)vFriend.elementAt(num27);
                                if (infoItem2.charInfo != null && infoItem2.charInfo.charID == num26)
                                {
                                    Res.outz("online= " + infoItem2.isOnline);
                                    infoItem2.isOnline = msg.reader().readBoolean();
                                    break;
                                }
                            }
                        }
                        if (b42 != 2)
                        {
                            break;
                        }
                        MyVector vFriend2 = GameCanvas.panel.vFriend;
                        int num28 = msg.reader().readInt();
                        for (int num29 = 0; num29 < vFriend2.size(); num29++)
                        {
                            InfoItem infoItem3 = (InfoItem)vFriend2.elementAt(num29);
                            if (infoItem3.charInfo != null && infoItem3.charInfo.charID == num28)
                            {
                                vFriend2.removeElement(infoItem3);
                                break;
                            }
                        }
                        if (GameCanvas.panel.isShow)
                        {
                            GameCanvas.panel.setTabFriend();
                        }
                        break;
                    }
                case -99:
                    InfoDlg.hide();
                    if (msg.reader().readByte() == 0)
                    {
                        GameCanvas.panel.vEnemy.removeAllElements();
                        int num45 = msg.reader().readUnsignedByte();
                        for (int num47 = 0; num47 < num45; num47++)
                        {
                            Char char2 = new Char();
                            char2.charID = msg.reader().readInt();
                            char2.head = msg.reader().readShort();
                            char2.headICON = msg.reader().readShort();
                            char2.body = msg.reader().readShort();
                            char2.leg = msg.reader().readShort();
                            char2.bag = msg.reader().readShort();
                            char2.cName = msg.reader().readUTF();
                            InfoItem infoItem4 = new InfoItem(msg.reader().readUTF());
                            bool flag2 = msg.reader().readBoolean();
                            infoItem4.charInfo = char2;
                            infoItem4.isOnline = flag2;
                            Res.outz("isonline = " + flag2);
                            GameCanvas.panel.vEnemy.addElement(infoItem4);
                        }
                        GameCanvas.panel.setTypeEnemy();
                        GameCanvas.panel.show();
                    }
                    break;
                case -79:
                    {
                        InfoDlg.hide();
                        int num9 = msg.reader().readInt();
                        Char charMenu = GameCanvas.panel.charMenu;
                        if (charMenu == null)
                        {
                            return;
                        }
                        charMenu.cPower = msg.reader().readLong();
                        charMenu.currStrLevel = msg.reader().readUTF();
                        break;
                    }
                case -93:
                    {
                        short num63 = msg.reader().readShort();
                        BgItem.newSmallVersion = new sbyte[num63];
                        for (int num64 = 0; num64 < num63; num64++)
                        {
                            BgItem.newSmallVersion[num64] = msg.reader().readByte();
                        }
                        break;
                    }
                case -77:
                    {
                        short num21 = msg.reader().readShort();
                        SmallImage.newSmallVersion = new sbyte[num21];
                        SmallImage.maxSmall = num21;
                        if (SmallImage.imgNew != null)
                        {
                            for (int i = 0; i < SmallImage.imgNew.Length; i++)
                            {
                                if (SmallImage.imgNew[i] != null && SmallImage.imgNew[i].img != null && SmallImage.imgNew[i].img != SmallImage.imgEmpty)
                                {
                                    try { SmallImage.imgNew[i].img.Dispose(); } catch { }
                                }
                            }
                        }
                        SmallImage.imgNew = new Small[num21];
                        for (int num22 = 0; num22 < num21; num22++)
                        {
                            SmallImage.newSmallVersion[num22] = msg.reader().readByte();
                        }
                        break;
                    }
                case -76:
                    switch (msg.reader().readByte())
                    {
                        case 0:
                            {
                                sbyte b7 = msg.reader().readByte();
                                if (b7 <= 0)
                                {
                                    return;
                                }
                                Char.myCharz().arrArchive = new Archivement[b7];
                                for (int num124 = 0; num124 < b7; num124++)
                                {
                                    Char.myCharz().arrArchive[num124] = new Archivement();
                                    Char.myCharz().arrArchive[num124].info1 = num124 + 1 + ". " + msg.reader().readUTF();
                                    Char.myCharz().arrArchive[num124].info2 = msg.reader().readUTF();
                                    Char.myCharz().arrArchive[num124].money = msg.reader().readShort();
                                    Char.myCharz().arrArchive[num124].isFinish = msg.reader().readBoolean();
                                    Char.myCharz().arrArchive[num124].isRecieve = msg.reader().readBoolean();
                                }
                                GameCanvas.panel.setTypeArchivement();
                                GameCanvas.panel.show();
                                break;
                            }
                        case 1:
                            {
                                int num125 = msg.reader().readUnsignedByte();
                                if (Char.myCharz().arrArchive[num125] != null)
                                {
                                    Char.myCharz().arrArchive[num125].isRecieve = true;
                                }
                                break;
                            }
                    }
                    break;
                case -74:
                    {
                        if (ServerListScreen.stopDownload)
                        {
                            return;
                        }
                        if (!GameCanvas.isGetResourceFromServer())
                        {
                            Service.gI().getResource(3, null);
                            SmallImage.loadBigRMS();
                            SplashScr.imgLogo = null;
                            if (Rms.loadRMSString("acc") != null || Rms.loadRMSString("userAo" + ServerListScreen.ipSelect) != null)
                            {
                                LoginScr.isContinueToLogin = true;
                            }
                            GameCanvas.loginScr = new LoginScr();
                            GameCanvas.loginScr.switchToMe();
                            return;
                        }
                        bool flag9 = true;
                        Res.outz("1>>GET_IMAGE_SOURCE = " + msg.reader().available());
                        sbyte b33 = msg.reader().readByte();
                        Res.outz("2>GET_IMAGE_SOURCE = " + b33);
                        if (b33 == 0)
                        {
                            int num194 = msg.reader().readInt();
                            Res.outz("3>GET_IMAGE_SOURCE serverVersion = " + num194);
                            string text3 = Rms.loadRMSString("ResVersion");
                            int num195 = ((text3 == null || !(text3 != string.Empty)) ? (-1) : int.Parse(text3));
                            Res.outz("4>>>GET_IMAGE_SOURCE: version>> " + text3 + " <> " + num195 + "!=" + num194);
                            if (num195 == -1 || num195 != num194)
                            {
                                GameCanvas.serverScreen.show2();
                            }
                            else
                            {
                                SmallImage.loadBigRMS();
                                SplashScr.imgLogo = null;
                                ServerListScreen.loadScreen = true;
                                Res.outz(">>>vo ne: " + GameCanvas.currentScreen);
                                if (GameCanvas.currentScreen != GameCanvas.loginScr)
                                {
                                    if (GameCanvas.serverScreen == null)
                                    {
                                        GameCanvas.serverScreen = new ServerListScreen();
                                    }
                                    GameCanvas.serverScreen.switchToMe();
                                }
                                else
                                {
                                    if (GameCanvas.loginScr == null)
                                    {
                                        GameCanvas.loginScr = new LoginScr();
                                    }
                                    GameCanvas.loginScr.doLogin();
                                }
                            }
                        }
                        if (b33 == 1)
                        {
                            ServerListScreen.strWait = mResources.downloading_data;
                            short num196 = (short)(ServerListScreen.nBig = msg.reader().readShort());
                            Service.gI().getResource(2, null);
                        }
                        if (b33 == 2)
                        {
                            try
                            {
                                isLoadingData = true;
                                GameCanvas.endDlg();
                                ServerListScreen.demPercent++;
                                ServerListScreen.percent = ServerListScreen.demPercent * 100 / ServerListScreen.nBig;
                                string text4 = msg.reader().readUTF();
                                Res.outz(">>>vo serverPath: " + text4);
                                string[] array17 = Res.split(text4, "/", 0);
                                string filename = array17[array17.Length - 1];
                                int num197 = msg.reader().readInt();
                                sbyte[] data4 = new sbyte[num197];
                                msg.reader().read(ref data4, 0, num197);
                                Rms.saveRMS(filename, data4);
                            }
                            catch (Exception)
                            {
                                GameCanvas.startOK(mResources.pls_restart_game_error, 8885, null);
                            }
                        }
                        if (b33 == 3 && flag9)
                        {
                            isLoadingData = false;
                            int num198 = msg.reader().readInt();
                            Res.outz(">>>GET_IMAGE_SOURCE: lastVersion>> " + num198);
                            Rms.saveRMSString("ResVersion", num198 + string.Empty);
                            Service.gI().getResource(3, null);
                            GameCanvas.endDlg();
                            SplashScr.imgLogo = null;
                            SmallImage.loadBigRMS();
                            mSystem.gcc();
                            ServerListScreen.bigOk = true;
                            ServerListScreen.loadScreen = true;
                            GameScr.gI().loadGameScr();
                            GameScr.isLoadAllData = false;
                            Service.gI().updateData();
                            if (GameCanvas.currentScreen != GameCanvas.loginScr)
                            {
                                GameCanvas.serverScreen.switchToMe();
                            }
                        }
                        break;
                    }
                case -43:
                    {
                        sbyte itemAction = msg.reader().readByte();
                        sbyte where = msg.reader().readByte();
                        sbyte index2 = msg.reader().readByte();
                        string info = msg.reader().readUTF();
                        GameCanvas.panel.itemRequest(itemAction, info, where, index2);
                        break;
                    }
                case -59:
                    {
                        sbyte typePK = msg.reader().readByte();
                        GameScr.gI().player_vs_player(msg.reader().readInt(), msg.reader().readInt(), msg.reader().readUTF(), typePK);
                        break;
                    }
                case -62:
                    {
                        int num135 = msg.reader().readUnsignedByte();
                        sbyte b12 = msg.reader().readByte();
                        if (b12 <= 0)
                        {
                            break;
                        }
                        ClanImage clanImage = ClanImage.getClanImage((short)num135);
                        if (clanImage == null)
                        {
                            break;
                        }
                        clanImage.idImage = new short[b12];
                        for (int num136 = 0; num136 < b12; num136++)
                        {
                            clanImage.idImage[num136] = msg.reader().readShort();
                            if (clanImage.idImage[num136] > 0)
                            {
                                SmallImage.vKeys.addElement(clanImage.idImage[num136] + string.Empty);
                            }
                        }
                        break;
                    }
                case -65:
                    {
                        InfoDlg.hide();
                        int num187 = msg.reader().readInt();
                        sbyte b29 = msg.reader().readByte();
                        if (b29 == 0)
                        {
                            break;
                        }
                        if (Char.myCharz().charID == num187)
                        {
                            Controller.isStopReadMessage = true;
                            GameScr.lockTick = 500;
                            GameScr.gI().center = null;
                            Teleport teleport = new Teleport(Char.myCharz().cx, Char.myCharz().cy, Char.myCharz().head, Char.myCharz().cdir, 0, isMe: true, (b29 != 1) ? b29 : Char.myCharz().cgender);
                            Teleport.addTeleport(teleport);
                        }
                        else
                        {
                            Char char12 = GameScr.findCharInMap(num187);
                            if ((b29 == 0 || b29 == 1 || b29 == 3) && char12 != null)
                            {
                                char12.isUsePlane = true;
                                Teleport teleport = new Teleport(char12.cx, char12.cy, char12.head, char12.cdir, 0, isMe: false, (b29 != 1) ? b29 : char12.cgender);
                                teleport.id = num187;
                                Teleport.addTeleport(teleport);
                            }
                            if (b29 == 2 && char12 != null)
                            {
                                char12.hide();
                            }
                        }
                        break;
                    }
                case -64:
                    {
                        int num142 = msg.reader().readInt();
                        int num143 = msg.reader().readShort();
                        @char = null;
                        @char = ((num142 != Char.myCharz().charID) ? GameScr.findCharInMap(num142) : Char.myCharz());
                        if (@char == null)
                        {
                            return;
                        }
                        @char.bag = num143;
                        for (int num144 = 0; num144 < 54; num144++)
                        {
                            @char.removeEffChar(0, 201 + num144);
                        }
                        if (@char.bag >= 201 && @char.bag < 255)
                        {
                            Effect effect = new Effect(@char.bag, @char, 2, -1, 10, 1);
                            effect.typeEff = 5;
                            @char.addEffChar(effect);
                        }
                        Res.outz("cmd:-64 UPDATE BAG PLAER = " + ((@char != null) ? @char.cName : string.Empty) + num142 + " BAG ID= " + num143);
                        if (num143 == 30 && @char.me)
                        {
                            GameScr.isPickNgocRong = true;
                        }
                        break;
                    }
                case -63:
                    {
                        Res.outz("GET BAG");
                        int num146 = msg.reader().readShort();
                        sbyte b16 = msg.reader().readByte();
                        ClanImage clanImage2 = new ClanImage();
                        clanImage2.ID = num146;
                        if (b16 > 0)
                        {
                            clanImage2.idImage = new short[b16];
                            for (int num147 = 0; num147 < b16; num147++)
                            {
                                clanImage2.idImage[num147] = msg.reader().readShort();
                                Res.outz("ID=  " + num146 + " frame= " + clanImage2.idImage[num147]);
                            }
                            ClanImage.idImages.put(num146 + string.Empty, clanImage2);
                        }
                        break;
                    }
                case -57:
                    {
                        string strInvite = msg.reader().readUTF();
                        int clanID = msg.reader().readInt();
                        int code = msg.reader().readInt();
                        GameScr.gI().clanInvite(strInvite, clanID, code);
                        break;
                    }
                case -51:
                    InfoDlg.hide();
                    readClanMsg(msg, 0);
                    if (GameCanvas.panel.isMessage && GameCanvas.panel.type == 5)
                    {
                        GameCanvas.panel.initTabClans();
                    }
                    break;
                case -53:
                    {
                        InfoDlg.hide();
                        bool flag10 = false;
                        int num5 = msg.reader().readInt();
                        Res.outz("clanId= " + num5);
                        if (num5 == -1)
                        {
                            flag10 = true;
                            Char.myCharz().clan = null;
                            ClanMessage.vMessage.removeAllElements();
                            if (GameCanvas.panel.member != null)
                            {
                                GameCanvas.panel.member.removeAllElements();
                            }
                            if (GameCanvas.panel.myMember != null)
                            {
                                GameCanvas.panel.myMember.removeAllElements();
                            }
                            if (GameCanvas.currentScreen == GameScr.gI())
                            {
                                GameCanvas.panel.setTabClans();
                            }
                            return;
                        }
                        GameCanvas.panel.tabIcon = null;
                        if (Char.myCharz().clan == null)
                        {
                            Char.myCharz().clan = new Clan();
                        }
                        Char.myCharz().clan.ID = num5;
                        Char.myCharz().clan.name = msg.reader().readUTF();
                        Char.myCharz().clan.slogan = msg.reader().readUTF();
                        Char.myCharz().clan.imgID = msg.reader().readShort();
                        Char.myCharz().clan.powerPoint = msg.reader().readUTF();
                        Char.myCharz().clan.leaderName = msg.reader().readUTF();
                        Char.myCharz().clan.currMember = msg.reader().readUnsignedByte();
                        Char.myCharz().clan.maxMember = msg.reader().readUnsignedByte();
                        Char.myCharz().role = msg.reader().readByte();
                        Char.myCharz().clan.clanPoint = msg.reader().readInt();
                        Char.myCharz().clan.level = msg.reader().readByte();
                        GameCanvas.panel.myMember = new MyVector();
                        for (int num6 = 0; num6 < Char.myCharz().clan.currMember; num6++)
                        {
                            Member member5 = new Member();
                            member5.ID = msg.reader().readInt();
                            member5.head = msg.reader().readShort();
                            member5.headICON = msg.reader().readShort();
                            member5.leg = msg.reader().readShort();
                            member5.body = msg.reader().readShort();
                            member5.name = msg.reader().readUTF();
                            member5.role = msg.reader().readByte();
                            member5.powerPoint = msg.reader().readUTF();
                            member5.donate = msg.reader().readInt();
                            member5.receive_donate = msg.reader().readInt();
                            member5.clanPoint = msg.reader().readInt();
                            member5.curClanPoint = msg.reader().readInt();
                            member5.joinTime = NinjaUtil.getDate(msg.reader().readInt());
                            GameCanvas.panel.myMember.addElement(member5);
                        }
                        int num7 = msg.reader().readUnsignedByte();
                        for (int num8 = 0; num8 < num7; num8++)
                        {
                            readClanMsg(msg, -1);
                        }
                        if (GameCanvas.panel.isSearchClan || GameCanvas.panel.isViewMember || GameCanvas.panel.isMessage)
                        {
                            GameCanvas.panel.setTabClans();
                        }
                        if (flag10)
                        {
                            GameCanvas.panel.setTabClans();
                        }
                        Res.outz("=>>>>>>>>>>>>>>>>>>>>>> -537 MY CLAN INFO");
                        break;
                    }
                case -52:
                    {
                        sbyte b15 = msg.reader().readByte();
                        if (b15 == 0)
                        {
                            Member member2 = new Member();
                            member2.ID = msg.reader().readInt();
                            member2.head = msg.reader().readShort();
                            member2.headICON = msg.reader().readShort();
                            member2.leg = msg.reader().readShort();
                            member2.body = msg.reader().readShort();
                            member2.name = msg.reader().readUTF();
                            member2.role = msg.reader().readByte();
                            member2.powerPoint = msg.reader().readUTF();
                            member2.donate = msg.reader().readInt();
                            member2.receive_donate = msg.reader().readInt();
                            member2.clanPoint = msg.reader().readInt();
                            member2.joinTime = NinjaUtil.getDate(msg.reader().readInt());
                            if (GameCanvas.panel.myMember == null)
                            {
                                GameCanvas.panel.myMember = new MyVector();
                            }
                            GameCanvas.panel.myMember.addElement(member2);
                            GameCanvas.panel.initTabClans();
                        }
                        if (b15 == 1)
                        {
                            GameCanvas.panel.myMember.removeElementAt(msg.reader().readByte());
                            PanelG panel = GameCanvas.panel;
                            panel.currentListLength--;
                            GameCanvas.panel.initTabClans();
                        }
                        if (b15 == 2)
                        {
                            Member member3 = new Member();
                            member3.ID = msg.reader().readInt();
                            member3.head = msg.reader().readShort();
                            member3.headICON = msg.reader().readShort();
                            member3.leg = msg.reader().readShort();
                            member3.body = msg.reader().readShort();
                            member3.name = msg.reader().readUTF();
                            member3.role = msg.reader().readByte();
                            member3.powerPoint = msg.reader().readUTF();
                            member3.donate = msg.reader().readInt();
                            member3.receive_donate = msg.reader().readInt();
                            member3.clanPoint = msg.reader().readInt();
                            member3.joinTime = NinjaUtil.getDate(msg.reader().readInt());
                            for (int num141 = 0; num141 < GameCanvas.panel.myMember.size(); num141++)
                            {
                                Member member4 = (Member)GameCanvas.panel.myMember.elementAt(num141);
                                if (member4.ID == member3.ID)
                                {
                                    if (Char.myCharz().charID == member3.ID)
                                    {
                                        Char.myCharz().role = member3.role;
                                    }
                                    Member o = member3;
                                    GameCanvas.panel.myMember.removeElement(member4);
                                    GameCanvas.panel.myMember.insertElementAt(o, num141);
                                    return;
                                }
                            }
                        }
                        Res.outz("=>>>>>>>>>>>>>>>>>>>>>> -52  MY CLAN UPDSTE");
                        break;
                    }
                case -50:
                    {
                        InfoDlg.hide();
                        GameCanvas.panel.member = new MyVector();
                        sbyte b8 = msg.reader().readByte();
                        for (int num131 = 0; num131 < b8; num131++)
                        {
                            Member member = new Member();
                            member.ID = msg.reader().readInt();
                            member.head = msg.reader().readShort();
                            member.headICON = msg.reader().readShort();
                            member.leg = msg.reader().readShort();
                            member.body = msg.reader().readShort();
                            member.name = msg.reader().readUTF();
                            member.role = msg.reader().readByte();
                            member.powerPoint = msg.reader().readUTF();
                            member.donate = msg.reader().readInt();
                            member.receive_donate = msg.reader().readInt();
                            member.clanPoint = msg.reader().readInt();
                            member.joinTime = NinjaUtil.getDate(msg.reader().readInt());
                            GameCanvas.panel.member.addElement(member);
                        }
                        GameCanvas.panel.isViewMember = true;
                        GameCanvas.panel.isSearchClan = false;
                        GameCanvas.panel.isMessage = false;
                        GameCanvas.panel.currentListLength = GameCanvas.panel.member.size() + 2;
                        GameCanvas.panel.initTabClans();
                        break;
                    }
                case -47:
                    {
                        InfoDlg.hide();
                        sbyte b73 = msg.reader().readByte();
                        Res.outz("clan = " + b73);
                        if (b73 == 0)
                        {
                            GameCanvas.panel.clanReport = mResources.cannot_find_clan;
                            GameCanvas.panel.clans = null;
                        }
                        else
                        {
                            GameCanvas.panel.clans = new Clan[b73];
                            Res.outz("clan search lent= " + GameCanvas.panel.clans.Length);
                            for (int k = 0; k < GameCanvas.panel.clans.Length; k++)
                            {
                                GameCanvas.panel.clans[k] = new Clan();
                                GameCanvas.panel.clans[k].ID = msg.reader().readInt();
                                GameCanvas.panel.clans[k].name = msg.reader().readUTF();
                                GameCanvas.panel.clans[k].slogan = msg.reader().readUTF();
                                GameCanvas.panel.clans[k].imgID = msg.reader().readShort();
                                GameCanvas.panel.clans[k].powerPoint = msg.reader().readUTF();
                                GameCanvas.panel.clans[k].leaderName = msg.reader().readUTF();
                                GameCanvas.panel.clans[k].currMember = msg.reader().readUnsignedByte();
                                GameCanvas.panel.clans[k].maxMember = msg.reader().readUnsignedByte();
                                GameCanvas.panel.clans[k].date = msg.reader().readInt();
                            }
                        }
                        GameCanvas.panel.isSearchClan = true;
                        GameCanvas.panel.isViewMember = false;
                        GameCanvas.panel.isMessage = false;
                        if (GameCanvas.panel.isSearchClan)
                        {
                            GameCanvas.panel.initTabClans();
                        }
                        break;
                    }
                case -46:
                    {
                        InfoDlg.hide();
                        sbyte b56 = msg.reader().readByte();
                        if (b56 == 1 || b56 == 3)
                        {
                            GameCanvas.endDlg();
                            ClanImage.vClanImage.removeAllElements();
                            int num52 = msg.reader().readShort();
                            for (int num53 = 0; num53 < num52; num53++)
                            {
                                ClanImage clanImage3 = new ClanImage();
                                clanImage3.ID = msg.reader().readShort();
                                clanImage3.name = msg.reader().readUTF();
                                clanImage3.xu = msg.reader().readInt();
                                clanImage3.luong = msg.reader().readInt();
                                if (!ClanImage.isExistClanImage(clanImage3.ID))
                                {
                                    ClanImage.addClanImage(clanImage3);
                                    continue;
                                }
                                ClanImage.getClanImage((short)clanImage3.ID).name = clanImage3.name;
                                ClanImage.getClanImage((short)clanImage3.ID).xu = clanImage3.xu;
                                ClanImage.getClanImage((short)clanImage3.ID).luong = clanImage3.luong;
                            }
                            if (Char.myCharz().clan != null)
                            {
                                GameCanvas.panel.changeIcon();
                            }
                        }
                        if (b56 == 4)
                        {
                            Char.myCharz().clan.imgID = msg.reader().readShort();
                            Char.myCharz().clan.slogan = msg.reader().readUTF();
                        }
                        break;
                    }
                case -61:
                    {
                        int num43 = msg.reader().readInt();
                        if (num43 != Char.myCharz().charID)
                        {
                            if (GameScr.findCharInMap(num43) != null)
                            {
                                GameScr.findCharInMap(num43).clanID = msg.reader().readInt();
                                if (GameScr.findCharInMap(num43).clanID == -2)
                                {
                                    GameScr.findCharInMap(num43).isCopy = true;
                                }
                            }
                        }
                        else if (Char.myCharz().clan != null)
                        {
                            Char.myCharz().clan.ID = msg.reader().readInt();
                        }
                        break;
                    }
                case -42:
                    Char.myCharz().cHPGoc = msg.readInt3Byte();
                    Char.myCharz().cMPGoc = msg.readInt3Byte();
                    Char.myCharz().cDamGoc = msg.reader().readInt();
                    Char.myCharz().cHPFull = msg.reader().readLong();
                    Char.myCharz().cMPFull = msg.reader().readLong();
                    Char.myCharz().cHP = msg.reader().readLong();
                    Char.myCharz().cMP = msg.reader().readLong();
                    Char.myCharz().cspeed = msg.reader().readByte();
                    Char.myCharz().hpFrom1000TiemNang = msg.reader().readByte();
                    Char.myCharz().mpFrom1000TiemNang = msg.reader().readByte();
                    Char.myCharz().damFrom1000TiemNang = msg.reader().readByte();
                    Char.myCharz().cDamFull = msg.reader().readLong();
                    Char.myCharz().cDefull = msg.reader().readLong();
                    Char.myCharz().cCriticalFull = msg.reader().readByte();
                    Char.myCharz().cTiemNang = msg.reader().readLong();
                    Char.myCharz().expForOneAdd = msg.reader().readShort();
                    Char.myCharz().cDefGoc = msg.reader().readInt();
                    Char.myCharz().cCriticalGoc = msg.reader().readByte();
                    Char.myCharz().cGiamST = msg.reader().readByte();
                    Char.myCharz().cCritDameFull = msg.reader().readShort();
                    InfoDlg.hide();
                    break;
                case 1:
                    {
                        bool flag12 = msg.reader().readBool();
                        Res.outz("isRes= " + flag12);
                        if (!flag12)
                        {
                            GameScr.info1.addInfo(msg.reader().readUTF(), 0);
                            break;
                        }
                        GameCanvas.loginScr.isLogin2 = false;
                        Rms.saveRMSString("userAo" + ServerListScreen.ipSelect, string.Empty);
                        GameCanvas.endDlg();
                        GameCanvas.loginScr.doLogin();
                        break;
                    }
                case 2:
                    Char.isLoadingMap = false;
                    LoginScr.isLoggingIn = false;
                    if (!GameScr.isLoadAllData)
                    {
                        GameScr.gI().initSelectChar();
                    }
                    BgItem.clearHashTable();
                    GameCanvas.endDlg();
                    CreateCharScr.isCreateChar = true;
                    CreateCharScr.gI().switchToMe();
                    break;
                case -107:
                    {
                        sbyte b18 = msg.reader().readByte();
                        if (b18 == 0)
                        {
                            Char.myCharz().havePet = false;
                        }
                        if (b18 == 1)
                        {
                            Char.myCharz().havePet = true;
                        }
                        if (b18 != 2)
                        {
                            break;
                        }
                        InfoDlg.hide();
                        Char.myPetz().head = msg.reader().readShort();
                        Debug.WriteLine(">>>cmd head:" + Char.myPetz().avatarz());
                        Res.outz("tra ve head= " + Char.myCharz().head);
                        Char.myPetz().setDefaultPart();
                        int num149 = msg.reader().readUnsignedByte();
                        Res.outz("num body = " + num149);
                        Char.myPetz().arrItemBody = new Item[num149];
                        for (int num150 = 0; num150 < num149; num150++)
                        {
                            short num151 = msg.reader().readShort();
                            Res.outz("template id= " + num151);
                            if (num151 == -1)
                            {
                                continue;
                            }
                            Res.outz("1");
                            Char.myPetz().arrItemBody[num150] = new Item();
                            Char.myPetz().arrItemBody[num150].template = ItemTemplates.get(num151);
                            int num152 = Char.myPetz().arrItemBody[num150].template.type;
                            Char.myPetz().arrItemBody[num150].quantity = msg.reader().readInt();
                            Res.outz("3");
                            Char.myPetz().arrItemBody[num150].info = msg.reader().readUTF();
                            Char.myPetz().arrItemBody[num150].content = msg.reader().readUTF();
                            int num153 = msg.reader().readUnsignedByte();
                            Res.outz("option size= " + num153);
                            if (num153 != 0)
                            {
                                Char.myPetz().arrItemBody[num150].itemOption = new ItemOption[num153];
                                for (int num154 = 0; num154 < Char.myPetz().arrItemBody[num150].itemOption.Length; num154++)
                                {
                                    ItemOption itemOption2 = readItemOption(msg);
                                    if (itemOption2 != null)
                                    {
                                        Char.myPetz().arrItemBody[num150].itemOption[num154] = itemOption2;
                                    }
                                }
                            }
                            switch (num152)
                            {
                                case 0:
                                    Char.myPetz().body = Char.myPetz().arrItemBody[num150].template.part;
                                    break;
                                case 1:
                                    Char.myPetz().leg = Char.myPetz().arrItemBody[num150].template.part;
                                    break;
                            }
                        }
                        Char.myPetz().cHP = msg.reader().readLong();
                        Char.myPetz().cHPFull = msg.reader().readLong();
                        Char.myPetz().cMP = msg.reader().readLong();
                        Char.myPetz().cMPFull = msg.reader().readLong();
                        Char.myPetz().cDamFull = msg.reader().readLong();
                        Char.myPetz().cName = msg.reader().readUTF();
                        Char.myPetz().currStrLevel = msg.reader().readUTF();
                        Char.myPetz().cPower = msg.reader().readLong();
                        Char.myPetz().cTiemNang = msg.reader().readLong();
                        Char.myPetz().petStatus = msg.reader().readByte();
                        Char.myPetz().cStamina = msg.reader().readShort();
                        Char.myPetz().cMaxStamina = msg.reader().readShort();
                        Char.myPetz().cCriticalFull = msg.reader().readByte();
                        Char.myPetz().cDefull = msg.reader().readLong();
                        Char.myPetz().arrPetSkill = new Skill[msg.reader().readByte()];
                        Res.outz("SKILLENT = " + Char.myPetz().arrPetSkill);
                        for (int num155 = 0; num155 < Char.myPetz().arrPetSkill.Length; num155++)
                        {
                            short num157 = msg.reader().readShort();
                            if (num157 != -1)
                            {
                                Char.myPetz().arrPetSkill[num155] = Skills.get(num157);
                                continue;
                            }
                            Char.myPetz().arrPetSkill[num155] = new Skill();
                            Char.myPetz().arrPetSkill[num155].template = null;
                            Char.myPetz().arrPetSkill[num155].moreInfo = msg.reader().readUTF();
                        }
                        Char.myPetz().cGiamST = msg.reader().readByte();
                        Char.myPetz().cCritDameFull = msg.reader().readShort();
                        if (NRO_v247.SocketGame.isAutoRequestingPetInfo)
                        {
                            NRO_v247.SocketGame.isAutoRequestingPetInfo = false;
                        }
                        else
                        {
                            if (GameCanvas.w > 2 * PanelG.WIDTH_PANEL)
                            {
                                GameCanvas.panel2 = new PanelG();
                                GameCanvas.panel2.tabName[7] = new string[1][] { new string[1] { string.Empty } };
                                GameCanvas.panel2.setTypeBodyOnly();
                                GameCanvas.panel2.show();
                                GameCanvas.panel.setTypePetMain();
                                GameCanvas.panel.show();
                            }
                            else
                            {
                                GameCanvas.panel.tabName[21] = mResources.petMainTab;
                                GameCanvas.panel.setTypePetMain();
                                GameCanvas.panel.show();
                            }
                        }
                        break;
                    }
                case -37:
                    {
                        sbyte b28 = msg.reader().readByte();
                        Res.outz("cAction= " + b28);
                        if (b28 != 0)
                        {
                            break;
                        }
                        Char.myCharz().head = msg.reader().readShort();
                        Char.myCharz().setDefaultPart();
                        int num181 = msg.reader().readUnsignedByte();
                        Res.outz("num body = " + num181);
                        Char.myCharz().arrItemBody = new Item[num181];
                        for (int num182 = 0; num182 < num181; num182++)
                        {
                            short num183 = msg.reader().readShort();
                            if (num183 == -1)
                            {
                                continue;
                            }
                            Char.myCharz().arrItemBody[num182] = new Item();
                            Char.myCharz().arrItemBody[num182].template = ItemTemplates.get(num183);
                            int num184 = Char.myCharz().arrItemBody[num182].template.type;
                            Char.myCharz().arrItemBody[num182].quantity = msg.reader().readInt();
                            Char.myCharz().arrItemBody[num182].info = msg.reader().readUTF();
                            Char.myCharz().arrItemBody[num182].content = msg.reader().readUTF();
                            int num185 = msg.reader().readUnsignedByte();
                            if (num185 != 0)
                            {
                                Char.myCharz().arrItemBody[num182].itemOption = new ItemOption[num185];
                                for (int num186 = 0; num186 < Char.myCharz().arrItemBody[num182].itemOption.Length; num186++)
                                {
                                    ItemOption itemOption4 = readItemOption(msg);
                                    if (itemOption4 != null)
                                    {
                                        Char.myCharz().arrItemBody[num182].itemOption[num186] = itemOption4;
                                    }
                                }
                            }
                            switch (num184)
                            {
                                case 0:
                                    Char.myCharz().body = Char.myCharz().arrItemBody[num182].template.part;
                                    break;
                                case 1:
                                    Char.myCharz().leg = Char.myCharz().arrItemBody[num182].template.part;
                                    break;
                            }
                        }
                        break;
                    }
                case -36:
                    {
                        sbyte b74 = msg.reader().readByte();
                        Res.outz("cAction= " + b74);
                        GameScr.isudungCapsun4 = false;
                        GameScr.isudungCapsun3 = false;
                        if (b74 == 0)
                        {
                            int num110 = msg.reader().readUnsignedByte();
                            Char.myCharz().arrItemBag = new Item[num110];
                            GameScr.hpPotion = 0;
                            Res.outz("numC=" + num110);
                            for (int l = 0; l < num110; l++)
                            {
                                short num111 = msg.reader().readShort();
                                if (num111 == -1)
                                {
                                    continue;
                                }
                                Char.myCharz().arrItemBag[l] = new Item();
                                Char.myCharz().arrItemBag[l].template = ItemTemplates.get(num111);
                                Char.myCharz().arrItemBag[l].quantity = msg.reader().readInt();
                                Char.myCharz().arrItemBag[l].info = msg.reader().readUTF();
                                Char.myCharz().arrItemBag[l].content = msg.reader().readUTF();
                                Char.myCharz().arrItemBag[l].indexUI = l;
                                int num112 = msg.reader().readUnsignedByte();
                                if (num112 != 0)
                                {
                                    Char.myCharz().arrItemBag[l].itemOption = new ItemOption[num112];
                                    for (int m = 0; m < Char.myCharz().arrItemBag[l].itemOption.Length; m++)
                                    {
                                        ItemOption itemOption = readItemOption(msg);
                                        if (itemOption != null)
                                        {
                                            Char.myCharz().arrItemBag[l].itemOption[m] = itemOption;
                                        }
                                    }
                                    Char.myCharz().arrItemBag[l].compare = GameCanvas.panel.getCompare(Char.myCharz().arrItemBag[l]);
                                }
                                if (Char.myCharz().arrItemBag[l].template.type == 11)
                                {
                                }
                                if (Char.myCharz().arrItemBag[l].template.type == 6)
                                {
                                    GameScr.hpPotion += Char.myCharz().arrItemBag[l].quantity;
                                }
                                if (Char.myCharz().arrItemBag[l].template.id == 194)
                                {
                                    GameScr.isudungCapsun4 = Char.myCharz().arrItemBag[l].quantity > 0;
                                }
                                else if (Char.myCharz().arrItemBag[l].template.id == 193 && !GameScr.isudungCapsun4)
                                {
                                    GameScr.isudungCapsun3 = Char.myCharz().arrItemBag[l].quantity > 0;
                                }
                            }
                        }
                        if (b74 == 2)
                        {
                            sbyte b2 = msg.reader().readByte();
                            int num113 = msg.reader().readInt();
                            int quantity = Char.myCharz().arrItemBag[b2].quantity;
                            int id = Char.myCharz().arrItemBag[b2].template.id;
                            Char.myCharz().arrItemBag[b2].quantity = num113;
                            if (Char.myCharz().arrItemBag[b2].quantity < quantity && Char.myCharz().arrItemBag[b2].template.type == 6)
                            {
                                GameScr.hpPotion -= quantity - Char.myCharz().arrItemBag[b2].quantity;
                            }
                            if (Char.myCharz().arrItemBag[b2].quantity == 0)
                            {
                                Char.myCharz().arrItemBag[b2] = null;
                            }
                            switch (id)
                            {
                                case 194:
                                    GameScr.isudungCapsun4 = num113 > 0;
                                    break;
                                case 193:
                                    GameScr.isudungCapsun3 = num113 > 0;
                                    break;
                            }
                        }
                        break;
                    }
                case -35:
                    {
                        sbyte b57 = msg.reader().readByte();
                        Res.outz("cAction= " + b57);
                        if (b57 == 0)
                        {
                            int num58 = msg.reader().readUnsignedByte();
                            Char.myCharz().arrItemBox = new Item[num58];
                            GameCanvas.panel.hasUse = 0;
                            for (int num59 = 0; num59 < num58; num59++)
                            {
                                short num60 = msg.reader().readShort();
                                if (num60 == -1)
                                {
                                    continue;
                                }
                                Char.myCharz().arrItemBox[num59] = new Item();
                                Char.myCharz().arrItemBox[num59].template = ItemTemplates.get(num60);
                                Char.myCharz().arrItemBox[num59].quantity = msg.reader().readInt();
                                Char.myCharz().arrItemBox[num59].info = msg.reader().readUTF();
                                Char.myCharz().arrItemBox[num59].content = msg.reader().readUTF();
                                int num61 = msg.reader().readUnsignedByte();
                                if (num61 != 0)
                                {
                                    Char.myCharz().arrItemBox[num59].itemOption = new ItemOption[num61];
                                    for (int num62 = 0; num62 < Char.myCharz().arrItemBox[num59].itemOption.Length; num62++)
                                    {
                                        ItemOption itemOption6 = readItemOption(msg);
                                        if (itemOption6 != null)
                                        {
                                            Char.myCharz().arrItemBox[num59].itemOption[num62] = itemOption6;
                                        }
                                    }
                                }
                                PanelG panel = GameCanvas.panel;
                                panel.hasUse++;
                            }
                        }
                        if (b57 == 1)
                        {
                            bool isBoxClan = false;
                            try
                            {
                                sbyte b58 = msg.reader().readByte();
                                if (b58 == 1)
                                {
                                    isBoxClan = true;
                                }
                            }
                            catch (Exception)
                            {
                            }
                            GameCanvas.panel.setTypeBox();
                            GameCanvas.panel.isBoxClan = isBoxClan;
                            GameCanvas.panel.show();
                        }
                        if (b57 == 2)
                        {
                            sbyte b59 = msg.reader().readByte();
                            int quantity2 = msg.reader().readInt();
                            Char.myCharz().arrItemBox[b59].quantity = quantity2;
                            if (Char.myCharz().arrItemBox[b59].quantity == 0)
                            {
                                Char.myCharz().arrItemBox[b59] = null;
                            }
                        }
                        break;
                    }
                case -45:
                    {
                        sbyte b44 = msg.reader().readByte();
                        int num30 = msg.reader().readInt();
                        short num31 = msg.reader().readShort();
                        Res.outz(">.SKILL_NOT_FOCUS      skillNotFocusID: " + num31 + " skill type= " + b44 + "   player use= " + num30);
                        if (b44 == 20)
                        {
                            sbyte b45 = msg.reader().readByte();
                            sbyte dir = msg.reader().readByte();
                            short timeGong = msg.reader().readShort();
                            bool isFly = ((msg.reader().readByte() != 0) ? true : false);
                            sbyte typePaint = msg.reader().readByte();
                            sbyte typeItem = -1;
                            try
                            {
                                typeItem = msg.reader().readByte();
                            }
                            catch (Exception)
                            {
                            }
                            Res.outz(">.SKILL_NOT_FOCUS  skill typeFrame= " + b45);
                            @char = ((Char.myCharz().charID != num30) ? GameScr.findCharInMap(num30) : Char.myCharz());
                            @char.SetSkillPaint_NEW(num31, isFly, b45, typePaint, dir, timeGong, typeItem);
                        }
                        if (b44 == 21)
                        {
                            Point point = new Point();
                            point.x = msg.reader().readShort();
                            point.y = msg.reader().readShort();
                            short timeDame = msg.reader().readShort();
                            short rangeDame = msg.reader().readShort();
                            sbyte typePaint2 = 0;
                            sbyte typeItem2 = -1;
                            Point[] array18 = null;
                            @char = ((Char.myCharz().charID != num30) ? GameScr.findCharInMap(num30) : Char.myCharz());
                            try
                            {
                                typePaint2 = msg.reader().readByte();
                                sbyte b46 = msg.reader().readByte();
                                if (b46 > 0)
                                {
                                    array18 = new Point[b46];
                                    for (int num32 = 0; num32 < array18.Length; num32++)
                                    {
                                        array18[num32] = new Point();
                                        array18[num32].type = msg.reader().readByte();
                                        if (array18[num32].type == 0)
                                        {
                                            array18[num32].id = msg.reader().readByte();
                                        }
                                        else
                                        {
                                            array18[num32].id = msg.reader().readInt();
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                            try
                            {
                                typeItem2 = msg.reader().readByte();
                            }
                            catch (Exception)
                            {
                            }
                            Res.outz(">.SKILL_NOT_FOCUS  skill targetDame= " + point.x + ":" + point.y + "    c:" + @char.cx + ":" + @char.cy + "   cdir:" + @char.cdir);
                            @char.SetSkillPaint_STT(1, num31, point, timeDame, rangeDame, typePaint2, array18, typeItem2);
                        }
                        if (b44 == 0)
                        {
                            Res.outz("id use= " + num30);
                            if (Char.myCharz().charID != num30)
                            {
                                @char = GameScr.findCharInMap(num30);
                                if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
                                {
                                    @char.setSkillPaint(GameScr.sks[num31], 0);
                                }
                                else
                                {
                                    @char.setSkillPaint(GameScr.sks[num31], 1);
                                    @char.delayFall = 20;
                                }
                            }
                            else
                            {
                                Char.myCharz().saveLoadPreviousSkill();
                                Res.outz("LOAD LAST SKILL");
                            }
                            sbyte b47 = msg.reader().readByte();
                            Res.outz("npc size= " + b47);
                            for (int num33 = 0; num33 < b47; num33++)
                            {
                                sbyte b48 = msg.reader().readByte();
                                sbyte b49 = msg.reader().readByte();
                                Res.outz("index= " + b48);
                                if (num31 >= 42 && num31 <= 48)
                                {
                                    ((Mob)GameScr.vMob.elementAt(b48)).isFreez = true;
                                    ((Mob)GameScr.vMob.elementAt(b48)).seconds = b49;
                                    ((Mob)GameScr.vMob.elementAt(b48)).last = (((Mob)GameScr.vMob.elementAt(b48)).cur = mSystem.currentTimeMillis());
                                }
                            }
                            sbyte b50 = msg.reader().readByte();
                            for (int num34 = 0; num34 < b50; num34++)
                            {
                                int num36 = msg.reader().readInt();
                                sbyte b51 = msg.reader().readByte();
                                Res.outz("player ID= " + num36 + " my ID= " + Char.myCharz().charID);
                                if (num31 < 42 || num31 > 48)
                                {
                                    continue;
                                }
                                if (num36 == Char.myCharz().charID)
                                {
                                    if (!Char.myCharz().isFlyAndCharge && !Char.myCharz().isStandAndCharge)
                                    {
                                        GameScr.gI().isFreez = true;
                                        Char.myCharz().isFreez = true;
                                        Char.myCharz().freezSeconds = b51;
                                        Char.myCharz().lastFreez = (Char.myCharz().currFreez = mSystem.currentTimeMillis());
                                        Char.myCharz().isLockMove = true;
                                    }
                                }
                                else
                                {
                                    @char = GameScr.findCharInMap(num36);
                                    if (@char != null && !@char.isFlyAndCharge && !@char.isStandAndCharge)
                                    {
                                        @char.isFreez = true;
                                        @char.seconds = b51;
                                        @char.freezSeconds = b51;
                                        @char.lastFreez = (GameScr.findCharInMap(num36).currFreez = mSystem.currentTimeMillis());
                                    }
                                }
                            }
                        }
                        if (b44 == 1 && num30 != Char.myCharz().charID)
                        {
                            try
                            {
                                GameScr.findCharInMap(num30).isCharge = true;
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (b44 == 3)
                        {
                            if (num30 == Char.myCharz().charID)
                            {
                                Char.myCharz().isCharge = false;
                                SoundMn.gI().taitaoPause();
                                Char.myCharz().saveLoadPreviousSkill();
                            }
                            else
                            {
                                GameScr.findCharInMap(num30).isCharge = false;
                            }
                        }
                        if (b44 == 4)
                        {
                            if (num30 == Char.myCharz().charID)
                            {
                                Char.myCharz().seconds = msg.reader().readShort() - 1000;
                                Char.myCharz().last = mSystem.currentTimeMillis();
                                Res.outz("second= " + Char.myCharz().seconds + " last= " + Char.myCharz().last);
                            }
                            else if (GameScr.findCharInMap(num30) != null)
                            {
                                Char char16 = GameScr.findCharInMap(num30);
                                switch (char16.cgender)
                                {
                                    case 0:
                                        if (TileMap.mapID != 170)
                                        {
                                            @char.useChargeSkill(isGround: false);
                                            break;
                                        }
                                        if (num31 >= 77 && num31 <= 83)
                                        {
                                            @char.useChargeSkill(isGround: true);
                                        }
                                        if (num31 >= 70 && num31 <= 76)
                                        {
                                            @char.useChargeSkill(isGround: false);
                                        }
                                        break;
                                    case 1:
                                        {
                                            if (TileMap.mapID != 170)
                                            {
                                                @char.useChargeSkill(isGround: true);
                                                break;
                                            }
                                            bool isGround2 = true;
                                            if (num31 >= 70 && num31 <= 76)
                                            {
                                                isGround2 = false;
                                            }
                                            if (num31 >= 77 && num31 <= 83)
                                            {
                                                isGround2 = true;
                                            }
                                            @char.useChargeSkill(isGround2);
                                            break;
                                        }
                                    default:
                                        if (TileMap.mapID == 170)
                                        {
                                            bool isGround = true;
                                            if (num31 >= 70 && num31 <= 76)
                                            {
                                                isGround = false;
                                            }
                                            if (num31 >= 77 && num31 <= 83)
                                            {
                                                isGround = true;
                                            }
                                            @char.useChargeSkill(isGround);
                                        }
                                        break;
                                }
                                @char.skillTemplateId = num31;
                                if (num31 >= 70 && num31 <= 76)
                                {
                                    @char.isUseSkillAfterCharge = true;
                                }
                                @char.seconds = msg.reader().readShort();
                                @char.last = mSystem.currentTimeMillis();
                            }
                        }
                        if (b44 == 5)
                        {
                            if (num30 == Char.myCharz().charID)
                            {
                                Char.myCharz().stopUseChargeSkill();
                            }
                            else if (GameScr.findCharInMap(num30) != null)
                            {
                                GameScr.findCharInMap(num30).stopUseChargeSkill();
                            }
                        }
                        if (b44 == 6)
                        {
                            if (num30 == Char.myCharz().charID)
                            {
                                Char.myCharz().setAutoSkillPaint(GameScr.sks[num31], 0);
                            }
                            else if (GameScr.findCharInMap(num30) != null)
                            {
                                GameScr.findCharInMap(num30).setAutoSkillPaint(GameScr.sks[num31], 0);
                                SoundMn.gI().gong();
                            }
                        }
                        if (b44 == 7)
                        {
                            if (num30 == Char.myCharz().charID)
                            {
                                Char.myCharz().seconds = msg.reader().readShort();
                                Res.outz("second = " + Char.myCharz().seconds);
                                Char.myCharz().last = mSystem.currentTimeMillis();
                            }
                            else if (GameScr.findCharInMap(num30) != null)
                            {
                                GameScr.findCharInMap(num30).useChargeSkill(isGround: true);
                                GameScr.findCharInMap(num30).seconds = msg.reader().readShort();
                                GameScr.findCharInMap(num30).last = mSystem.currentTimeMillis();
                                SoundMn.gI().gong();
                            }
                        }
                        if (b44 == 8 && num30 != Char.myCharz().charID && GameScr.findCharInMap(num30) != null)
                        {
                            GameScr.findCharInMap(num30).setAutoSkillPaint(GameScr.sks[num31], 0);
                        }
                        break;
                    }
                case -44:
                    {
                        bool flag8 = false;
                        if (GameCanvas.w > 2 * PanelG.WIDTH_PANEL)
                        {
                            flag8 = true;
                        }
                        sbyte b25 = msg.reader().readByte();
                        int num165 = msg.reader().readUnsignedByte();
                        Char.myCharz().arrItemShop = new Item[num165][];
                        GameCanvas.panel.shopTabName = new string[num165 + ((!flag8) ? 1 : 0)][];
                        for (int num166 = 0; num166 < GameCanvas.panel.shopTabName.Length; num166++)
                        {
                            GameCanvas.panel.shopTabName[num166] = new string[2];
                        }
                        if (b25 == 2)
                        {
                            GameCanvas.panel.maxPageShop = new int[num165];
                            GameCanvas.panel.currPageShop = new int[num165];
                        }
                        if (!flag8)
                        {
                            GameCanvas.panel.shopTabName[num165] = mResources.inventory;
                        }
                        for (int num168 = 0; num168 < num165; num168++)
                        {
                            string[] array13 = Res.split(msg.reader().readUTF(), "\n", 0);
                            if (b25 == 2)
                            {
                                GameCanvas.panel.maxPageShop[num168] = msg.reader().readUnsignedByte();
                            }
                            if (array13.Length == 2)
                            {
                                GameCanvas.panel.shopTabName[num168] = array13;
                            }
                            if (array13.Length == 1)
                            {
                                GameCanvas.panel.shopTabName[num168][0] = array13[0];
                                GameCanvas.panel.shopTabName[num168][1] = string.Empty;
                            }
                            int num169 = msg.reader().readUnsignedByte();
                            Char.myCharz().arrItemShop[num168] = new Item[num169];
                            PanelG.strWantToBuy = mResources.say_wat_do_u_want_to_buy;
                            if (b25 == 1)
                            {
                                PanelG.strWantToBuy = mResources.say_wat_do_u_want_to_buy2;
                            }
                            for (int num170 = 0; num170 < num169; num170++)
                            {
                                short num171 = msg.reader().readShort();
                                if (num171 == -1)
                                {
                                    continue;
                                }
                                Char.myCharz().arrItemShop[num168][num170] = new Item();
                                Char.myCharz().arrItemShop[num168][num170].template = ItemTemplates.get(num171);
                                switch (b25)
                                {
                                    case 8:
                                        Char.myCharz().arrItemShop[num168][num170].buyCoin = msg.reader().readInt();
                                        Char.myCharz().arrItemShop[num168][num170].buyGold = msg.reader().readInt();
                                        Char.myCharz().arrItemShop[num168][num170].quantity = msg.reader().readInt();
                                        break;
                                    case 4:
                                        Char.myCharz().arrItemShop[num168][num170].reason = msg.reader().readUTF();
                                        break;
                                    case 0:
                                        Char.myCharz().arrItemShop[num168][num170].buyCoin = msg.reader().readInt();
                                        Char.myCharz().arrItemShop[num168][num170].buyGold = msg.reader().readInt();
                                        break;
                                    case 1:
                                        Char.myCharz().arrItemShop[num168][num170].powerRequire = msg.reader().readLong();
                                        break;
                                    case 2:
                                        Char.myCharz().arrItemShop[num168][num170].itemId = msg.reader().readShort();
                                        Char.myCharz().arrItemShop[num168][num170].buyCoin = msg.reader().readInt();
                                        Char.myCharz().arrItemShop[num168][num170].buyGold = msg.reader().readInt();
                                        Char.myCharz().arrItemShop[num168][num170].buyType = msg.reader().readByte();
                                        Char.myCharz().arrItemShop[num168][num170].quantity = msg.reader().readInt();
                                        Char.myCharz().arrItemShop[num168][num170].isMe = msg.reader().readByte();
                                        break;
                                    case 3:
                                        Char.myCharz().arrItemShop[num168][num170].isBuySpec = true;
                                        Char.myCharz().arrItemShop[num168][num170].iconSpec = msg.reader().readShort();
                                        Char.myCharz().arrItemShop[num168][num170].buySpec = msg.reader().readInt();
                                        break;
                                }
                                int num172 = msg.reader().readUnsignedByte();
                                if (num172 != 0)
                                {
                                    Char.myCharz().arrItemShop[num168][num170].itemOption = new ItemOption[num172];
                                    for (int num173 = 0; num173 < Char.myCharz().arrItemShop[num168][num170].itemOption.Length; num173++)
                                    {
                                        ItemOption itemOption3 = readItemOption(msg);
                                        if (itemOption3 != null)
                                        {
                                            Char.myCharz().arrItemShop[num168][num170].itemOption[num173] = itemOption3;
                                            Char.myCharz().arrItemShop[num168][num170].compare = GameCanvas.panel.getCompare(Char.myCharz().arrItemShop[num168][num170]);
                                        }
                                    }
                                }
                                sbyte b26 = msg.reader().readByte();
                                Char.myCharz().arrItemShop[num168][num170].newItem = ((b26 != 0) ? true : false);
                                sbyte b27 = msg.reader().readByte();
                                if (b27 == 1)
                                {
                                    int headTemp = msg.reader().readShort();
                                    int bodyTemp = msg.reader().readShort();
                                    int legTemp = msg.reader().readShort();
                                    int bagTemp = msg.reader().readShort();
                                    Char.myCharz().arrItemShop[num168][num170].setPartTemp(headTemp, bodyTemp, legTemp, bagTemp);
                                }
                                if (b25 == 2 && GameMidlet.intVERSION >= 237)
                                {
                                    Char.myCharz().arrItemShop[num168][num170].nameNguoiKyGui = msg.reader().readUTF();
                                    Res.err("nguoi ki gui  " + Char.myCharz().arrItemShop[num168][num170].nameNguoiKyGui);
                                }
                            }
                        }
                        if (flag8)
                        {
                            if (b25 != 2)
                            {
                                GameCanvas.panel2 = new PanelG();
                                GameCanvas.panel2.tabName[7] = new string[1][] { new string[1] { string.Empty } };
                                GameCanvas.panel2.setTypeBodyOnly();
                                GameCanvas.panel2.show();
                            }
                            else
                            {
                                GameCanvas.panel2 = new PanelG();
                                GameCanvas.panel2.setTypeKiGuiOnly();
                                GameCanvas.panel2.show();
                            }
                        }
                        GameCanvas.panel.tabName[1] = GameCanvas.panel.shopTabName;
                        if (b25 == 2)
                        {
                            string[][] array14 = GameCanvas.panel.tabName[1];
                            if (flag8)
                            {
                                GameCanvas.panel.tabName[1] = new string[4][]
                                {
                            array14[0],
                            array14[1],
                            array14[2],
                            array14[3]
                                };
                            }
                            else
                            {
                                GameCanvas.panel.tabName[1] = new string[5][]
                                {
                            array14[0],
                            array14[1],
                            array14[2],
                            array14[3],
                            array14[4]
                                };
                            }
                        }
                        GameCanvas.panel.setTypeShop(b25);
                        GameCanvas.panel.show();
                        break;
                    }
                case -41:
                    {
                        sbyte b17 = msg.reader().readByte();
                        Char.myCharz().strLevel = new string[b17];
                        for (int num148 = 0; num148 < b17; num148++)
                        {
                            string text = msg.reader().readUTF();
                            Char.myCharz().strLevel[num148] = text;
                        }
                        Res.outz("---   xong  level caption cmd : " + msg.command);
                        break;
                    }
                case -34:
                    {
                        sbyte b9 = msg.reader().readByte();
                        Res.outz("act= " + b9);
                        if (b9 == 0 && GameScr.gI().magicTree != null)
                        {
                            Res.outz("toi duoc day");
                            MagicTree magicTree = GameScr.gI().magicTree;
                            magicTree.id = msg.reader().readShort();
                            magicTree.name = msg.reader().readUTF();
                            magicTree.name = Res.changeString(magicTree.name);
                            magicTree.x = msg.reader().readShort();
                            magicTree.y = msg.reader().readShort();
                            magicTree.level = msg.reader().readByte();
                            magicTree.currPeas = msg.reader().readShort();
                            magicTree.maxPeas = msg.reader().readShort();
                            Res.outz("curr Peas= " + magicTree.currPeas);
                            magicTree.strInfo = msg.reader().readUTF();
                            magicTree.seconds = msg.reader().readInt();
                            magicTree.timeToRecieve = magicTree.seconds;
                            sbyte b10 = msg.reader().readByte();
                            magicTree.peaPostionX = new int[b10];
                            magicTree.peaPostionY = new int[b10];
                            for (int num133 = 0; num133 < b10; num133++)
                            {
                                magicTree.peaPostionX[num133] = msg.reader().readByte();
                                magicTree.peaPostionY[num133] = msg.reader().readByte();
                            }
                            magicTree.isUpdate = msg.reader().readBool();
                            magicTree.last = (magicTree.cur = mSystem.currentTimeMillis());
                            GameScr.gI().magicTree.isUpdateTree = true;
                        }
                        if (b9 == 1)
                        {
                            myVector = new MyVector();
                            try
                            {
                                while (msg.reader().available() > 0)
                                {
                                    string caption = msg.reader().readUTF();
                                    myVector.addElement(new Command(caption, GameCanvas.instance, 888392, null));
                                }
                            }
                            catch (Exception ex31)
                            {
                                Cout.println("Loi MAGIC_TREE " + ex31.ToString());
                            }
                            GameCanvas.menu.startAt(myVector, 3);
                        }
                        if (b9 == 2)
                        {
                            GameScr.gI().magicTree.remainPeas = msg.reader().readShort();
                            GameScr.gI().magicTree.seconds = msg.reader().readInt();
                            GameScr.gI().magicTree.last = (GameScr.gI().magicTree.cur = mSystem.currentTimeMillis());
                            GameScr.gI().magicTree.isUpdateTree = true;
                            GameScr.gI().magicTree.isPeasEffect = true;
                        }
                        break;
                    }
                case 11:
                    {
                        GameCanvas.debug("SA9", 2);
                        int num114 = msg.reader().readShort();
                        sbyte b3 = msg.reader().readByte();
                        if (b3 != 0)
                        {
                            Mob.arrMobTemplate[num114].data.readDataNewBoss(NinjaUtil.readByteArray(msg), b3);
                        }
                        else
                        {
                            Mob.arrMobTemplate[num114].data.readData(NinjaUtil.readByteArray(msg));
                        }
                        for (int n = 0; n < GameScr.vMob.size(); n++)
                        {
                            mob = (Mob)GameScr.vMob.elementAt(n);
                            if (mob.templateId == num114)
                            {
                                mob.w = Mob.arrMobTemplate[num114].data.width;
                                mob.h = Mob.arrMobTemplate[num114].data.height;
                            }
                        }
                        sbyte[] array11 = NinjaUtil.readByteArray(msg);
                        Image img = Image.createImage(array11, 0, array11.Length);
                        Mob.arrMobTemplate[num114].data.img = img;
                        int num115 = msg.reader().readByte();
                        Mob.arrMobTemplate[num114].data.typeData = num115;
                        if (num115 == 1 || num115 == 2)
                        {
                            readFrameBoss(msg, num114);
                        }
                        break;
                    }
                case -69:
                    Char.myCharz().cMaxStamina = msg.reader().readShort();
                    break;
                case -68:
                    Char.myCharz().cStamina = msg.reader().readShort();
                    break;
                case -67:
                    {
                        demCount += 1f;
                        int num66 = msg.reader().readInt();
                        Res.outz("RECIEVE  hinh small: " + num66);
                        sbyte[] array10 = null;
                        try
                        {
                            array10 = NinjaUtil.readByteArray(msg);
                            Res.outz(">SIZE CHECK= " + array10.Length);
                            if (num66 == 3896)
                            {
                            }
                            if (SmallImage.imgNew[num66] != null && SmallImage.imgNew[num66].img != null && SmallImage.imgNew[num66].img != SmallImage.imgEmpty)
                            {
                                try { SmallImage.imgNew[num66].img.Dispose(); } catch { }
                            }
                            SmallImage.imgNew[num66].img = createImage(array10);
                        }
                        catch (Exception)
                        {
                            array10 = null;
                            if (SmallImage.imgNew[num66] != null && SmallImage.imgNew[num66].img != null && SmallImage.imgNew[num66].img != SmallImage.imgEmpty)
                            {
                                try { SmallImage.imgNew[num66].img.Dispose(); } catch { }
                            }
                            SmallImage.imgNew[num66].img = Image.createRGBImage(new int[1], 1, 1, bl: true);
                        }
                        if (array10 != null)
                        {
                            Rms.saveRMS("Small" + num66, array10);
                        }
                        break;
                    }
                case -66:
                    {
                        short id3 = msg.reader().readShort();
                        sbyte[] data5 = NinjaUtil.readByteArray(msg);
                        EffectData effDataById = Effect.getEffDataById(id3);
                        sbyte b60 = msg.reader().readSByte();
                        if (b60 == 0)
                        {
                            effDataById.readData(data5);
                        }
                        else
                        {
                            effDataById.readDataNewBoss(data5, b60);
                        }
                        sbyte[] array8 = NinjaUtil.readByteArray(msg);
                        if (effDataById.img != null)
                        {
                            try { effDataById.img.Dispose(); } catch { }
                        }
                        effDataById.img = Image.createImage(array8, 0, array8.Length);
                        break;
                    }
                case -32:
                    {
                        short num48 = msg.reader().readShort();
                        int num49 = msg.reader().readInt();
                        sbyte[] array4 = null;
                        Image image = null;
                        try
                        {
                            array4 = new sbyte[num49];
                            for (int num50 = 0; num50 < num49; num50++)
                            {
                                array4[num50] = msg.reader().readByte();
                            }
                            image = Image.createImage(array4, 0, num49);
                            BgItem.imgNew.put(num48 + string.Empty, image);
                        }
                        catch (Exception)
                        {
                            array4 = null;
                            BgItem.imgNew.put(num48 + string.Empty, Image.createRGBImage(new int[1], 1, 1, bl: true));
                        }
                        if (array4 != null)
                        {
                            BgItemMn.blendcurrBg(num48, image);
                        }
                        break;
                    }
                case 92:
                    {
                        if (GameCanvas.currentScreen == GameScr.instance)
                        {
                            GameCanvas.endDlg();
                        }
                        string text5 = msg.reader().readUTF();
                        string str2 = msg.reader().readUTF();
                        str2 = Res.changeString(str2);
                        string empty = string.Empty;
                        Char char14 = null;
                        sbyte b40 = 0;
                        if (!text5.Equals(string.Empty))
                        {
                            char14 = new Char();
                            char14.charID = msg.reader().readInt();
                            char14.head = msg.reader().readShort();
                            char14.headICON = msg.reader().readShort();
                            char14.body = msg.reader().readShort();
                            char14.bag = msg.reader().readShort();
                            char14.leg = msg.reader().readShort();
                            b40 = msg.reader().readByte();
                            char14.cName = text5;
                        }
                        empty += str2;
                        InfoDlg.hide();
                        if (text5.Equals(string.Empty))
                        {
                            GameScr.info1.addInfo(empty, 0);
                            NRO_v247.Mods.Notifications.NotifyCatcher.CatchNpcMessage(string.Empty, empty, null);
                            break;
                        }
                        GameScr.info2.addInfoWithChar(empty, char14, (b40 == 0) ? true : false);
                        NRO_v247.Mods.Notifications.NotifyCatcher.CatchNpcMessage(char14.cName, empty, char14);
                        if (GameCanvas.panel.isShow && GameCanvas.panel.type == 8)
                        {
                            GameCanvas.panel.initLogMessage();
                        }
                        break;
                    }
                case -26:
                {
                    ServerListScreen.testConnect = 2;
                    GameCanvas.debug("SA2", 2);
                    string text7 = msg.reader().readUTF();
                    GameCanvas.startOKDlg(text7);
                    InfoDlg.hide();
                    LoginScr.isContinueToLogin = false;
                    Char.isLoadingMap = false;
                    if (GameCanvas.currentScreen == GameCanvas.loginScr)
                    {
                        GameCanvas.serverScreen.switchToMe();
                    }
                    break;
                }
                case -25:
                    {
                        GameCanvas.debug("SA3", 2);
                        string txt25 = msg.reader().readUTF();
                        GameScr.info1.addInfo(txt25, 0);
                        NRO_v247.Mods.Notifications.NotifyCatcher.CatchServerMessage(txt25);
                        break;
                    }
                case 94:
                    {
                        GameCanvas.debug("SA3", 2);
                        string txt94 = msg.reader().readUTF();
                        GameScr.info1.addInfo(txt94, 0);
                        NRO_v247.Mods.Notifications.NotifyCatcher.CatchServerMessage(txt94);
                        break;
                    }
                case 47:
                    GameCanvas.debug("SA4", 2);
                    GameScr.gI().resetButton();
                    break;
                case 81:
                    {
                        GameCanvas.debug("SXX4", 2);
                        Mob mob13 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        mob13.isDisable = msg.reader().readBool();
                        break;
                    }
                case 82:
                    {
                        GameCanvas.debug("SXX5", 2);
                        Mob mob12 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        mob12.isDontMove = msg.reader().readBool();
                        break;
                    }
                case 85:
                    {
                        GameCanvas.debug("SXX5", 2);
                        Mob mob11 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        mob11.isFire = msg.reader().readBool();
                        break;
                    }
                case 86:
                    {
                        GameCanvas.debug("SXX5", 2);
                        Mob mob10 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        mob10.isIce = msg.reader().readBool();
                        if (!mob10.isIce)
                        {
                            ServerEffect.addServerEffect(77, mob10.x, mob10.y - 9, 1);
                        }
                        break;
                    }
                case 87:
                    {
                        GameCanvas.debug("SXX5", 2);
                        Mob mob9 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        mob9.isWind = msg.reader().readBool();
                        break;
                    }
                case 56:
                    {
                        GameCanvas.debug("SXX6", 2);
                        @char = null;
                        int num130 = msg.reader().readInt();
                        if (num130 == Char.myCharz().charID)
                        {
                            bool flag6 = false;
                            @char = Char.myCharz();
                            @char.cHP = msg.reader().readLong();
                            long num137 = msg.reader().readLong();
                            Res.outz("dame hit = " + num137);
                            if (num137 != 0)
                            {
                                @char.doInjure();
                            }
                            int num138 = 0;
                            try
                            {
                                flag6 = msg.reader().readBoolean();
                                sbyte b13 = msg.reader().readByte();
                                if (b13 != -1)
                                {
                                    Res.outz("hit eff= " + b13);
                                    EffecMn.addEff(new Effect(b13, @char.cx, @char.cy, 3, 1, -1));
                                }
                            }
                            catch (Exception)
                            {
                            }
                            num137 += num138;
                            if (Char.myCharz().cTypePk != 4)
                            {
                                if (num137 == 0)
                                {
                                    GameScr.startFlyText(mResources.miss, @char.cx, @char.cy - @char.ch, 0, -3, mFont.MISS_ME);
                                }
                                else
                                {
                                    GameScr.startFlyText("-" + num137, @char.cx, @char.cy - @char.ch, 0, -3, flag6 ? mFont.FATAL : mFont.RED);
                                }
                            }
                            break;
                        }
                        @char = GameScr.findCharInMap(num130);
                        if (@char == null)
                        {
                            return;
                        }
                        @char.cHP = msg.reader().readLong();
                        bool flag7 = false;
                        long num139 = msg.reader().readLong();
                        if (num139 != 0)
                        {
                            @char.doInjure();
                        }
                        int num140 = 0;
                        try
                        {
                            flag7 = msg.reader().readBoolean();
                            sbyte b14 = msg.reader().readByte();
                            if (b14 != -1)
                            {
                                Res.outz("hit eff= " + b14);
                                EffecMn.addEff(new Effect(b14, @char.cx, @char.cy, 3, 1, -1));
                            }
                        }
                        catch (Exception)
                        {
                        }
                        num139 += num140;
                        if (@char.cTypePk != 4)
                        {
                            if (num139 == 0)
                            {
                                GameScr.startFlyText(mResources.miss, @char.cx, @char.cy - @char.ch, 0, -3, mFont.MISS);
                            }
                            else
                            {
                                GameScr.startFlyText("-" + num139, @char.cx, @char.cy - @char.ch, 0, -3, flag7 ? mFont.FATAL : mFont.ORANGE);
                            }
                        }
                        break;
                    }
                case 83:
                    {
                        GameCanvas.debug("SXX8", 2);
                        int num129 = msg.reader().readInt();
                        @char = ((num129 != Char.myCharz().charID) ? GameScr.findCharInMap(num129) : Char.myCharz());
                        if (@char == null)
                        {
                            return;
                        }
                        Mob mobToAttack = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        if (@char.mobMe != null)
                        {
                            @char.mobMe.attackOtherMob(mobToAttack);
                        }
                        break;
                    }
                case 84:
                    {
                        int num128 = msg.reader().readInt();
                        if (num128 == Char.myCharz().charID)
                        {
                            @char = Char.myCharz();
                        }
                        else
                        {
                            @char = GameScr.findCharInMap(num128);
                            if (@char == null)
                            {
                                return;
                            }
                        }
                        @char.cHP = @char.cHPFull;
                        @char.cMP = @char.cMPFull;
                        @char.cx = msg.reader().readShort();
                        @char.cy = msg.reader().readShort();
                        @char.liveFromDead();
                        break;
                    }
                case 46:
                    GameCanvas.debug("SA5", 2);
                    Cout.LogWarning("Controler RESET_POINT  " + Char.ischangingMap);
                    Char.isLockKey = false;
                    Char.myCharz().setResetPoint(msg.reader().readShort(), msg.reader().readShort());
                    break;
                case -29:
                    messageNotLogin(msg);
                    break;
                case -28:
                    messageNotMap(msg);
                    break;
                case -30:
                    messageSubCommand(msg);
                    break;
                case 62:
                    GameCanvas.debug("SZ3", 2);
                    @char = GameScr.findCharInMap(msg.reader().readInt());
                    if (@char != null)
                    {
                        @char.killCharId = Char.myCharz().charID;
                        Char.myCharz().npcFocus = null;
                        Char.myCharz().mobFocus = null;
                        Char.myCharz().itemFocus = null;
                        Char.myCharz().charFocus = @char;
                        Char.isManualFocus = true;
                        GameScr.info1.addInfo(@char.cName + mResources.CUU_SAT, 0);
                    }
                    break;
                case 63:
                    GameCanvas.debug("SZ4", 2);
                    Char.myCharz().killCharId = msg.reader().readInt();
                    Char.myCharz().npcFocus = null;
                    Char.myCharz().mobFocus = null;
                    Char.myCharz().itemFocus = null;
                    Char.myCharz().charFocus = GameScr.findCharInMap(Char.myCharz().killCharId);
                    Char.isManualFocus = true;
                    break;
                case 64:
                    GameCanvas.debug("SZ5", 2);
                    @char = Char.myCharz();
                    try
                    {
                        @char = GameScr.findCharInMap(msg.reader().readInt());
                    }
                    catch (Exception ex27)
                    {
                        Cout.println("Loi CLEAR_CUU_SAT " + ex27.ToString());
                    }
                    @char.killCharId = -9999;
                    break;
                case 39:
                    GameCanvas.debug("SA49", 2);
                    GameScr.gI().typeTradeOrder = 2;
                    if (GameScr.gI().typeTrade >= 2 && GameScr.gI().typeTradeOrder >= 2)
                    {
                        InfoDlg.showWait();
                    }
                    break;
                case 57:
                    {
                        GameCanvas.debug("SZ6", 2);
                        MyVector myVector2 = new MyVector();
                        myVector2.addElement(new Command(msg.reader().readUTF(), GameCanvas.instance, 88817, null));
                        GameCanvas.menu.startAt(myVector2, 3);
                        break;
                    }
                case 58:
                    {
                        GameCanvas.debug("SZ7", 2);
                        int num127 = msg.reader().readInt();
                        Char char3 = ((num127 != Char.myCharz().charID) ? GameScr.findCharInMap(num127) : Char.myCharz());
                        char3.moveFast = new short[3];
                        char3.moveFast[0] = 0;
                        short num80 = msg.reader().readShort();
                        short num81 = msg.reader().readShort();
                        char3.moveFast[1] = num80;
                        char3.moveFast[2] = num81;
                        try
                        {
                            num127 = msg.reader().readInt();
                            Char char4 = ((num127 != Char.myCharz().charID) ? GameScr.findCharInMap(num127) : Char.myCharz());
                            char4.cx = num80;
                            char4.cy = num81;
                        }
                        catch (Exception ex24)
                        {
                            Cout.println("Loi MOVE_FAST " + ex24.ToString());
                        }
                        break;
                    }
                case 88:
                    {
                        string info4 = msg.reader().readUTF();
                        short num78 = msg.reader().readShort();
                        GameCanvas.inputDlg.show(info4, new Command(mResources.ACCEPT, GameCanvas.instance, 88818, num78), TField.INPUT_TYPE_ANY);
                        break;
                    }
                case 27:
                    {
                        myVector = new MyVector();
                        string text8 = msg.reader().readUTF();
                        int num69 = msg.reader().readByte();
                        for (int num70 = 0; num70 < num69; num70++)
                        {
                            string caption4 = msg.reader().readUTF();
                            short num71 = msg.reader().readShort();
                            myVector.addElement(new Command(caption4, GameCanvas.instance, 88819, num71));
                        }
                        GameCanvas.menu.startWithoutCloseButton(myVector, 3);
                        break;
                    }
                case 33:
                    {
                        GameCanvas.debug("SA51", 2);
                        InfoDlg.hide();
                        GameCanvas.clearKeyHold();
                        GameCanvas.clearKeyPressed();
                        myVector = new MyVector();
                        try
                        {
                            while (true)
                            {
                                string caption3 = msg.reader().readUTF();
                                myVector.addElement(new Command(caption3, GameCanvas.instance, 88822, null));
                            }
                        }
                        catch (Exception ex23)
                        {
                            Cout.println("Loi OPEN_UI_MENU " + ex23.ToString());
                        }
                        if (Char.myCharz().npcFocus == null)
                        {
                            return;
                        }
                        for (int num65 = 0; num65 < Char.myCharz().npcFocus.template.menu.Length; num65++)
                        {
                            string[] array9 = Char.myCharz().npcFocus.template.menu[num65];
                            myVector.addElement(new Command(array9[0], GameCanvas.instance, 88820, array9));
                        }
                        GameCanvas.menu.startAt(myVector, 3);
                        break;
                    }
                case 40:
                    {
                        GameCanvas.debug("SA52", 2);
                        GameCanvas.taskTick = 150;
                        short taskId = msg.reader().readShort();
                        sbyte index3 = msg.reader().readByte();
                        string str3 = msg.reader().readUTF();
                        str3 = Res.changeString(str3);
                        string str4 = msg.reader().readUTF();
                        str4 = Res.changeString(str4);
                        string[] array5 = new string[msg.reader().readByte()];
                        string[] array6 = new string[array5.Length];
                        GameScr.tasks = new int[array5.Length];
                        GameScr.mapTasks = new int[array5.Length];
                        short[] array7 = new short[array5.Length];
                        short num54 = -1;
                        for (int num55 = 0; num55 < array5.Length; num55++)
                        {
                            string str5 = msg.reader().readUTF();
                            str5 = Res.changeString(str5);
                            GameScr.tasks[num55] = msg.reader().readByte();
                            GameScr.mapTasks[num55] = msg.reader().readShort();
                            string str6 = msg.reader().readUTF();
                            str6 = Res.changeString(str6);
                            array7[num55] = -1;
                            array5[num55] = str5;
                            if (!str6.Equals(string.Empty))
                            {
                                array6[num55] = str6;
                            }
                        }
                        try
                        {
                            num54 = msg.reader().readShort();
                            Cout.println(" TASK_GET count:" + num54);
                            for (int num56 = 0; num56 < array5.Length; num56++)
                            {
                                array7[num56] = msg.reader().readShort();
                                Cout.println(num56 + " i TASK_GET   counts[i]:" + array7[num56]);
                            }
                        }
                        catch (Exception ex22)
                        {
                            Cout.println("Loi TASK_GET " + ex22.ToString());
                        }
                        Char.myCharz().taskMaint = new Task(taskId, index3, str3, str4, array5, array7, num54, array6);
                        if (Char.myCharz().npcFocus != null)
                        {
                            Npc.clearEffTask();
                        }
                        Char.taskAction(isNextStep: true);
                        break;
                    }
                case 41:
                    {
                        GameCanvas.debug("SA53", 2);
                        GameCanvas.taskTick = 100;
                        Res.outz("TASK NEXT");
                        Task taskMaint = Char.myCharz().taskMaint;
                        taskMaint.index++;
                        Char.myCharz().taskMaint.count = 0;
                        Npc.clearEffTask();
                        Char.taskAction(isNextStep: true);
                        break;
                    }
                case 50:
                    {
                        sbyte b55 = msg.reader().readByte();
                        PanelG.vGameInfo.removeAllElements();
                        for (int num51 = 0; num51 < b55; num51++)
                        {
                            GameInfo gameInfo = new GameInfo();
                            gameInfo.id = msg.reader().readShort();
                            gameInfo.main = msg.reader().readUTF();
                            gameInfo.content = msg.reader().readUTF();
                            PanelG.vGameInfo.addElement(gameInfo);
                            bool flag3 = (gameInfo.hasRead = Rms.loadRMSInt(gameInfo.id + string.Empty) != -1);
                        }
                        break;
                    }
                case 43:
                    GameCanvas.taskTick = 50;
                    GameCanvas.debug("SA55", 2);
                    Char.myCharz().taskMaint.count = msg.reader().readShort();
                    if (Char.myCharz().npcFocus != null)
                    {
                        Npc.clearEffTask();
                    }
                    try
                    {
                        short x_hint = msg.reader().readShort();
                        short y_hint = msg.reader().readShort();
                        Char.myCharz().x_hint = x_hint;
                        Char.myCharz().y_hint = y_hint;
                    }
                    catch (Exception)
                    {
                    }
                    break;
                case 90:
                    GameCanvas.debug("SA577", 2);
                    requestItemPlayer(msg);
                    break;
                case 29:
                    GameCanvas.debug("SA58", 2);
                    GameScr.gI().openUIZone(msg);
                    break;
                case -21:
                    {
                        GameCanvas.debug("SA60", 2);
                        short itemMapID4 = msg.reader().readShort();
                        for (int num44 = 0; num44 < GameScr.vItemMap.size(); num44++)
                        {
                            if (((ItemMap)GameScr.vItemMap.elementAt(num44)).itemMapID == itemMapID4)
                            {
                                GameScr.vItemMap.removeElementAt(num44);
                                break;
                            }
                        }
                        break;
                    }
                case -20:
                    {
                        GameCanvas.debug("SA61", 2);
                        Char.myCharz().itemFocus = null;
                        short itemMapID3 = msg.reader().readShort();
                        for (int num42 = 0; num42 < GameScr.vItemMap.size(); num42++)
                        {
                            ItemMap itemMap4 = (ItemMap)GameScr.vItemMap.elementAt(num42);
                            if (itemMap4.itemMapID != itemMapID3)
                            {
                                continue;
                            }
                            itemMap4.setPoint(Char.myCharz().cx, Char.myCharz().cy - 10);
                            string text6 = msg.reader().readUTF();
                            num = 0;
                            try
                            {
                                num = msg.reader().readShort();
                                if (itemMap4.template.type == 9)
                                {
                                    num = msg.reader().readShort();
                                    Char char17 = Char.myCharz();
                                    char17.xu += num;
                                    Char.myCharz().xuStr = Res.formatNumber(Char.myCharz().xu);
                                }
                                else if (itemMap4.template.type == 10)
                                {
                                    num = msg.reader().readShort();
                                    Char char17 = Char.myCharz();
                                    char17.luong += num;
                                    Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
                                }
                                else if (itemMap4.template.type == 34)
                                {
                                    num = msg.reader().readShort();
                                    Char char17 = Char.myCharz();
                                    char17.luongKhoa += num;
                                    Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
                                }
                            }
                            catch (Exception)
                            {
                            }
                            if (text6.Equals(string.Empty))
                            {
                                if (itemMap4.template.type == 9)
                                {
                                    GameScr.startFlyText(((num >= 0) ? "+" : string.Empty) + num, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -2, mFont.YELLOW);
                                    SoundMn.gI().getItem();
                                }
                                else if (itemMap4.template.type == 10)
                                {
                                    GameScr.startFlyText(((num >= 0) ? "+" : string.Empty) + num, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -2, mFont.GREEN);
                                    SoundMn.gI().getItem();
                                }
                                else if (itemMap4.template.type == 34)
                                {
                                    GameScr.startFlyText(((num >= 0) ? "+" : string.Empty) + num, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -2, mFont.RED);
                                    SoundMn.gI().getItem();
                                }
                                else
                                {
                                    GameScr.info1.addInfo(mResources.you_receive + " " + ((num <= 0) ? string.Empty : (num + " ")) + itemMap4.template.name, 0);
                                    SoundMn.gI().getItem();
                                }
                                if (num > 0 && Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 4683)
                                {
                                    ServerEffect.addServerEffect(55, Char.myCharz().petFollow.cmx, Char.myCharz().petFollow.cmy, 1);
                                    ServerEffect.addServerEffect(55, Char.myCharz().cx, Char.myCharz().cy, 1);
                                }
                            }
                            else if (text6.Length == 1)
                            {
                                Cout.LogError3("strInf.Length =1:  " + text6);
                            }
                            else
                            {
                                GameScr.info1.addInfo(text6, 0);
                            }
                            break;
                        }
                        break;
                    }
                case -19:
                    {
                        GameCanvas.debug("SA62", 2);
                        short itemMapID2 = msg.reader().readShort();
                        @char = GameScr.findCharInMap(msg.reader().readInt());
                        for (int num38 = 0; num38 < GameScr.vItemMap.size(); num38++)
                        {
                            ItemMap itemMap3 = (ItemMap)GameScr.vItemMap.elementAt(num38);
                            if (itemMap3.itemMapID != itemMapID2)
                            {
                                continue;
                            }
                            if (@char == null)
                            {
                                return;
                            }
                            itemMap3.setPoint(@char.cx, @char.cy - 10);
                            if (itemMap3.x < @char.cx)
                            {
                                @char.cdir = -1;
                            }
                            else if (itemMap3.x > @char.cx)
                            {
                                @char.cdir = 1;
                            }
                            break;
                        }
                        break;
                    }
                case -18:
                    {
                        GameCanvas.debug("SA63", 2);
                        int num37 = msg.reader().readByte();
                        GameScr.vItemMap.addElement(new ItemMap(msg.reader().readShort(), Char.myCharz().arrItemBag[num37].template.id, Char.myCharz().cx, Char.myCharz().cy, msg.reader().readShort(), msg.reader().readShort()));
                        Char.myCharz().arrItemBag[num37] = null;
                        break;
                    }
                case 68:
                    {
                        Res.outz("ADD ITEM TO MAP --------------------------------------");
                        GameCanvas.debug("SA6333", 2);
                        short itemMapID = msg.reader().readShort();
                        short itemTemplateID = msg.reader().readShort();
                        int x = msg.reader().readShort();
                        int y = msg.reader().readShort();
                        int num18 = msg.reader().readInt();
                        short r = 0;
                        if (num18 == -2)
                        {
                            r = msg.reader().readShort();
                        }
                        ItemMap itemMap = new ItemMap(num18, itemMapID, itemTemplateID, x, y, r);
                        bool flag11 = false;
                        for (int num19 = 0; num19 < GameScr.vItemMap.size(); num19++)
                        {
                            ItemMap itemMap2 = (ItemMap)GameScr.vItemMap.elementAt(num19);
                            if (itemMap2.itemMapID == itemMap.itemMapID)
                            {
                                flag11 = true;
                                break;
                            }
                        }
                        if (!flag11)
                        {
                            GameScr.vItemMap.addElement(itemMap);
                        }
                        break;
                    }
                case 69:
                    SoundMn.IsDelAcc = ((msg.reader().readByte() != 0) ? true : false);
                    break;
                case -14:
                    GameCanvas.debug("SA64", 2);
                    @char = GameScr.findCharInMap(msg.reader().readInt());
                    if (@char == null)
                    {
                        return;
                    }
                    GameScr.vItemMap.addElement(new ItemMap(msg.reader().readShort(), msg.reader().readShort(), @char.cx, @char.cy, msg.reader().readShort(), msg.reader().readShort()));
                    break;
                case -22:
                    GameCanvas.debug("SA65", 2);
                    Char.isLockKey = true;
                    Char.ischangingMap = true;
                    GameScr.gI().timeStartMap = 0;
                    GameScr.gI().timeLengthMap = 0;
                    Char.myCharz().mobFocus = null;
                    Char.myCharz().npcFocus = null;
                    Char.myCharz().charFocus = null;
                    Char.myCharz().itemFocus = null;
                    Char.myCharz().focus.removeAllElements();
                    Char.myCharz().testCharId = -9999;
                    Char.myCharz().killCharId = -9999;
                    GameCanvas.resetBg();
                    GameScr.gI().resetButton();
                    GameScr.gI().center = null;
                    if (Effect.vEffData.size() > 15)
                    {
                        for (int num17 = 0; num17 < 5; num17++)
                        {
                            Effect.vEffData.removeElementAt(0);
                        }
                    }
                    break;
                case -70:
                    {
                        Res.outz("BIG MESSAGE .......................................");
                        GameCanvas.endDlg();
                        int avatar = msg.reader().readShort();
                        string chat3 = msg.reader().readUTF();
                        GameScr.info1.addInfo(chat3, 0);
                        
                        sbyte b38 = msg.reader().readByte();
                        if (b38 == 1)
                        {
                            string p2 = msg.reader().readUTF();
                            string caption2 = msg.reader().readUTF();
                        }
                        break;
                    }
                case 38:
                    {
                        GameCanvas.debug("SA67", 2);
                        InfoDlg.hide();
                        int num175 = msg.reader().readShort();
                        Res.outz("OPEN_UI_SAY ID= " + num175);
                        string str = msg.reader().readUTF();
                        str = Res.changeString(str);
                        for (int num10 = 0; num10 < GameScr.vNpc.size(); num10++)
                        {
                            Npc npc4 = (Npc)GameScr.vNpc.elementAt(num10);
                            Res.outz("npc id= " + npc4.template.npcTemplateId);
                            if (npc4.template.npcTemplateId == num175)
                            {
                                ChatPopup.addChatPopupMultiLine(str, 100000, npc4);
                                GameCanvas.panel.hideNow();
                                return;
                            }
                        }
                        Npc npc5 = new Npc(num175, 0, 0, 0, num175, GameScr.info1.charId[Char.myCharz().cgender][2]);
                        if (npc5.template.npcTemplateId == 5)
                        {
                            npc5.charID = 5;
                        }
                        try
                        {
                            npc5.avatar = msg.reader().readShort();
                        }
                        catch (Exception)
                        {
                        }
                        ChatPopup.addChatPopupMultiLine(str, 100000, npc5);
                        GameCanvas.panel.hideNow();
                        break;
                    }
                case 32:
                    {
                        GameCanvas.debug("SA68", 2);
                        int num174 = msg.reader().readShort();
                        for (int num176 = 0; num176 < GameScr.vNpc.size(); num176++)
                        {
                            Npc npc2 = (Npc)GameScr.vNpc.elementAt(num176);
                            if (npc2.template.npcTemplateId == num174 && npc2.Equals(Char.myCharz().npcFocus))
                            {
                                string chat = msg.reader().readUTF();
                                string[] array15 = new string[msg.reader().readByte()];
                                for (int num177 = 0; num177 < array15.Length; num177++)
                                {
                                    array15[num177] = msg.reader().readUTF();
                                }
                                GameScr.gI().createMenu(array15, npc2);
                                ChatPopup.addChatPopup(chat, 100000, npc2);
                                return;
                            }
                        }
                        Npc npc3 = new Npc(num174, 0, -100, 100, num174, GameScr.info1.charId[Char.myCharz().cgender][2]);
                        Res.outz((Char.myCharz().npcFocus == null) ? "null" : "!null");
                        string chat2 = msg.reader().readUTF();
                        string[] array16 = new string[msg.reader().readByte()];
                        for (int num178 = 0; num178 < array16.Length; num178++)
                        {
                            array16[num178] = msg.reader().readUTF();
                        }
                        try
                        {
                            short num180 = (short)(npc3.avatar = msg.reader().readShort());
                        }
                        catch (Exception)
                        {
                        }
                        Res.outz((Char.myCharz().npcFocus == null) ? "null" : "!null");
                        GameScr.gI().createMenu(array16, npc3);
                        ChatPopup.addChatPopup(chat2, 100000, npc3);
                        break;
                    }
                case 7:
                    {
                        sbyte type = msg.reader().readByte();
                        short id2 = msg.reader().readShort();
                        string info2 = msg.reader().readUTF();
                        GameCanvas.panel.saleRequest(type, info2, id2);
                        break;
                    }
                case 6:
                    GameCanvas.debug("SA70", 2);
                    Char.myCharz().xu = msg.reader().readLong();
                    Char.myCharz().luong = msg.reader().readInt();
                    Char.myCharz().luongKhoa = msg.reader().readInt();
                    Char.myCharz().xuStr = Res.formatNumber(Char.myCharz().xu);
                    Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
                    Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
                    GameCanvas.endDlg();
                    break;
                case -24:
                    Res.outz("***************MAP_INFO**************");
                    GameScr.isPickNgocRong = false;
                    Char.isLoadingMap = true;
                    Cout.println("GET MAP INFO");
                    GameScr.gI().magicTree = null;
                    GameCanvas.isLoading = true;
                    GameCanvas.debug("SA75", 2);
                    GameScr.resetAllvector();
                    GameCanvas.endDlg();
                    TileMap.vGo.removeAllElements();
                    PopUp.vPopups.removeAllElements();
                    mSystem.gcc();
                    TileMap.mapID = msg.reader().readUnsignedByte();
                    TileMap.planetID = msg.reader().readByte();
                    TileMap.tileID = msg.reader().readByte();
                    TileMap.bgID = msg.reader().readByte();
                    GameScr.isPaint_CT = TileMap.mapID != 170;
                    Cout.println("load planet from server: " + TileMap.planetID + "bgType= " + TileMap.bgType + ".............................");
                    TileMap.typeMap = msg.reader().readByte();
                    TileMap.mapName = msg.reader().readUTF();
                    TileMap.zoneID = msg.reader().readByte();
                    GameCanvas.debug("SA75x1", 2);
                    try
                    {
                        TileMap.loadMapFromResource(TileMap.mapID);
                    }
                    catch (Exception)
                    {
                        Service.gI().requestMaptemplate(TileMap.mapID);
                        messWait = msg;
                        break;
                    }
                    loadInfoMap(msg);
                    try
                    {
                        TileMap.isMapDouble = ((msg.reader().readByte() != 0) ? true : false);
                    }
                    catch (Exception)
                    {
                    }
                    GameScr.cmx = GameScr.cmtoX;
                    GameScr.cmy = GameScr.cmtoY;
                    GameCanvas.isRequestMapID = 2;
                    GameCanvas.waitingTimeChangeMap = mSystem.currentTimeMillis() + 1000;
                    break;
                case -31:
                    {
                        TileMap.vItemBg.removeAllElements();
                        short num158 = msg.reader().readShort();
                        Res.err("[ITEM_BACKGROUND] nItem= " + num158);
                        for (int num159 = 0; num159 < num158; num159++)
                        {
                            BgItem bgItem = new BgItem();
                            bgItem.id = num159;
                            bgItem.idImage = msg.reader().readShort();
                            bgItem.layer = msg.reader().readByte();
                            bgItem.dx = msg.reader().readShort();
                            bgItem.dy = msg.reader().readShort();
                            sbyte b19 = msg.reader().readByte();
                            bgItem.tileX = new int[b19];
                            bgItem.tileY = new int[b19];
                            for (int num160 = 0; num160 < b19; num160++)
                            {
                                bgItem.tileX[num159] = msg.reader().readByte();
                                bgItem.tileY[num159] = msg.reader().readByte();
                            }
                            TileMap.vItemBg.addElement(bgItem);
                        }
                        break;
                    }
                case -4:
                    {
                        GameCanvas.debug("SA76", 2);
                        @char = GameScr.findCharInMap(msg.reader().readInt());
                        if (@char == null)
                        {
                            return;
                        }
                        GameCanvas.debug("SA76v1", 2);
                        if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
                        {
                            @char.setSkillPaint(GameScr.sks[msg.reader().readUnsignedByte()], 0);
                        }
                        else
                        {
                            @char.setSkillPaint(GameScr.sks[msg.reader().readUnsignedByte()], 1);
                        }
                        GameCanvas.debug("SA76v2", 2);
                        @char.attMobs = new Mob[msg.reader().readByte()];
                        for (int num132 = 0; num132 < @char.attMobs.Length; num132++)
                        {
                            Mob mob8 = (Mob)GameScr.vMob.elementAt(msg.reader().readByte());
                            @char.attMobs[num132] = mob8;
                            if (num132 == 0)
                            {
                                if (@char.cx <= mob8.x)
                                {
                                    @char.cdir = 1;
                                }
                                else
                                {
                                    @char.cdir = -1;
                                }
                            }
                        }
                        GameCanvas.debug("SA76v3", 2);
                        @char.charFocus = null;
                        @char.mobFocus = @char.attMobs[0];
                        Char[] array2 = new Char[10];
                        num = 0;
                        try
                        {
                            for (num = 0; num < array2.Length; num++)
                            {
                                int num126 = msg.reader().readInt();
                                Char char11 = (array2[num] = ((num126 != Char.myCharz().charID) ? GameScr.findCharInMap(num126) : Char.myCharz()));
                                if (num == 0)
                                {
                                    if (@char.cx <= char11.cx)
                                    {
                                        @char.cdir = 1;
                                    }
                                    else
                                    {
                                        @char.cdir = -1;
                                    }
                                }
                            }
                        }
                        catch (Exception ex30)
                        {
                            Cout.println("Loi PLAYER_ATTACK_N_P " + ex30.ToString());
                        }
                        GameCanvas.debug("SA76v4", 2);
                        if (num > 0)
                        {
                            @char.attChars = new Char[num];
                            for (num = 0; num < @char.attChars.Length; num++)
                            {
                                @char.attChars[num] = array2[num];
                            }
                            @char.charFocus = @char.attChars[0];
                            @char.mobFocus = null;
                        }
                        GameCanvas.debug("SA76v5", 2);
                        break;
                    }
                case 54:
                    {
                        @char = GameScr.findCharInMap(msg.reader().readInt());
                        if (@char == null)
                        {
                            return;
                        }
                        int num116 = msg.reader().readUnsignedByte();
                        if ((TileMap.tileTypeAtPixel(@char.cx, @char.cy) & 2) == 2)
                        {
                            @char.setSkillPaint(GameScr.sks[num116], 0);
                        }
                        else
                        {
                            @char.setSkillPaint(GameScr.sks[num116], 1);
                        }
                        Mob[] array12 = new Mob[10];
                        num = 0;
                        try
                        {
                            for (num = 0; num < array12.Length; num++)
                            {
                                Mob mob7 = (array12[num] = (Mob)GameScr.vMob.elementAt(msg.reader().readByte()));
                                if (num == 0)
                                {
                                    if (@char.cx <= mob7.x)
                                    {
                                        @char.cdir = 1;
                                    }
                                    else
                                    {
                                        @char.cdir = -1;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (num > 0)
                        {
                            @char.attMobs = new Mob[num];
                            for (num = 0; num < @char.attMobs.Length; num++)
                            {
                                @char.attMobs[num] = array12[num];
                            }
                            @char.charFocus = null;
                            @char.mobFocus = @char.attMobs[0];
                        }
                        break;
                    }
                case -60:
                    {
                        GameCanvas.debug("SA7666", 2);
                        int num108 = msg.reader().readInt();
                        int num119 = -1;
                        if (num108 != Char.myCharz().charID)
                        {
                            Char char9 = GameScr.findCharInMap(num108);
                            if (char9 == null)
                            {
                                return;
                            }
                            if (char9.currentMovePoint != null)
                            {
                                char9.createShadow(char9.cx, char9.cy, 10);
                                char9.cx = char9.currentMovePoint.xEnd;
                                char9.cy = char9.currentMovePoint.yEnd;
                            }
                            int num134 = msg.reader().readUnsignedByte();
                            if ((TileMap.tileTypeAtPixel(char9.cx, char9.cy) & 2) == 2)
                            {
                                char9.setSkillPaint(GameScr.sks[num134], 0);
                            }
                            else
                            {
                                char9.setSkillPaint(GameScr.sks[num134], 1);
                            }
                            sbyte b = msg.reader().readByte();
                            Char[] array = new Char[b];
                            for (num = 0; num < array.Length; num++)
                            {
                                num119 = msg.reader().readInt();
                                Char char10;
                                if (num119 == Char.myCharz().charID)
                                {
                                    char10 = Char.myCharz();
                                    if (!GameScr.isChangeZone && GameScr.isAutoPlay && GameScr.canAutoPlay)
                                    {
                                        Service.gI().requestChangeZone(-1, -1);
                                        GameScr.isChangeZone = true;
                                    }
                                }
                                else
                                {
                                    char10 = GameScr.findCharInMap(num119);
                                }
                                array[num] = char10;
                                if (num == 0)
                                {
                                    if (char9.cx <= char10.cx)
                                    {
                                        char9.cdir = 1;
                                    }
                                    else
                                    {
                                        char9.cdir = -1;
                                    }
                                }
                            }
                            if (num > 0)
                            {
                                char9.attChars = new Char[num];
                                for (num = 0; num < char9.attChars.Length; num++)
                                {
                                    char9.attChars[num] = array[num];
                                }
                                char9.mobFocus = null;
                                char9.charFocus = char9.attChars[0];
                            }
                        }
                        else
                        {
                            sbyte b11 = msg.reader().readByte();
                            sbyte b22 = msg.reader().readByte();
                            num119 = msg.reader().readInt();
                        }
                        try
                        {
                            sbyte b32 = msg.reader().readByte();
                            Res.outz("isRead continue = " + b32);
                            if (b32 != 1)
                            {
                                break;
                            }
                            sbyte b43 = msg.reader().readByte();
                            Res.outz("type skill = " + b43);
                            if (num119 == Char.myCharz().charID)
                            {
                                bool flag = false;
                                @char = Char.myCharz();
                                long num145 = msg.reader().readLong();
                                Res.outz("dame hit = " + num145);
                                @char.isDie = msg.reader().readBoolean();
                                if (@char.isDie)
                                {
                                    Char.isLockKey = true;
                                }
                                Res.outz("isDie=" + @char.isDie + "---------------------------------------");
                                int num156 = 0;
                                flag = (@char.isCrit = msg.reader().readBoolean());
                                @char.isMob = false;
                                num145 = (@char.damHP = num145 + num156);
                                if (b43 == 0)
                                {
                                    @char.doInjure(num145, 0L, flag, isMob: false);
                                }
                            }
                            else
                            {
                                @char = GameScr.findCharInMap(num119);
                                if (@char == null)
                                {
                                    return;
                                }
                                bool flag5 = false;
                                long num167 = msg.reader().readLong();
                                Res.outz("dame hit= " + num167);
                                @char.isDie = msg.reader().readBoolean();
                                Res.outz("isDie=" + @char.isDie + "---------------------------------------");
                                int num179 = 0;
                                flag5 = (@char.isCrit = msg.reader().readBoolean());
                                @char.isMob = false;
                                num167 = (@char.damHP = num167 + num179);
                                if (b43 == 0)
                                {
                                    @char.doInjure(num167, 0L, flag5, isMob: false);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        break;
                    }
            }
            switch (msg.command)
            {
                case -2:
                    {
                        GameCanvas.debug("SA77", 22);
                        int num107 = msg.reader().readInt();
                        Char char17 = Char.myCharz();
                        char17.yen += num107;
                        GameScr.startFlyText((num107 <= 0) ? (string.Empty + num107) : ("+" + num107), Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 10, 0, -2, mFont.YELLOW);
                        break;
                    }
                case 95:
                    {
                        GameCanvas.debug("SA77", 22);
                        int num93 = msg.reader().readInt();
                        Char char17 = Char.myCharz();
                        char17.xu += num93;
                        Char.myCharz().xuStr = Res.formatNumber(Char.myCharz().xu);
                        GameScr.startFlyText((num93 <= 0) ? (string.Empty + num93) : ("+" + num93), Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 10, 0, -2, mFont.YELLOW);
                        break;
                    }
                case 96:
                    GameCanvas.debug("SA77a", 22);
                    Char.myCharz().taskOrders.addElement(new TaskOrder(msg.reader().readByte(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readUTF(), msg.reader().readUTF(), msg.reader().readByte(), msg.reader().readByte()));
                    break;
                case 97:
                    {
                        sbyte b70 = msg.reader().readByte();
                        for (int num99 = 0; num99 < Char.myCharz().taskOrders.size(); num99++)
                        {
                            TaskOrder taskOrder = (TaskOrder)Char.myCharz().taskOrders.elementAt(num99);
                            if (taskOrder.taskId == b70)
                            {
                                taskOrder.count = msg.reader().readShort();
                                break;
                            }
                        }
                        break;
                    }
                case -1:
                    {
                        GameCanvas.debug("SA77", 222);
                        int num106 = msg.reader().readInt();
                        Char char17 = Char.myCharz();
                        char17.xu += num106;
                        Char.myCharz().xuStr = Res.formatNumber(Char.myCharz().xu);
                        char17 = Char.myCharz();
                        char17.yen -= num106;
                        GameScr.startFlyText("+" + num106, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 10, 0, -2, mFont.YELLOW);
                        break;
                    }
                case -3:
                    {
                        GameCanvas.debug("SA78", 2);
                        sbyte b66 = msg.reader().readByte();
                        int num89 = msg.reader().readInt();
                        if (b66 == 0)
                        {
                            Char char17 = Char.myCharz();
                            char17.cPower += num89;
                        }
                        if (b66 == 1)
                        {
                            Char char17 = Char.myCharz();
                            char17.cTiemNang += num89;
                        }
                        if (b66 == 2)
                        {
                            Char char17 = Char.myCharz();
                            char17.cPower += num89;
                            char17 = Char.myCharz();
                            char17.cTiemNang += num89;
                        }
                        Char.myCharz().applyCharLevelPercent();
                        if (Char.myCharz().cTypePk != 3)
                        {
                            GameScr.startFlyText(((num89 <= 0) ? string.Empty : "+") + num89, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch, 0, -4, mFont.GREEN);
                            if (num89 > 0 && Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 5002)
                            {
                                ServerEffect.addServerEffect(55, Char.myCharz().petFollow.cmx, Char.myCharz().petFollow.cmy, 1);
                                ServerEffect.addServerEffect(55, Char.myCharz().cx, Char.myCharz().cy, 1);
                            }
                        }
                        break;
                    }
                case -73:
                    {
                        sbyte b72 = msg.reader().readByte();
                        for (int num105 = 0; num105 < GameScr.vNpc.size(); num105++)
                        {
                            Npc npc7 = (Npc)GameScr.vNpc.elementAt(num105);
                            if (npc7.template.npcTemplateId == b72)
                            {
                                if (msg.reader().readByte() == 0)
                                {
                                    npc7.isHide = true;
                                }
                                else
                                {
                                    npc7.isHide = false;
                                }
                                break;
                            }
                        }
                        break;
                    }
                case -5:
                    {
                        GameCanvas.debug("SA79", 2);
                        int charID = msg.reader().readInt();
                        int num95 = msg.reader().readInt();
                        Char char8;
                        if (num95 != -100)
                        {
                            char8 = new Char();
                            char8.charID = charID;
                            char8.clanID = num95;
                        }
                        else
                        {
                            char8 = new Mabu();
                            char8.charID = charID;
                            char8.clanID = num95;
                        }
                        if (char8.clanID == -2)
                        {
                            char8.isCopy = true;
                        }
                        if (readCharInfo(char8, msg))
                        {
                            sbyte b68 = msg.reader().readByte();
                            if (char8.cy <= 10 && b68 != 0 && b68 != 2)
                            {
                                Res.outz("nhân vật bay trên trời xuống x= " + char8.cx + " y= " + char8.cy);
                                Teleport teleport2 = new Teleport(char8.cx, char8.cy, char8.head, char8.cdir, 1, isMe: false, (b68 != 1) ? b68 : char8.cgender);
                                teleport2.id = char8.charID;
                                char8.isTeleport = true;
                                Teleport.addTeleport(teleport2);
                            }
                            if (b68 == 2)
                            {
                                char8.show();
                            }
                            for (int num96 = 0; num96 < GameScr.vMob.size(); num96++)
                            {
                                Mob mob2 = (Mob)GameScr.vMob.elementAt(num96);
                                if (mob2 != null && mob2.isMobMe && mob2.mobId == char8.charID)
                                {
                                    Res.outz("co 1 con quai");
                                    char8.mobMe = mob2;
                                    char8.mobMe.x = char8.cx;
                                    char8.mobMe.y = char8.cy - 40;
                                    break;
                                }
                            }
                            if (GameScr.findCharInMap(char8.charID) == null)
                            {
                                GameScr.vCharInMap.addElement(char8);
                            }
                            char8.isMonkey = msg.reader().readByte();
                            short num97 = msg.reader().readShort();
                            Res.outz("mount id= " + num97 + "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                            if (num97 != -1)
                            {
                                char8.isHaveMount = true;
                                switch (num97)
                                {
                                    case 346:
                                    case 347:
                                    case 348:
                                        char8.isMountVip = false;
                                        break;
                                    case 349:
                                    case 350:
                                    case 351:
                                        char8.isMountVip = true;
                                        break;
                                    case 396:
                                        char8.isEventMount = true;
                                        break;
                                    case 532:
                                        char8.isSpeacialMount = true;
                                        break;
                                    default:
                                        if (num97 >= Char.ID_NEW_MOUNT)
                                        {
                                            char8.idMount = num97;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                char8.isHaveMount = false;
                            }
                        }
                        sbyte b69 = msg.reader().readByte();
                        Res.outz("addplayer:   " + b69);
                        char8.cFlag = b69;
                        char8.isNhapThe = msg.reader().readByte() == 1;
                        try
                        {
                            char8.idAuraEff = msg.reader().readShort();
                            char8.idEff_Set_Item = msg.reader().readSByte();
                            char8.idHat = msg.reader().readShort();
                            if (char8.bag >= 201 && char8.bag < 255)
                            {
                                Effect effect3 = new Effect(char8.bag, char8, 2, -1, 10, 1);
                                effect3.typeEff = 5;
                                char8.addEffChar(effect3);
                            }
                            else
                            {
                                for (int num98 = 0; num98 < 54; num98++)
                                {
                                    char8.removeEffChar(0, 201 + num98);
                                }
                            }
                        }
                        catch (Exception ex28)
                        {
                            Res.outz("cmd: -5 err: " + ex28.StackTrace);
                        }
                        GameScr.gI().getFlagImage(char8.charID, char8.cFlag);
                        break;
                    }
                case -7:
                    {
                        GameCanvas.debug("SA80", 2);
                        int num86 = msg.reader().readInt();
                        for (int num90 = 0; num90 < GameScr.vCharInMap.size(); num90++)
                        {
                            Char char7 = null;
                            try
                            {
                                char7 = (Char)GameScr.vCharInMap.elementAt(num90);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            if (char7 == null || char7.charID != num86)
                            {
                                continue;
                            }
                            GameCanvas.debug("SA8x2y" + num90, 2);
                            char7.moveTo(msg.reader().readShort(), msg.reader().readShort(), 0);
                            char7.lastUpdateTime = mSystem.currentTimeMillis();
                            break;
                        }
                        GameCanvas.debug("SA80x3", 2);
                        break;
                    }
                case -6:
                    {
                        GameCanvas.debug("SA81", 2);
                        int num87 = msg.reader().readInt();
                        for (int num88 = 0; num88 < GameScr.vCharInMap.size(); num88++)
                        {
                            Char char6 = (Char)GameScr.vCharInMap.elementAt(num88);
                            if (char6 != null && char6.charID == num87)
                            {
                                if (!char6.isInvisiblez && !char6.isUsePlane)
                                {
                                    ServerEffect.addServerEffect(60, char6.cx, char6.cy, 1);
                                }
                                if (!char6.isUsePlane)
                                {
                                    GameScr.vCharInMap.removeElementAt(num88);
                                }
                                return;
                            }
                        }
                        break;
                    }
                case -13:
                    {
                        GameCanvas.debug("SA82", 2);
                        int num100 = msg.reader().readUnsignedByte();
                        if (num100 > GameScr.vMob.size() - 1 || num100 < 0)
                        {
                            return;
                        }
                        Mob mob16 = (Mob)GameScr.vMob.elementAt(num100);
                        mob16.sys = msg.reader().readByte();
                        mob16.levelBoss = msg.reader().readByte();
                        if (mob16.levelBoss != 0)
                        {
                            mob16.typeSuperEff = Res.random(0, 3);
                        }
                        mob16.x = mob16.xFirst;
                        mob16.y = mob16.yFirst;
                        mob16.status = 5;
                        mob16.injureThenDie = false;
                        mob16.hp = msg.reader().readLong();
                        mob16.maxHp = mob16.hp;
                        mob16.updateHp_bar();
                        ServerEffect.addServerEffect(60, mob16.x, mob16.y, 1);
                        break;
                    }
                case -75:
                    {
                        Mob mob21 = null;
                        try
                        {
                            mob21 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        }
                        catch (Exception)
                        {
                        }
                        if (mob21 != null)
                        {
                            mob21.levelBoss = msg.reader().readByte();
                            if (mob21.levelBoss > 0)
                            {
                                mob21.typeSuperEff = Res.random(0, 3);
                            }
                        }
                        break;
                    }
                case -9:
                    {
                        GameCanvas.debug("SA83", 2);
                        Mob mob17 = null;
                        try
                        {
                            mob17 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        }
                        catch (Exception)
                        {
                        }
                        GameCanvas.debug("SA83v1", 2);
                        if (mob17 != null)
                        {
                            mob17.hp = msg.reader().readLong();
                            mob17.updateHp_bar();
                            long num92 = msg.reader().readLong();
                            if (num92 == 1)
                            {
                                return;
                            }
                            if (num92 > 1)
                            {
                                mob17.setInjure();
                            }
                            bool flag4 = false;
                            try
                            {
                                flag4 = msg.reader().readBoolean();
                            }
                            catch (Exception)
                            {
                            }
                            sbyte b67 = msg.reader().readByte();
                            if (b67 != -1)
                            {
                                EffecMn.addEff(new Effect(b67, mob17.x, mob17.getY(), 3, 1, -1));
                            }
                            GameCanvas.debug("SA83v2", 2);
                            if (flag4)
                            {
                                GameScr.startFlyText("-" + num92, mob17.x, mob17.getY() - mob17.getH(), 0, -2, mFont.FATAL);
                            }
                            else if (num92 == 0)
                            {
                                mob17.x = mob17.xFirst;
                                mob17.y = mob17.yFirst;
                                GameScr.startFlyText(mResources.miss, mob17.x, mob17.getY() - mob17.getH(), 0, -2, mFont.MISS);
                            }
                            else if (num92 > 1)
                            {
                                GameScr.startFlyText("-" + num92, mob17.x, mob17.getY() - mob17.getH(), 0, -2, mFont.ORANGE);
                            }
                        }
                        GameCanvas.debug("SA83v3", 2);
                        break;
                    }
                case 45:
                    {
                        GameCanvas.debug("SA84", 2);
                        Mob mob15 = null;
                        try
                        {
                            mob15 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        }
                        catch (Exception ex26)
                        {
                            Cout.println("Loi tai NPC_MISS  " + ex26.ToString());
                        }
                        if (mob15 != null)
                        {
                            mob15.hp = msg.reader().readLong();
                            mob15.updateHp_bar();
                            GameScr.startFlyText(mResources.miss, mob15.x, mob15.y - mob15.h, 0, -2, mFont.MISS);
                        }
                        break;
                    }
                case -12:
                    {
                        Res.outz("SERVER SEND MOB DIE");
                        GameCanvas.debug("SA85", 2);
                        Mob mob18 = null;
                        try
                        {
                            mob18 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        }
                        catch (Exception)
                        {
                            Cout.println("LOi tai NPC_DIE cmd " + msg.command);
                        }
                        if (mob18 == null || mob18.status == 0 || mob18.status == 0)
                        {
                            break;
                        }
                        mob18.startDie();
                        try
                        {
                            long num101 = msg.reader().readLong();
                            if (msg.reader().readBool())
                            {
                                GameScr.startFlyText("-" + num101, mob18.x, mob18.y - mob18.h, 0, -2, mFont.FATAL);
                            }
                            else
                            {
                                GameScr.startFlyText("-" + num101, mob18.x, mob18.y - mob18.h, 0, -2, mFont.ORANGE);
                            }
                            sbyte b71 = msg.reader().readByte();
                            for (int num103 = 0; num103 < b71; num103++)
                            {
                                ItemMap itemMap6 = new ItemMap(msg.reader().readShort(), msg.reader().readShort(), mob18.x, mob18.y, msg.reader().readShort(), msg.reader().readShort());
                                int num104 = (itemMap6.playerId = msg.reader().readInt());
                                Res.outz("playerid= " + num104 + " my id= " + Char.myCharz().charID);
                                GameScr.vItemMap.addElement(itemMap6);
                                if (Res.abs(itemMap6.y - Char.myCharz().cy) < 24 && Res.abs(itemMap6.x - Char.myCharz().cx) < 24)
                                {
                                    Char.myCharz().charFocus = null;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        break;
                    }
                case 74:
                    {
                        GameCanvas.debug("SA85", 2);
                        Mob mob14 = null;
                        try
                        {
                            mob14 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        }
                        catch (Exception)
                        {
                            Cout.println("Loi tai NPC CHANGE " + msg.command);
                        }
                        if (mob14 != null && mob14.status != 0 && mob14.status != 0)
                        {
                            mob14.status = 0;
                            ServerEffect.addServerEffect(60, mob14.x, mob14.y, 1);
                            ItemMap itemMap5 = new ItemMap(msg.reader().readShort(), msg.reader().readShort(), mob14.x, mob14.y, msg.reader().readShort(), msg.reader().readShort());
                            GameScr.vItemMap.addElement(itemMap5);
                            if (Res.abs(itemMap5.y - Char.myCharz().cy) < 24 && Res.abs(itemMap5.x - Char.myCharz().cx) < 24)
                            {
                                Char.myCharz().charFocus = null;
                            }
                        }
                        break;
                    }
                case -11:
                    {
                        GameCanvas.debug("SA86", 2);
                        Mob mob19 = null;
                        try
                        {
                            int index4 = msg.reader().readUnsignedByte();
                            mob19 = (Mob)GameScr.vMob.elementAt(index4);
                        }
                        catch (Exception ex25)
                        {
                            Res.outz("Loi tai NPC_ATTACK_ME " + msg.command + " err= " + ex25.StackTrace);
                        }
                        if (mob19 != null)
                        {
                            Char.myCharz().isDie = false;
                            Char.isLockKey = false;
                            long num83 = msg.reader().readLong();
                            long num84;
                            try
                            {
                                num84 = msg.reader().readLong();
                            }
                            catch (Exception)
                            {
                                num84 = 0L;
                            }
                            if (mob19.isBusyAttackSomeOne)
                            {
                                Char.myCharz().doInjure(num83, num84, isCrit: false, isMob: true);
                                break;
                            }
                            mob19.dame = num83;
                            mob19.dameMp = num84;
                            mob19.setAttack(Char.myCharz());
                        }
                        break;
                    }
                case -10:
                    {
                        GameCanvas.debug("SA87", 2);
                        Mob mob20 = null;
                        try
                        {
                            mob20 = (Mob)GameScr.vMob.elementAt(msg.reader().readUnsignedByte());
                        }
                        catch (Exception)
                        {
                        }
                        GameCanvas.debug("SA87x1", 2);
                        if (mob20 != null)
                        {
                            GameCanvas.debug("SA87x2", 2);
                            @char = GameScr.findCharInMap(msg.reader().readInt());
                            if (@char == null)
                            {
                                return;
                            }
                            GameCanvas.debug("SA87x3", 2);
                            long num94 = msg.reader().readLong();
                            mob20.dame = @char.cHP - num94;
                            @char.cHPNew = num94;
                            GameCanvas.debug("SA87x4", 2);
                            try
                            {
                                @char.cMP = msg.reader().readLong();
                            }
                            catch (Exception)
                            {
                            }
                            GameCanvas.debug("SA87x5", 2);
                            if (mob20.isBusyAttackSomeOne)
                            {
                                @char.doInjure(mob20.dame, 0L, isCrit: false, isMob: true);
                            }
                            else
                            {
                                mob20.setAttack(@char);
                            }
                            GameCanvas.debug("SA87x6", 2);
                        }
                        break;
                    }
                case -17:
                    GameCanvas.debug("SA88", 2);
                    Char.myCharz().meDead = true;
                    Char.myCharz().cPk = msg.reader().readByte();
                    Char.myCharz().startDie(msg.reader().readShort(), msg.reader().readShort());
                    try
                    {
                        Char.myCharz().cPower = msg.reader().readLong();
                        Char.myCharz().applyCharLevelPercent();
                    }
                    catch (Exception)
                    {
                        Cout.println("Loi tai ME_DIE " + msg.command);
                    }
                    Char.myCharz().countKill = 0;
                    break;
                case 66:
                    Res.outz("ME DIE XP DOWN NOT IMPLEMENT YET!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    break;
                case -8:
                    GameCanvas.debug("SA89", 2);
                    @char = GameScr.findCharInMap(msg.reader().readInt());
                    if (@char == null)
                    {
                        return;
                    }
                    @char.cPk = msg.reader().readByte();
                    @char.waitToDie(msg.reader().readShort(), msg.reader().readShort());
                    break;
                case -16:
                    GameCanvas.debug("SA90", 2);
                    if (Char.myCharz().wdx != 0 || Char.myCharz().wdy != 0)
                    {
                        Char.myCharz().cx = Char.myCharz().wdx;
                        Char.myCharz().cy = Char.myCharz().wdy;
                        Char.myCharz().wdx = (Char.myCharz().wdy = 0);
                    }
                    Char.myCharz().liveFromDead();
                    Char.myCharz().isLockMove = false;
                    Char.myCharz().meDead = false;
                    break;
                case 44:
                    {
                        GameCanvas.debug("SA91", 2);
                        int num85 = msg.reader().readInt();
                        string text9 = msg.reader().readUTF();
                        Res.outz("user id= " + num85 + " text= " + text9);
                        @char = ((Char.myCharz().charID != num85) ? GameScr.findCharInMap(num85) : Char.myCharz());
                        if (@char == null)
                        {
                            return;
                        }
                        @char.addInfo(text9);
                        break;
                    }
                case 18:
                    {
                        sbyte b65 = msg.reader().readByte();
                        for (int num82 = 0; num82 < b65; num82++)
                        {
                            int charId = msg.reader().readInt();
                            int cx = msg.reader().readShort();
                            int cy = msg.reader().readShort();
                            long cHPShow = msg.reader().readLong();
                            Char char5 = GameScr.findCharInMap(charId);
                            if (char5 != null)
                            {
                                char5.cx = cx;
                                char5.cy = cy;
                                char5.cHP = (char5.cHPShow = cHPShow);
                                char5.lastUpdateTime = mSystem.currentTimeMillis();
                            }
                        }
                        break;
                    }
                case 19:
                    Char.myCharz().countKill = msg.reader().readUnsignedShort();
                    Char.myCharz().countKillMax = msg.reader().readUnsignedShort();
                    break;
            }
            GameCanvas.debug("SA92", 2);
        }
        catch (Exception ex29)
        {
            Res.err("[Controller] [error] " + ex29.StackTrace + " msg: " + ex29.Message + " cause " + ex29.Data);
        }
        finally
        {
            msg?.cleanup();
        }
    }

    private void readLogin(Message msg)
    {
        sbyte b = msg.reader().readByte();
        ChooseCharScr.playerData = new PlayerData[b];
        Res.outz("[LEN] sl nguoi choi " + b);
        for (int i = 0; i < b; i++)
        {
            int playerID = msg.reader().readInt();
            string name = msg.reader().readUTF();
            short head = msg.reader().readShort();
            short body = msg.reader().readShort();
            short leg = msg.reader().readShort();
            long ppoint = msg.reader().readLong();
            ChooseCharScr.playerData[i] = new PlayerData(playerID, name, head, body, leg, ppoint);
        }
        GameCanvas.chooseCharScr.switchToMe();
        GameCanvas.chooseCharScr.updateChooseCharacter((byte)b);
    }

    private void createSkill(myReader d)
    {
        GameScr.vcSkill = d.readByte();
        GameScr.gI().sOptionTemplates = new SkillOptionTemplate[d.readByte()];
        for (int i = 0; i < GameScr.gI().sOptionTemplates.Length; i++)
        {
            GameScr.gI().sOptionTemplates[i] = new SkillOptionTemplate();
            GameScr.gI().sOptionTemplates[i].id = i;
            GameScr.gI().sOptionTemplates[i].name = d.readUTF();
        }
        GameScr.nClasss = new NClass[d.readByte()];
        for (int j = 0; j < GameScr.nClasss.Length; j++)
        {
            GameScr.nClasss[j] = new NClass();
            GameScr.nClasss[j].classId = j;
            GameScr.nClasss[j].name = d.readUTF();
            GameScr.nClasss[j].skillTemplates = new SkillTemplate[d.readByte()];
            for (int k = 0; k < GameScr.nClasss[j].skillTemplates.Length; k++)
            {
                GameScr.nClasss[j].skillTemplates[k] = new SkillTemplate();
                GameScr.nClasss[j].skillTemplates[k].id = d.readByte();
                string skillName = d.readUTF();
                GameScr.nClasss[j].skillTemplates[k].name = "[" + GameScr.nClasss[j].skillTemplates[k].id + "] " + skillName;
                GameScr.nClasss[j].skillTemplates[k].maxPoint = d.readByte();
                GameScr.nClasss[j].skillTemplates[k].manaUseType = d.readByte();
                GameScr.nClasss[j].skillTemplates[k].type = d.readByte();
                GameScr.nClasss[j].skillTemplates[k].iconId = d.readShort();
                GameScr.nClasss[j].skillTemplates[k].damInfo = d.readUTF();
                int lineWidth = 130;
                if (GameCanvas.w == 128 || GameCanvas.h <= 208)
                {
                    lineWidth = 100;
                }
                GameScr.nClasss[j].skillTemplates[k].description = mFont.tahoma_7_green2.splitFontArray(d.readUTF(), lineWidth);
                GameScr.nClasss[j].skillTemplates[k].skills = new Skill[d.readByte()];
                for (int l = 0; l < GameScr.nClasss[j].skillTemplates[k].skills.Length; l++)
                {
                    GameScr.nClasss[j].skillTemplates[k].skills[l] = new Skill();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].skillId = d.readShort();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].template = GameScr.nClasss[j].skillTemplates[k];
                    GameScr.nClasss[j].skillTemplates[k].skills[l].point = d.readByte();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].powRequire = d.readLong();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].manaUse = d.readShort();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].coolDown = d.readInt();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].dx = d.readShort();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].dy = d.readShort();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].maxFight = d.readByte();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].damage = d.readShort();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].price = d.readShort();
                    GameScr.nClasss[j].skillTemplates[k].skills[l].moreInfo = d.readUTF();
                    Skills.add(GameScr.nClasss[j].skillTemplates[k].skills[l]);
                }
            }
        }
    }

    private void createMap(myReader d)
    {
        GameScr.vcMap = d.readByte();
        TileMap.mapNames = new string[d.readShort()];
        for (int i = 0; i < TileMap.mapNames.Length; i++)
        {
            TileMap.mapNames[i] = d.readUTF();
        }
        Npc.arrNpcTemplate = new NpcTemplate[d.readByte()];
        for (sbyte b = 0; b < Npc.arrNpcTemplate.Length; b = (sbyte)(b + 1))
        {
            Npc.arrNpcTemplate[b] = new NpcTemplate();
            Npc.arrNpcTemplate[b].npcTemplateId = b;
            Npc.arrNpcTemplate[b].name = d.readUTF();
            Npc.arrNpcTemplate[b].headId = d.readShort();
            Npc.arrNpcTemplate[b].bodyId = d.readShort();
            Npc.arrNpcTemplate[b].legId = d.readShort();
            Npc.arrNpcTemplate[b].menu = new string[d.readByte()][];
            for (int j = 0; j < Npc.arrNpcTemplate[b].menu.Length; j++)
            {
                Npc.arrNpcTemplate[b].menu[j] = new string[d.readByte()];
                for (int k = 0; k < Npc.arrNpcTemplate[b].menu[j].Length; k++)
                {
                    Npc.arrNpcTemplate[b].menu[j][k] = d.readUTF();
                }
            }
        }
        Mob.arrMobTemplate = new MobTemplate[d.readShort()];
        for (int l = 0; l < Mob.arrMobTemplate.Length; l++)
        {
            Mob.arrMobTemplate[l] = new MobTemplate();
            Mob.arrMobTemplate[l].mobTemplateId = l;
            Mob.arrMobTemplate[l].type = d.readByte();
            Mob.arrMobTemplate[l].name = d.readUTF();
            Mob.arrMobTemplate[l].hp = d.readLong();
            Mob.arrMobTemplate[l].rangeMove = d.readByte();
            Mob.arrMobTemplate[l].speed = d.readByte();
            Mob.arrMobTemplate[l].dartType = d.readByte();
        }
    }

    private void createData(myReader d, bool isSaveRMS)
    {
        GameScr.vcData = d.readByte();
        if (isSaveRMS)
        {
            Rms.saveRMS("NR_dart", NinjaUtil.readByteArray(d));
            Rms.saveRMS("NR_arrow", NinjaUtil.readByteArray(d));
            Rms.saveRMS("NR_effect", NinjaUtil.readByteArray(d));
            Rms.saveRMS("NR_image", NinjaUtil.readByteArray(d));
            Rms.saveRMS("NR_part", NinjaUtil.readByteArray(d));
            Rms.saveRMS("NR_skill", NinjaUtil.readByteArray(d));
            Rms.DeleteStorage("NRdata");
        }
    }

    private Image createImage(sbyte[] arr)
    {
        try
        {
            return Image.createImage(arr, 0, arr.Length);
        }
        catch (Exception)
        {
        }
        return null;
    }

    public int[] arrayByte2Int(sbyte[] b)
    {
        int[] array = new int[b.Length];
        for (int i = 0; i < b.Length; i++)
        {
            int num = b[i];
            if (num < 0)
            {
                num += 256;
            }
            array[i] = num;
        }
        return array;
    }

    public void readClanMsg(Message msg, int index)
    {
        try
        {
            ClanMessage clanMessage = new ClanMessage();
            sbyte b = (sbyte)(clanMessage.type = msg.reader().readByte());
            clanMessage.id = msg.reader().readInt();
            clanMessage.playerId = msg.reader().readInt();
            clanMessage.playerName = msg.reader().readUTF();
            clanMessage.role = msg.reader().readByte();
            clanMessage.time = msg.reader().readInt() + 1000000000;
            bool flag = false;
            GameScr.isNewClanMessage = false;
            switch (b)
            {
                case 0:
                    {
                        string text = msg.reader().readUTF();
                        GameScr.isNewClanMessage = true;
                        if (mFont.tahoma_7.getWidth(text) > PanelG.WIDTH_PANEL - 60)
                        {
                            clanMessage.chat = mFont.tahoma_7.splitFontArray(text, PanelG.WIDTH_PANEL - 10);
                        }
                        else
                        {
                            clanMessage.chat = new string[1];
                            clanMessage.chat[0] = text;
                        }
                        clanMessage.color = msg.reader().readByte();
                        break;
                    }
                case 1:
                    clanMessage.recieve = msg.reader().readByte();
                    clanMessage.maxCap = msg.reader().readByte();
                    flag = msg.reader().readByte() == 1;
                    if (flag)
                    {
                        GameScr.isNewClanMessage = true;
                    }
                    if (clanMessage.playerId != Char.myCharz().charID)
                    {
                        if (clanMessage.recieve < clanMessage.maxCap)
                        {
                            clanMessage.option = new string[1] { mResources.donate };
                        }
                        else
                        {
                            clanMessage.option = null;
                        }
                    }
                    if (GameCanvas.panel.cp != null)
                    {
                        GameCanvas.panel.updateRequest(clanMessage.recieve, clanMessage.maxCap);
                    }
                    break;
                case 2:
                    if (Char.myCharz().role == 0)
                    {
                        GameScr.isNewClanMessage = true;
                        clanMessage.option = new string[2]
                        {
                        mResources.CANCEL,
                        mResources.receive
                        };
                    }
                    break;
            }
            if (GameCanvas.currentScreen != GameScr.instance)
            {
                GameScr.isNewClanMessage = false;
            }
            else if (GameCanvas.panel.isShow && GameCanvas.panel.type == 0 && GameCanvas.panel.currentTabIndex == 3)
            {
                GameScr.isNewClanMessage = false;
            }
            ClanMessage.addMessage(clanMessage, index, flag);
        }
        catch (Exception)
        {
            Cout.println("LOI TAI CMD -= " + msg.command);
        }
    }

    public static void loadCurrMap(sbyte teleport3)
    {
        Res.outz("[CONTROLER] start load map " + teleport3);
        GameScr.gI().auto = 0;
        GameScr.isChangeZone = false;
        CreateCharScr.instance = null;
        GameScr.info1.isUpdate = false;
        GameScr.info2.isUpdate = false;
        GameScr.lockTick = 0;
        GameCanvas.panel.isShow = false;
        SoundMn.gI().stopAll();
        if (!GameScr.isLoadAllData && !CreateCharScr.isCreateChar)
        {
            GameScr.gI().initSelectChar();
        }
        GameScr.loadCamera(fullmScreen: false, (teleport3 != 1) ? (-1) : Char.myCharz().cx, (teleport3 == 0) ? (-1) : 0);
        TileMap.loadMainTile();
        TileMap.loadMap(TileMap.tileID);
        Res.outz("LOAD GAMESCR 2");
        Char.myCharz().cvx = 0;
        Char.myCharz().statusMe = 4;
        Char.myCharz().currentMovePoint = null;
        Char.myCharz().mobFocus = null;
        Char.myCharz().charFocus = null;
        Char.myCharz().npcFocus = null;
        Char.myCharz().itemFocus = null;
        Char.myCharz().skillPaint = null;
        Char.myCharz().setMabuHold(m: false);
        Char.myCharz().skillPaintRandomPaint = null;
        GameCanvas.clearAllPointerEvent();
        if (Char.myCharz().cy >= TileMap.pxh - 100)
        {
            Char.myCharz().isFlyUp = true;
            Char.myCharz().cx += Res.abs(Res.random(0, 80));
            Service.gI().charMove();
        }
        GameScr.gI().loadGameScr();
        GameCanvas.loadBG(TileMap.bgID);
        Char.isLockKey = false;
        Res.outz("cy= " + Char.myCharz().cy + "---------------------------------------------");
        for (int i = 0; i < Char.myCharz().vEff.size(); i++)
        {
            EffectChar effectChar = (EffectChar)Char.myCharz().vEff.elementAt(i);
            if (effectChar.template.type == 10)
            {
                Char.isLockKey = true;
                break;
            }
        }
        GameCanvas.clearKeyHold();
        GameCanvas.clearKeyPressed();
        GameScr.gI().dHP = Char.myCharz().cHP;
        GameScr.gI().dMP = Char.myCharz().cMP;
        Char.ischangingMap = false;
        GameScr.gI().switchToMe();
        if (Char.myCharz().cy <= 10 && teleport3 != 0 && teleport3 != 2)
        {
            Teleport p = new Teleport(Char.myCharz().cx, Char.myCharz().cy, Char.myCharz().head, Char.myCharz().cdir, 1, isMe: true, (teleport3 != 1) ? teleport3 : Char.myCharz().cgender);
            Char.myCharz().isTeleport = true;
            Teleport.addTeleport(p);
        }
        if (teleport3 == 2)
        {
            Char.myCharz().show();
        }
        if (GameScr.gI().isRongThanXuatHien && TileMap.mapID == GameScr.gI().mapRID && TileMap.zoneID == GameScr.gI().zoneRID)
        {
            GameScr.gI().callRongThan(GameScr.gI().xR, GameScr.gI().yR);
        }
        InfoDlg.hide();
        InfoDlg.show(TileMap.mapName, mResources.zone + " " + TileMap.zoneID, 30);
        GameCanvas.endDlg();
        GameCanvas.isLoading = false;
        Hint.clickMob();
        Hint.clickNpc();
        GameCanvas.debug("SA75x9", 2);
        GameCanvas.isRequestMapID = 2;
        GameCanvas.waitingTimeChangeMap = mSystem.currentTimeMillis() + 1000;
        Res.outz("[CONTROLLER] loadMap DONE!!!!!!!!!");
    }

    public void loadInfoMap(Message msg)
    {
        try
        {
            Char.myCharz().cx = (Char.myCharz().cxSend = (Char.myCharz().cxFocus = msg.reader().readShort()));
            Char.myCharz().cy = (Char.myCharz().cySend = (Char.myCharz().cyFocus = msg.reader().readShort()));
            Char.myCharz().xSd = Char.myCharz().cx;
            Char.myCharz().ySd = Char.myCharz().cy;
            Res.outz("head= " + Char.myCharz().head + " body= " + Char.myCharz().body + " left= " + Char.myCharz().leg + " x= " + Char.myCharz().cx + " y= " + Char.myCharz().cy + " chung toc= " + Char.myCharz().cgender);
            if (Char.myCharz().cx >= 0 && Char.myCharz().cx <= 100)
            {
                Char.myCharz().cdir = 1;
            }
            else if (Char.myCharz().cx >= TileMap.tmw - 100 && Char.myCharz().cx <= TileMap.tmw)
            {
                Char.myCharz().cdir = -1;
            }
            GameCanvas.debug("SA75x4", 2);
            int num = msg.reader().readByte();
            Res.outz("vGo size= " + num);
            if (!GameScr.info1.isDone)
            {
                GameScr.info1.cmx = Char.myCharz().cx - GameScr.cmx;
                GameScr.info1.cmy = Char.myCharz().cy - GameScr.cmy;
            }
            for (int i = 0; i < num; i++)
            {
                Waypoint waypoint = new Waypoint(msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readUTF());
                if ((TileMap.mapID != 21 && TileMap.mapID != 22 && TileMap.mapID != 23) || waypoint.minX < 0 || waypoint.minX <= 24)
                {
                }
            }
            GC.Collect();
            GameCanvas.debug("SA75x5", 2);
            num = msg.reader().readByte();
            Mob.newMob.removeAllElements();
            for (sbyte b = 0; b < num; b = (sbyte)(b + 1))
            {
                Mob mob = new Mob(b, msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readBoolean(), msg.reader().readShort(), msg.reader().readByte(), msg.reader().readLong(), msg.reader().readByte(), msg.reader().readLong(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readByte(), msg.reader().readByte());
                mob.xSd = mob.x;
                mob.ySd = mob.y;
                mob.isBoss = msg.reader().readBoolean();
                if (Mob.arrMobTemplate[mob.templateId].type != 0)
                {
                    if (b % 3 == 0)
                    {
                        mob.dir = -1;
                    }
                    else
                    {
                        mob.dir = 1;
                    }
                    mob.x += 10 - b % 20;
                }
                mob.isMobMe = false;
                BigBoss bigBoss = null;
                BachTuoc bachTuoc = null;
                BigBoss2 bigBoss2 = null;
                NewBoss newBoss = null;
                if (mob.templateId == 70)
                {
                    bigBoss = new BigBoss(b, (short)mob.x, (short)mob.y, 70, mob.hp, mob.maxHp, mob.sys);
                }
                if (mob.templateId == 71)
                {
                    bachTuoc = new BachTuoc(b, (short)mob.x, (short)mob.y, 71, mob.hp, mob.maxHp, mob.sys);
                }
                if (mob.templateId == 72)
                {
                    bigBoss2 = new BigBoss2(b, (short)mob.x, (short)mob.y, 72, mob.hp, mob.maxHp, 3);
                }
                if (mob.isBoss)
                {
                    newBoss = new NewBoss(b, (short)mob.x, (short)mob.y, mob.templateId, mob.hp, mob.maxHp, mob.sys);
                }
                if (newBoss != null)
                {
                    GameScr.vMob.addElement(newBoss);
                }
                else if (bigBoss != null)
                {
                    GameScr.vMob.addElement(bigBoss);
                }
                else if (bachTuoc != null)
                {
                    GameScr.vMob.addElement(bachTuoc);
                }
                else if (bigBoss2 != null)
                {
                    GameScr.vMob.addElement(bigBoss2);
                }
                else
                {
                    GameScr.vMob.addElement(mob);
                }
            }
            if (Char.myCharz().mobMe != null && GameScr.findMobInMap(Char.myCharz().mobMe.mobId) == null)
            {
                Char.myCharz().mobMe.getData();
                Char.myCharz().mobMe.x = Char.myCharz().cx;
                Char.myCharz().mobMe.y = Char.myCharz().cy - 40;
                GameScr.vMob.addElement(Char.myCharz().mobMe);
            }
            num = msg.reader().readByte();
            for (byte b2 = 0; b2 < num; b2 = (byte)(b2 + 1))
            {
            }
            GameCanvas.debug("SA75x6", 2);
            num = msg.reader().readByte();
            Res.outz("NPC size= " + num);
            for (int j = 0; j < num; j++)
            {
                sbyte b3 = msg.reader().readByte();
                short cx = msg.reader().readShort();
                short num11 = msg.reader().readShort();
                sbyte b4 = msg.reader().readByte();
                short num12 = msg.reader().readShort();
                if (b4 != 6 && ((Char.myCharz().taskMaint.taskId >= 7 && (Char.myCharz().taskMaint.taskId != 7 || Char.myCharz().taskMaint.index > 1)) || (b4 != 7 && b4 != 8 && b4 != 9)) && (Char.myCharz().taskMaint.taskId >= 6 || b4 != 16))
                {
                    if (b4 == 4)
                    {
                        GameScr.gI().magicTree = new MagicTree(j, b3, cx, num11, b4, num12);
                        Service.gI().magicTree(2);
                        GameScr.vNpc.addElement(GameScr.gI().magicTree);
                    }
                    else
                    {
                        Npc o = new Npc(j, b3, cx, num11 + 3, b4, num12);
                        GameScr.vNpc.addElement(o);
                    }
                }
            }
            GameCanvas.debug("SA75x7", 2);
            num = msg.reader().readByte();
            string empty = string.Empty;
            Res.outz("item size = " + num);
            empty = empty + "item: " + num;
            for (int k = 0; k < num; k++)
            {
                short itemMapID = msg.reader().readShort();
                short num13 = msg.reader().readShort();
                int x = msg.reader().readShort();
                int y = msg.reader().readShort();
                int num14 = msg.reader().readInt();
                short r = 0;
                if (num14 == -2)
                {
                    r = msg.reader().readShort();
                }
                ItemMap itemMap = new ItemMap(num14, itemMapID, num13, x, y, r);
                bool flag = false;
                for (int l = 0; l < GameScr.vItemMap.size(); l++)
                {
                    ItemMap itemMap2 = (ItemMap)GameScr.vItemMap.elementAt(l);
                    if (itemMap2.itemMapID == itemMap.itemMapID)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    GameScr.vItemMap.addElement(itemMap);
                }
                empty = empty + num13 + ",";
            }
            Res.err("sl item on map " + empty + "\n");
            TileMap.vCurrItem.removeAllElements();
            BgItem.clearHashTable();
            BgItem.vKeysNew.removeAllElements();
            if (!GameCanvas.lowGraphic || (GameCanvas.lowGraphic && TileMap.isVoDaiMap()) || TileMap.mapID == 45 || TileMap.mapID == 46 || TileMap.mapID == 47 || TileMap.mapID == 48 || TileMap.mapID == 120 || TileMap.mapID == 128 || TileMap.mapID == 170 || TileMap.mapID == 49)
            {
                short num15 = msg.reader().readShort();
                empty = "item high graphic: ";
                for (int m = 0; m < num15; m++)
                {
                    short num16 = msg.reader().readShort();
                    short num17 = msg.reader().readShort();
                    short num18 = msg.reader().readShort();
                    if (TileMap.getBIById(num16) != null)
                    {
                        BgItem bIById = TileMap.getBIById(num16);
                        BgItem bgItem = new BgItem();
                        bgItem.id = num16;
                        bgItem.idImage = bIById.idImage;
                        bgItem.dx = bIById.dx;
                        bgItem.dy = bIById.dy;
                        bgItem.x = num17 * TileMap.size;
                        bgItem.y = num18 * TileMap.size;
                        bgItem.layer = bIById.layer;
                        if (TileMap.isExistMoreOne(bgItem.id))
                        {
                            bgItem.trans = ((m % 2 != 0) ? 2 : 0);
                            if (TileMap.mapID == 45)
                            {
                                bgItem.trans = 0;
                            }
                        }
                        Image image = null;
                        if (!BgItem.imgNew.containsKey(bgItem.idImage + string.Empty))
                        {
                            image = GameCanvas.loadImage("/mapBackGround/" + bgItem.idImage + ".png");
                            if (image == null)
                            {
                                image = Image.createRGBImage(new int[1], 1, 1, bl: true);
                                Service.gI().getBgTemplate(bgItem.idImage);
                            }
                            BgItem.imgNew.put(bgItem.idImage + string.Empty, image);
                            BgItem.vKeysLast.addElement(bgItem.idImage + string.Empty);
                        }
                        if (!BgItem.isExistKeyNews(bgItem.idImage + string.Empty))
                        {
                            BgItem.vKeysNew.addElement(bgItem.idImage + string.Empty);
                        }
                        bgItem.changeColor();
                        TileMap.vCurrItem.addElement(bgItem);
                    }
                    empty = empty + num16 + ",";
                }
                Res.err("item High Graphics: " + empty);
                for (int n = 0; n < BgItem.vKeysLast.size(); n++)
                {
                    string text = (string)BgItem.vKeysLast.elementAt(n);
                    if (!BgItem.isExistKeyNews(text))
                    {
                        BgItem.imgNew.remove(text);
                        if (BgItem.imgNew.containsKey(text + "blend" + 1))
                        {
                            BgItem.imgNew.remove(text + "blend" + 1);
                        }
                        if (BgItem.imgNew.containsKey(text + "blend" + 3))
                        {
                            BgItem.imgNew.remove(text + "blend" + 3);
                        }
                        BgItem.vKeysLast.removeElementAt(n);
                        n--;
                    }
                }
                BackgroudEffect.isFog = false;
                BackgroudEffect.nCloud = 0;
                EffecMn.vEff.removeAllElements();
                BackgroudEffect.vBgEffect.removeAllElements();
                Effect.newEff.removeAllElements();
                short num2 = msg.reader().readShort();
                for (int num3 = 0; num3 < num2; num3++)
                {
                    string key = msg.reader().readUTF();
                    string value = msg.reader().readUTF();
                    keyValueAction(key, value);
                }
            }
            else
            {
                short num4 = msg.reader().readShort();
                for (int num5 = 0; num5 < num4; num5++)
                {
                    short num6 = msg.reader().readShort();
                    short num7 = msg.reader().readShort();
                    short num8 = msg.reader().readShort();
                }
                short num9 = msg.reader().readShort();
                for (int num10 = 0; num10 < num9; num10++)
                {
                    string text2 = msg.reader().readUTF();
                    string text3 = msg.reader().readUTF();
                }
            }
            TileMap.bgType = msg.reader().readByte();
            sbyte teleport = msg.reader().readByte();
            loadCurrMap(teleport);
            GameCanvas.debug("SA75x8", 2);
        }
        catch (Exception)
        {
            Res.err(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Loadmap khong thanh cong");
            GameCanvas.instance.doResetToLoginScr(GameCanvas.serverScreen);
            ServerListScreen.waitToLogin = true;
            GameCanvas.endDlg();
        }
        GameCanvas.isLoading = false;
        Res.err(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Loadmap thanh cong");
    }

    public void keyValueAction(string key, string value)
    {
        if (key.Equals("eff"))
        {
            if (PanelG.graphics > 0)
            {
                return;
            }
            string[] array = Res.split(value, ".", 0);
            int id = int.Parse(array[0]);
            int layer = int.Parse(array[1]);
            int x = int.Parse(array[2]);
            int y = int.Parse(array[3]);
            int loop;
            int loopCount;
            if (array.Length <= 4)
            {
                loop = -1;
                loopCount = 1;
            }
            else
            {
                loop = int.Parse(array[4]);
                loopCount = int.Parse(array[5]);
            }
            Effect effect = new Effect(id, x, y, layer, loop, loopCount);
            if (array.Length > 6)
            {
                effect.typeEff = int.Parse(array[6]);
                if (array.Length > 7)
                {
                    effect.indexFrom = int.Parse(array[7]);
                    effect.indexTo = int.Parse(array[8]);
                }
            }
            EffecMn.addEff(effect);
        }
        else if (key.Equals("beff") && PanelG.graphics <= 1)
        {
            BackgroudEffect.addEffect(int.Parse(value));
        }
    }

    public void messageNotMap(Message msg)
    {
        GameCanvas.debug("SA6", 2);
        try
        {
            sbyte b = msg.reader().readByte();
            Res.outz("---messageNotMap : " + b);
            switch (b)
            {
                case 16:
                    MoneyCharge.gI().switchToMe();
                    break;
                case 17:
                    GameCanvas.debug("SYB123", 2);
                    Char.myCharz().clearTask();
                    break;
                case 18:
                    {
                        GameCanvas.isLoading = false;
                        GameCanvas.endDlg();
                        int num2 = msg.reader().readInt();
                        GameCanvas.inputDlg.show(mResources.changeNameChar, new Command(mResources.OK, GameCanvas.instance, 88829, num2), TField.INPUT_TYPE_ANY);
                        break;
                    }
                case 20:
                    Char.myCharz().cPk = msg.reader().readByte();
                    GameScr.info1.addInfo(mResources.PK_NOW + " " + Char.myCharz().cPk, 0);
                    break;
                case 35:
                    GameCanvas.endDlg();
                    GameScr.gI().resetButton();
                    GameScr.info1.addInfo(msg.reader().readUTF(), 0);
                    break;
                case 36:
                    GameScr.typeActive = msg.reader().readByte();
                    Res.outz("load Me Active: " + GameScr.typeActive);
                    break;
                case 4:
                    {
                        GameCanvas.debug("SA8", 2);
                        GameCanvas.loginScr.savePass();
                        GameScr.isAutoPlay = false;
                        GameScr.canAutoPlay = false;
                        LoginScr.isUpdateAll = true;
                        LoginScr.isUpdateData = true;
                        LoginScr.isUpdateMap = true;
                        LoginScr.isUpdateSkill = true;
                        LoginScr.isUpdateItem = true;
                        GameScr.vsData = msg.reader().readByte();
                        GameScr.vsMap = msg.reader().readByte();
                        GameScr.vsSkill = msg.reader().readByte();
                        GameScr.vsItem = msg.reader().readByte();
                        sbyte b2 = msg.reader().readByte();
                        if (GameCanvas.loginScr.isLogin2)
                        {
                            Rms.saveRMSString("acc", string.Empty);
                            Rms.saveRMSString("pass", string.Empty);
                        }
                        else
                        {
                            Rms.saveRMSString("userAo" + ServerListScreen.ipSelect, string.Empty);
                        }
                        if (GameScr.vsData != GameScr.vcData)
                        {
                            GameScr.isLoadAllData = false;
                            Service.gI().updateData();
                        }
                        else
                        {
                            try
                            {
                                LoginScr.isUpdateData = false;
                            }
                            catch (Exception)
                            {
                                GameScr.vcData = -1;
                                Service.gI().updateData();
                            }
                        }
                        if (GameScr.vsMap != GameScr.vcMap)
                        {
                            GameScr.isLoadAllData = false;
                            Service.gI().updateMap();
                        }
                        else
                        {
                            try
                            {
                                if (!GameScr.isLoadAllData)
                                {
                                    DataInputStream dataInputStream = new DataInputStream(Rms.loadRMS("NRmap"));
                                    createMap(dataInputStream.r);
                                }
                                LoginScr.isUpdateMap = false;
                            }
                            catch (Exception)
                            {
                                GameScr.vcMap = -1;
                                Service.gI().updateMap();
                            }
                        }
                        if (GameScr.vsSkill != GameScr.vcSkill)
                        {
                            GameScr.isLoadAllData = false;
                            Service.gI().updateSkill();
                        }
                        else
                        {
                            try
                            {
                                if (!GameScr.isLoadAllData)
                                {
                                    DataInputStream dataInputStream2 = new DataInputStream(Rms.loadRMS("NRskill"));
                                    createSkill(dataInputStream2.r);
                                }
                                LoginScr.isUpdateSkill = false;
                            }
                            catch (Exception)
                            {
                                GameScr.vcSkill = -1;
                                Service.gI().updateSkill();
                            }
                        }
                        if (GameScr.vsItem != GameScr.vcItem)
                        {
                            GameScr.isLoadAllData = false;
                            Service.gI().updateItem();
                        }
                        else
                        {
                            try
                            {
                                DataInputStream dataInputStream3 = new DataInputStream(Rms.loadRMS("NRitem0"));
                                loadItemNew(dataInputStream3.r, 0, isSave: false);
                                DataInputStream dataInputStream4 = new DataInputStream(Rms.loadRMS("NRitem1"));
                                loadItemNew(dataInputStream4.r, 1, isSave: false);
                                DataInputStream dataInputStream5 = new DataInputStream(Rms.loadRMS("NRitem100"));
                                loadItemNew(dataInputStream5.r, 100, isSave: false);
                                LoginScr.isUpdateItem = false;
                            }
                            catch (Exception)
                            {
                                GameScr.vcItem = -1;
                                Service.gI().updateItem();
                            }
                            try
                            {
                                DataInputStream dataInputStream6 = new DataInputStream(Rms.loadRMS("NRitem101"));
                                loadItemNew(dataInputStream6.r, 101, isSave: false);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (!GameScr.isLoadAllData)
                        {
                            GameScr.gI().readOk();
                        }
                        else
                        {
                            Service.gI().clientOk();
                        }
                        sbyte b3 = msg.reader().readByte();
                        Res.outz("CAPTION LENT= " + b3);
                        GameScr.exps = new long[b3];
                        for (int j = 0; j < GameScr.exps.Length; j++)
                        {
                            GameScr.exps[j] = msg.reader().readLong();
                        }
                        break;
                    }
                case 6:
                    {
                        Res.outz("GET UPDATE_MAP " + msg.reader().available() + " bytes");
                        msg.reader().mark(500000);
                        createMap(msg.reader());
                        msg.reader().reset();
                        sbyte[] data3 = new sbyte[msg.reader().available()];
                        msg.reader().readFully(ref data3);
                        Rms.saveRMS("NRmap", data3);
                        sbyte[] data4 = new sbyte[1] { GameScr.vcMap };
                        Rms.saveRMS("NRmapVersion", data4);
                        LoginScr.isUpdateMap = false;
                        GameScr.gI().readOk();
                        break;
                    }
                case 7:
                    {
                        Res.outz("GET UPDATE_SKILL " + msg.reader().available() + " bytes");
                        msg.reader().mark(500000);
                        createSkill(msg.reader());
                        msg.reader().reset();
                        sbyte[] data = new sbyte[msg.reader().available()];
                        msg.reader().readFully(ref data);
                        Rms.saveRMS("NRskill", data);
                        sbyte[] data2 = new sbyte[1] { GameScr.vcSkill };
                        Rms.saveRMS("NRskillVersion", data2);
                        LoginScr.isUpdateSkill = false;
                        GameScr.gI().readOk();
                        break;
                    }
                case 8:
                    Res.outz("GET UPDATE_ITEM " + msg.reader().available() + " bytes");
                    createItemNew(msg.reader());
                    break;
                case 10:
                    try
                    {
                        Char.isLoadingMap = true;
                        Res.outz("REQUEST MAP TEMPLATE");
                        GameCanvas.isLoading = true;
                        TileMap.maps = null;
                        TileMap.types = null;
                        mSystem.gcc();
                        GameCanvas.debug("SA99", 2);
                        TileMap.tmw = msg.reader().readByte();
                        TileMap.tmh = msg.reader().readByte();
                        TileMap.maps = new int[TileMap.tmw * TileMap.tmh];
                        Res.err("   M apsize= " + TileMap.tmw * TileMap.tmh);
                        for (int i = 0; i < TileMap.maps.Length; i++)
                        {
                            int num = msg.reader().readByte();
                            if (num < 0)
                            {
                                num += 256;
                            }
                            TileMap.maps[i] = (ushort)num;
                        }
                        TileMap.types = new int[TileMap.maps.Length];
                        msg = messWait;
                        loadInfoMap(msg);
                        try
                        {
                            TileMap.isMapDouble = ((msg.reader().readByte() != 0) ? true : false);
                        }
                        catch (Exception ex)
                        {
                            Res.err(" 1 LOI TAI CASE REQUEST_MAPTEMPLATE " + ex.ToString());
                        }
                    }
                    catch (Exception ex2)
                    {
                        Res.err("2 LOI TAI CASE REQUEST_MAPTEMPLATE " + ex2.ToString());
                    }
                    msg.cleanup();
                    messWait.cleanup();
                    msg = (messWait = null);
                    GameScr.gI().switchToMe();
                    break;
                case 9:
                    GameCanvas.debug("SA11", 2);
                    break;
            }
        }
        catch (Exception ex3)
        {
            Cout.LogError("LOI TAI messageNotMap=== " + msg.command + "  >>" + ex3.StackTrace);
        }
        finally
        {
            msg?.cleanup();
        }
    }

    public void messageNotLogin(Message msg)
    {
        try
        {
            sbyte b = msg.reader().readByte();
            Res.outz("---messageNotLogin : " + b);
            if (b == 2)
            {
                string linkDefault = msg.reader().readUTF();
                Res.outz(">>Get CLIENT_INFO");
                ServerListScreen.linkDefault = linkDefault;
                mSystem.AddIpTest();
                ServerListScreen.getServerList(ServerListScreen.linkDefault);
                try
                {
                    sbyte b2 = msg.reader().readByte();
                    PanelG.CanNapTien = b2 == 1;
                }
                catch (Exception)
                {
                }
                isGet_CLIENT_INFO = true;
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            msg?.cleanup();
        }
    }

    public void messageSubCommand(Message msg)
    {
        try
        {
            GameCanvas.debug("SA12", 2);
            sbyte b = msg.reader().readByte();
            Res.outz("---messageSubCommand : " + b);
            switch (b)
            {
                case 63:
                    {
                        sbyte b6 = msg.reader().readByte();
                        if (b6 > 0)
                        {
                            GameCanvas.panel.vPlayerMenu_id.removeAllElements();
                            InfoDlg.showWait();
                            MyVector vPlayerMenu = GameCanvas.panel.vPlayerMenu;
                            for (int j = 0; j < b6; j++)
                            {
                                string caption = msg.reader().readUTF();
                                string caption2 = msg.reader().readUTF();
                                short num12 = msg.reader().readShort();
                                GameCanvas.panel.vPlayerMenu_id.addElement(num12 + string.Empty);
                                Char.myCharz().charFocus.menuSelect = num12;
                                Command command = new Command(caption, 11115, Char.myCharz().charFocus);
                                command.caption2 = caption2;
                                vPlayerMenu.addElement(command);
                            }
                            InfoDlg.hide();
                            GameCanvas.panel.setTabPlayerMenu();
                        }
                        break;
                    }
                case 1:
                    GameCanvas.debug("SA13", 2);
                    Char.myCharz().nClass = GameScr.nClasss[msg.reader().readByte()];
                    Char.myCharz().cTiemNang = msg.reader().readLong();
                    Char.myCharz().vSkill.removeAllElements();
                    Char.myCharz().vSkillFight.removeAllElements();
                    Char.myCharz().myskill = null;
                    break;
                case 2:
                    {
                        GameCanvas.debug("SA14", 2);
                        if (Char.myCharz().statusMe != 14 && Char.myCharz().statusMe != 5)
                        {
                            Char.myCharz().cHP = Char.myCharz().cHPFull;
                            Char.myCharz().cMP = Char.myCharz().cMPFull;
                            Cout.LogError2(" ME_LOAD_SKILL");
                        }
                        Char.myCharz().vSkill.removeAllElements();
                        Char.myCharz().vSkillFight.removeAllElements();
                        sbyte b2 = msg.reader().readByte();
                        for (sbyte b4 = 0; b4 < b2; b4 = (sbyte)(b4 + 1))
                        {
                            short skillId = msg.reader().readShort();
                            Skill skill2 = Skills.get(skillId);
                            useSkill(skill2);
                        }
                        GameScr.gI().sortSkill();
                        if (GameScr.isPaintInfoMe)
                        {
                            GameScr.indexRow = -1;
                            GameScr.gI().left = (GameScr.gI().center = null);
                        }
                        break;
                    }
                case 19:
                    GameCanvas.debug("SA17", 2);
                    Char.myCharz().boxSort();
                    break;
                case 21:
                    {
                        GameCanvas.debug("SA19", 2);
                        int num10 = msg.reader().readInt();
                        Char.myCharz().xuInBox -= num10;
                        Char.myCharz().xu += num10;
                        Char.myCharz().xuStr = mSystem.numberTostring(Char.myCharz().xu);
                        break;
                    }
                case 0:
                    {
                        GameCanvas.debug("SA21", 2);
                        RadarScr.list = new MyVector();
                        Teleport.vTeleport.removeAllElements();
                        GameScr.vCharInMap.removeAllElements();
                        GameScr.vItemMap.removeAllElements();
                        Char.vItemTime.removeAllElements();
                        GameScr.loadImg();
                        GameScr.currentCharViewInfo = Char.myCharz();
                        Char.myCharz().charID = msg.reader().readInt();
                        Char.myCharz().ctaskId = msg.reader().readByte();
                        Char.myCharz().cgender = msg.reader().readByte();
                        Char.myCharz().head = msg.reader().readShort();
                        Char.myCharz().cName = msg.reader().readUTF();
                        Char.myCharz().cPk = msg.reader().readByte();
                        Char.myCharz().cTypePk = msg.reader().readByte();
                        Char.myCharz().cPower = msg.reader().readLong();
                        Char.myCharz().applyCharLevelPercent();
                        Char.myCharz().eff5BuffHp = msg.reader().readShort();
                        Char.myCharz().eff5BuffMp = msg.reader().readShort();
                        Char.myCharz().nClass = GameScr.nClasss[msg.reader().readByte()];
                        Char.myCharz().vSkill.removeAllElements();
                        Char.myCharz().vSkillFight.removeAllElements();
                        GameScr.gI().dHP = Char.myCharz().cHP;
                        GameScr.gI().dMP = Char.myCharz().cMP;
                        sbyte b3 = msg.reader().readByte();
                        for (sbyte b7 = 0; b7 < b3; b7 = (sbyte)(b7 + 1))
                        {
                            Skill skill3 = Skills.get(msg.reader().readShort());
                            useSkill(skill3);
                        }
                        GameScr.gI().sortSkill();
                        GameScr.gI().loadSkillShortcut();
                        Char.myCharz().xu = msg.reader().readLong();
                        Char.myCharz().luongKhoa = msg.reader().readInt();
                        Char.myCharz().luong = msg.reader().readInt();
                        Char.myCharz().xuStr = Res.formatNumber(Char.myCharz().xu);
                        Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
                        Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
                        Char.myCharz().arrItemBody = new Item[msg.reader().readByte()];
                        try
                        {
                            Char.myCharz().setDefaultPart();
                            for (int k = 0; k < Char.myCharz().arrItemBody.Length; k++)
                            {
                                short num13 = msg.reader().readShort();
                                if (num13 == -1)
                                {
                                    continue;
                                }
                                ItemTemplate itemTemplate = ItemTemplates.get(num13);
                                int num14 = itemTemplate.type;
                                Char.myCharz().arrItemBody[k] = new Item();
                                Char.myCharz().arrItemBody[k].template = itemTemplate;
                                Char.myCharz().arrItemBody[k].quantity = msg.reader().readInt();
                                Char.myCharz().arrItemBody[k].info = msg.reader().readUTF();
                                Char.myCharz().arrItemBody[k].content = msg.reader().readUTF();
                                int num15 = msg.reader().readUnsignedByte();
                                if (num15 != 0)
                                {
                                    Char.myCharz().arrItemBody[k].itemOption = new ItemOption[num15];
                                    for (int l = 0; l < Char.myCharz().arrItemBody[k].itemOption.Length; l++)
                                    {
                                        ItemOption itemOption = readItemOption(msg);
                                        if (itemOption != null)
                                        {
                                            Char.myCharz().arrItemBody[k].itemOption[l] = itemOption;
                                        }
                                    }
                                }
                                switch (num14)
                                {
                                    case 0:
                                        Res.outz("toi day =======================================" + Char.myCharz().body);
                                        Char.myCharz().body = Char.myCharz().arrItemBody[k].template.part;
                                        break;
                                    case 1:
                                        Char.myCharz().leg = Char.myCharz().arrItemBody[k].template.part;
                                        Res.outz("toi day =======================================" + Char.myCharz().leg);
                                        break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        Char.myCharz().arrItemBag = new Item[msg.reader().readByte()];
                        GameScr.hpPotion = 0;
                        GameScr.isudungCapsun4 = false;
                        GameScr.isudungCapsun3 = false;
                        for (int m = 0; m < Char.myCharz().arrItemBag.Length; m++)
                        {
                            short num16 = msg.reader().readShort();
                            if (num16 == -1)
                            {
                                continue;
                            }
                            Char.myCharz().arrItemBag[m] = new Item();
                            Char.myCharz().arrItemBag[m].template = ItemTemplates.get(num16);
                            Char.myCharz().arrItemBag[m].quantity = msg.reader().readInt();
                            Char.myCharz().arrItemBag[m].info = msg.reader().readUTF();
                            Char.myCharz().arrItemBag[m].content = msg.reader().readUTF();
                            Char.myCharz().arrItemBag[m].indexUI = m;
                            sbyte b8 = msg.reader().readByte();
                            if (b8 != 0)
                            {
                                Char.myCharz().arrItemBag[m].itemOption = new ItemOption[b8];
                                for (int n = 0; n < Char.myCharz().arrItemBag[m].itemOption.Length; n++)
                                {
                                    ItemOption itemOption2 = readItemOption(msg);
                                    if (itemOption2 != null)
                                    {
                                        Char.myCharz().arrItemBag[m].itemOption[n] = itemOption2;
                                        Char.myCharz().arrItemBag[m].getCompare();
                                    }
                                }
                            }
                            if (Char.myCharz().arrItemBag[m].template.type == 6)
                            {
                                GameScr.hpPotion += Char.myCharz().arrItemBag[m].quantity;
                            }
                            switch (num16)
                            {
                                case 194:
                                    GameScr.isudungCapsun4 = Char.myCharz().arrItemBag[m].quantity > 0;
                                    break;
                                case 193:
                                    if (!GameScr.isudungCapsun4)
                                    {
                                        GameScr.isudungCapsun3 = Char.myCharz().arrItemBag[m].quantity > 0;
                                    }
                                    break;
                            }
                        }
                        Char.myCharz().arrItemBox = new Item[msg.reader().readByte()];
                        GameCanvas.panel.hasUse = 0;
                        for (int num2 = 0; num2 < Char.myCharz().arrItemBox.Length; num2++)
                        {
                            short num3 = msg.reader().readShort();
                            if (num3 == -1)
                            {
                                continue;
                            }
                            Char.myCharz().arrItemBox[num2] = new Item();
                            Char.myCharz().arrItemBox[num2].template = ItemTemplates.get(num3);
                            Char.myCharz().arrItemBox[num2].quantity = msg.reader().readInt();
                            Char.myCharz().arrItemBox[num2].info = msg.reader().readUTF();
                            Char.myCharz().arrItemBox[num2].content = msg.reader().readUTF();
                            Char.myCharz().arrItemBox[num2].itemOption = new ItemOption[msg.reader().readByte()];
                            for (int num4 = 0; num4 < Char.myCharz().arrItemBox[num2].itemOption.Length; num4++)
                            {
                                ItemOption itemOption3 = readItemOption(msg);
                                if (itemOption3 != null)
                                {
                                    Char.myCharz().arrItemBox[num2].itemOption[num4] = itemOption3;
                                    Char.myCharz().arrItemBox[num2].getCompare();
                                }
                            }
                            GameCanvas.panel.hasUse++;
                        }
                        Char.myCharz().statusMe = 4;
                        int num5 = Rms.loadRMSInt(Char.myCharz().cName + "vci");
                        if (num5 < 1)
                        {
                            GameScr.isViewClanInvite = false;
                        }
                        else
                        {
                            GameScr.isViewClanInvite = true;
                        }
                        short num6 = msg.reader().readShort();
                        Char.idHead = new short[num6];
                        Char.idAvatar = new short[num6];
                        for (int num7 = 0; num7 < num6; num7++)
                        {
                            Char.idHead[num7] = msg.reader().readShort();
                            Char.idAvatar[num7] = msg.reader().readShort();
                        }
                        for (int num8 = 0; num8 < GameScr.info1.charId.Length; num8++)
                        {
                            GameScr.info1.charId[num8] = new int[3];
                        }
                        GameScr.info1.charId[Char.myCharz().cgender][0] = msg.reader().readShort();
                        GameScr.info1.charId[Char.myCharz().cgender][1] = msg.reader().readShort();
                        GameScr.info1.charId[Char.myCharz().cgender][2] = msg.reader().readShort();
                        Char.myCharz().isNhapThe = msg.reader().readByte() == 1;
                        Res.outz("NHAP THE= " + Char.myCharz().isNhapThe);
                        GameScr.deltaTime = mSystem.currentTimeMillis() - (long)msg.reader().readInt() * 1000L;
                        GameScr.isNewMember = msg.reader().readByte();
                        Service.gI().updateCaption((sbyte)Char.myCharz().cgender);
                        Service.gI().androidPack();
                        try
                        {
                            Char.myCharz().idAuraEff = msg.reader().readShort();
                            Char.myCharz().idEff_Set_Item = msg.reader().readSByte();
                            Char.myCharz().idHat = msg.reader().readShort();
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                case 4:
                    GameCanvas.debug("SA23", 2);
                    Char.myCharz().xu = msg.reader().readLong();
                    Char.myCharz().luong = msg.reader().readInt();
                    Char.myCharz().cHP = msg.reader().readLong();
                    Char.myCharz().cMP = msg.reader().readLong();
                    Char.myCharz().luongKhoa = msg.reader().readInt();
                    Char.myCharz().xuStr = Res.formatNumber2(Char.myCharz().xu);
                    Char.myCharz().luongStr = mSystem.numberTostring(Char.myCharz().luong);
                    Char.myCharz().luongKhoaStr = mSystem.numberTostring(Char.myCharz().luongKhoa);
                    break;
                case 5:
                    {
                        GameCanvas.debug("SA24", 2);
                        long cHP = Char.myCharz().cHP;
                        Char.myCharz().cHP = msg.reader().readLong();
                        if (Char.myCharz().cHP > cHP && Char.myCharz().cTypePk != 4)
                        {
                            GameScr.startFlyText("+" + (Char.myCharz().cHP - cHP) + " " + mResources.HP, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 20, 0, -1, mFont.HP);
                            SoundMn.gI().HP_MPup();
                            if (Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 5003)
                            {
                                MonsterDart.addMonsterDart(Char.myCharz().petFollow.cmx + ((Char.myCharz().petFollow.dir != 1) ? (-10) : 10), Char.myCharz().petFollow.cmy + 10, isBoss: true, -1L, -1L, Char.myCharz(), 29);
                            }
                        }
                        if (Char.myCharz().cHP < cHP)
                        {
                            GameScr.startFlyText("-" + (cHP - Char.myCharz().cHP) + " " + mResources.HP, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 20, 0, -1, mFont.HP);
                        }
                        GameScr.gI().dHP = Char.myCharz().cHP;
                        if (!GameScr.isPaintInfoMe)
                        {
                        }
                        break;
                    }
                case 6:
                    {
                        GameCanvas.debug("SA25", 2);
                        if (Char.myCharz().statusMe == 14 || Char.myCharz().statusMe == 5)
                        {
                            break;
                        }
                        long cMP = Char.myCharz().cMP;
                        Char.myCharz().cMP = msg.reader().readLong();
                        if (Char.myCharz().cMP > cMP)
                        {
                            GameScr.startFlyText("+" + (Char.myCharz().cMP - cMP) + " " + mResources.KI, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 23, 0, -2, mFont.MP);
                            SoundMn.gI().HP_MPup();
                            if (Char.myCharz().petFollow != null && Char.myCharz().petFollow.smallID == 5001)
                            {
                                MonsterDart.addMonsterDart(Char.myCharz().petFollow.cmx + ((Char.myCharz().petFollow.dir != 1) ? (-10) : 10), Char.myCharz().petFollow.cmy + 10, isBoss: true, -1L, -1L, Char.myCharz(), 29);
                            }
                        }
                        if (Char.myCharz().cMP < cMP)
                        {
                            GameScr.startFlyText("-" + (cMP - Char.myCharz().cMP) + " " + mResources.KI, Char.myCharz().cx, Char.myCharz().cy - Char.myCharz().ch - 23, 0, -2, mFont.MP);
                        }
                        Res.outz("curr MP= " + Char.myCharz().cMP);
                        GameScr.gI().dMP = Char.myCharz().cMP;
                        if (!GameScr.isPaintInfoMe)
                        {
                        }
                        break;
                    }
                case 7:
                    {
                        Char char10 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char10 == null)
                        {
                            break;
                        }
                        char10.clanID = msg.reader().readInt();
                        if (char10.clanID == -2)
                        {
                            char10.isCopy = true;
                        }
                        readCharInfo(char10, msg);
                        try
                        {
                            char10.idAuraEff = msg.reader().readShort();
                            char10.idEff_Set_Item = msg.reader().readSByte();
                            char10.idHat = msg.reader().readShort();
                            if (char10.bag >= 201)
                            {
                                Effect effect = new Effect(char10.bag, char10, 2, -1, 10, 1);
                                effect.typeEff = 5;
                                char10.addEffChar(effect);
                            }
                            else
                            {
                                char10.removeEffChar(0, 201);
                            }
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                case 8:
                    {
                        GameCanvas.debug("SA26", 2);
                        Char char9 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char9 != null)
                        {
                            char9.cspeed = msg.reader().readByte();
                        }
                        break;
                    }
                case 9:
                    {
                        GameCanvas.debug("SA27", 2);
                        Char char8 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char8 != null)
                        {
                            char8.cHP = msg.reader().readLong();
                            char8.cHPFull = msg.reader().readLong();
                        }
                        break;
                    }
                case 10:
                    {
                        GameCanvas.debug("SA28", 2);
                        Char char7 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char7 != null)
                        {
                            char7.cHP = msg.reader().readLong();
                            char7.cHPFull = msg.reader().readLong();
                            char7.eff5BuffHp = msg.reader().readShort();
                            char7.eff5BuffMp = msg.reader().readShort();
                            char7.wp = msg.reader().readShort();
                            if (char7.wp == -1)
                            {
                                char7.setDefaultWeapon();
                            }
                        }
                        break;
                    }
                case 11:
                    {
                        GameCanvas.debug("SA29", 2);
                        Char char6 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char6 != null)
                        {
                            char6.cHP = msg.reader().readLong();
                            char6.cHPFull = msg.reader().readLong();
                            char6.eff5BuffHp = msg.reader().readShort();
                            char6.eff5BuffMp = msg.reader().readShort();
                            char6.body = msg.reader().readShort();
                            if (char6.body == -1)
                            {
                                char6.setDefaultBody();
                            }
                        }
                        break;
                    }
                case 12:
                    {
                        GameCanvas.debug("SA30", 2);
                        Char char5 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char5 != null)
                        {
                            char5.cHP = msg.reader().readLong();
                            char5.cHPFull = msg.reader().readLong();
                            char5.eff5BuffHp = msg.reader().readShort();
                            char5.eff5BuffMp = msg.reader().readShort();
                            char5.leg = msg.reader().readShort();
                            if (char5.leg == -1)
                            {
                                char5.setDefaultLeg();
                            }
                        }
                        break;
                    }
                case 13:
                    {
                        GameCanvas.debug("SA31", 2);
                        int num9 = msg.reader().readInt();
                        Char char4 = ((num9 != Char.myCharz().charID) ? GameScr.findCharInMap(num9) : Char.myCharz());
                        if (char4 != null)
                        {
                            char4.cHP = msg.reader().readLong();
                            char4.cHPFull = msg.reader().readLong();
                            char4.eff5BuffHp = msg.reader().readShort();
                            char4.eff5BuffMp = msg.reader().readShort();
                        }
                        break;
                    }
                case 14:
                    {
                        GameCanvas.debug("SA32", 2);
                        Char char3 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char3 == null)
                        {
                            break;
                        }
                        char3.cHP = msg.reader().readLong();
                        sbyte b5 = msg.reader().readByte();
                        Res.outz("player load hp type= " + b5);
                        if (b5 == 1)
                        {
                            ServerEffect.addServerEffect(11, char3, 5);
                            ServerEffect.addServerEffect(104, char3, 4);
                        }
                        if (b5 == 2)
                        {
                            char3.doInjure();
                        }
                        try
                        {
                            char3.cHPFull = msg.reader().readLong();
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                case 15:
                    {
                        GameCanvas.debug("SA33", 2);
                        Char char2 = GameScr.findCharInMap(msg.reader().readInt());
                        if (char2 != null)
                        {
                            char2.cHP = msg.reader().readLong();
                            char2.cHPFull = msg.reader().readLong();
                            char2.cx = msg.reader().readShort();
                            char2.cy = msg.reader().readShort();
                            char2.statusMe = 1;
                            char2.cp3 = 3;
                            ServerEffect.addServerEffect(109, char2, 2);
                        }
                        break;
                    }
                case 35:
                    {
                        GameCanvas.debug("SY3", 2);
                        int num11 = msg.reader().readInt();
                        Res.outz("CID = " + num11);
                        if (TileMap.mapID == 130)
                        {
                            GameScr.gI().starVS();
                        }
                        if (num11 == Char.myCharz().charID)
                        {
                            Char.myCharz().cTypePk = msg.reader().readByte();
                            if (GameScr.gI().isVS() && Char.myCharz().cTypePk != 0)
                            {
                                GameScr.gI().starVS();
                            }
                            Res.outz("type pk= " + Char.myCharz().cTypePk);
                            Char.myCharz().npcFocus = null;
                            if (!GameScr.gI().isMeCanAttackMob(Char.myCharz().mobFocus))
                            {
                                Char.myCharz().mobFocus = null;
                            }
                            Char.myCharz().itemFocus = null;
                        }
                        else
                        {
                            Char @char = GameScr.findCharInMap(num11);
                            if (@char != null)
                            {
                                Res.outz("type pk= " + @char.cTypePk);
                                @char.cTypePk = msg.reader().readByte();
                                if (@char.isAttacPlayerStatus())
                                {
                                    Char.myCharz().charFocus = @char;
                                }
                            }
                        }
                        for (int i = 0; i < GameScr.vCharInMap.size(); i++)
                        {
                            Char char11 = GameScr.findCharInMap(i);
                            if (char11 != null && char11.cTypePk != 0 && char11.cTypePk == Char.myCharz().cTypePk)
                            {
                                if (!Char.myCharz().mobFocus.isMobMe)
                                {
                                    Char.myCharz().mobFocus = null;
                                }
                                Char.myCharz().npcFocus = null;
                                Char.myCharz().itemFocus = null;
                                break;
                            }
                        }
                        Res.outz("update type pk= ");
                        break;
                    }
                case 61:
                    {
                        string text = msg.reader().readUTF();
                        sbyte[] data = new sbyte[msg.reader().readInt()];
                        msg.reader().read(ref data);
                        if (data.Length == 0)
                        {
                            data = null;
                        }
                        if (text.Equals("KSkill"))
                        {
                            GameScr.gI().onKSkill(data);
                        }
                        else if (text.Equals("OSkill"))
                        {
                            GameScr.gI().onOSkill(data);
                        }
                        else if (text.Equals("CSkill"))
                        {
                            GameScr.gI().onCSkill(data);
                        }
                        break;
                    }
                case 23:
                    {
                        short num = msg.reader().readShort();
                        Skill skill = Skills.get(num);
                        useSkill(skill);
                        if (num != 0 && num != 14 && num != 28)
                        {
                            GameScr.info1.addInfo(mResources.LEARN_SKILL + " " + skill.template.name, 0);
                        }
                        break;
                    }
                case 62:
                    Res.outz("ME UPDATE SKILL");
                    read_UpdateSkill(msg);
                    break;
            }
        }
        catch (Exception ex5)
        {
            Cout.println("Loi tai Sub : " + ex5.ToString());
        }
        finally
        {
            msg?.cleanup();
        }
    }

    private void useSkill(Skill skill)
    {
        if (Char.myCharz().myskill == null)
        {
            Char.myCharz().myskill = skill;
        }
        else if (skill.template.Equals(Char.myCharz().myskill.template))
        {
            Char.myCharz().myskill = skill;
        }
        Char.myCharz().vSkill.addElement(skill);
        if ((skill.template.type == 1 || skill.template.type == 4 || skill.template.type == 2 || skill.template.type == 3) && (skill.template.maxPoint == 0 || (skill.template.maxPoint > 0 && skill.point > 0)))
        {
            if (skill.template.id == Char.myCharz().skillTemplateId)
            {
                Service.gI().selectSkill(Char.myCharz().skillTemplateId);
            }
            Char.myCharz().vSkillFight.addElement(skill);
        }
    }

    public bool readCharInfo(Char c, Message msg)
    {
        try
        {
            c.clevel = msg.reader().readByte();
            c.isInvisiblez = msg.reader().readBoolean();
            c.cTypePk = msg.reader().readByte();
            Res.outz("ADD TYPE PK= " + c.cTypePk + " to player " + c.charID + " @@ " + c.cName);
            c.nClass = GameScr.nClasss[msg.reader().readByte()];
            c.cgender = msg.reader().readByte();
            c.head = msg.reader().readShort();
            c.cName = msg.reader().readUTF();
            c.cHP = msg.reader().readLong();
            c.dHP = c.cHP;
            if (c.cHP == 0)
            {
                c.statusMe = 14;
            }
            c.cHPFull = msg.reader().readLong();
            if (c.cy >= TileMap.pxh - 100)
            {
                c.isFlyUp = true;
            }
            c.body = msg.reader().readShort();
            c.leg = msg.reader().readShort();
            c.bag = msg.reader().readShort();
            Res.outz(" body= " + c.body + " leg= " + c.leg + " bag=" + c.bag + "BAG ==" + c.bag + "*********************************");
            c.isShadown = true;
            sbyte b = msg.reader().readByte();
            if (c.wp == -1)
            {
                c.setDefaultWeapon();
            }
            if (c.body == -1)
            {
                c.setDefaultBody();
            }
            if (c.leg == -1)
            {
                c.setDefaultLeg();
            }
            c.cx = msg.reader().readShort();
            c.cy = msg.reader().readShort();
            c.xSd = c.cx;
            c.ySd = c.cy;
            c.eff5BuffHp = msg.reader().readShort();
            c.eff5BuffMp = msg.reader().readShort();
            int num = msg.reader().readByte();
            for (int i = 0; i < num; i++)
            {
                EffectChar effectChar = new EffectChar(msg.reader().readByte(), msg.reader().readInt(), msg.reader().readInt(), msg.reader().readShort());
                c.vEff.addElement(effectChar);
                if (effectChar.template.type == 12 || effectChar.template.type == 11)
                {
                    c.isInvisiblez = true;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            ex.StackTrace.ToString();
        }
        return false;
    }

    private void readGetImgByName(Message msg)
    {
        try
        {
            string name = msg.reader().readUTF();
            sbyte nFrame = msg.reader().readByte();
            sbyte[] array = null;
            array = NinjaUtil.readByteArray(msg);
            Image img = createImage(array);
            ImgByName.SetImage(name, img, nFrame);
            if (array != null)
            {
            }
        }
        catch (Exception)
        {
        }
    }

    private void createItemNew(myReader d)
    {
        try
        {
            loadItemNew(d, -1, isSave: true);
        }
        catch (Exception)
        {
        }
    }

    private void loadItemNew(myReader d, sbyte type, bool isSave)
    {
        try
        {
            d.mark(1000000);
            GameScr.vcItem = d.readByte();
            type = d.readByte();
            Res.err(GameScr.vcItem + ":<<GameScr.vcItem >>>>>>loadItemNew: " + type + "  isSave:" + isSave);
            switch (type)
            {
                case 0:
                    {
                        GameScr.gI().iOptionTemplates = new ItemOptionTemplate[d.readShort()];
                        for (int i = 0; i < GameScr.gI().iOptionTemplates.Length; i++)
                        {
                            GameScr.gI().iOptionTemplates[i] = new ItemOptionTemplate();
                            GameScr.gI().iOptionTemplates[i].id = i;
                            GameScr.gI().iOptionTemplates[i].name = d.readUTF();
                            GameScr.gI().iOptionTemplates[i].type = d.readByte();
                        }
                        try
                        {
                            short num = d.readShort();
                            for (int j = 0; j < num; j++)
                            {
                                short num2 = d.readShort();
                                GameScr.gI().iOptionTemplates[num2].color = d.readUnsignedByte();
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (isSave)
                        {
                            d.reset();
                            sbyte[] data = new sbyte[d.available()];
                            d.readFully(ref data);
                            Rms.saveRMS("NRitem0", data);
                        }
                        break;
                    }
                case 1:
                    {
                        ItemTemplates.itemTemplates.clear();
                        int num3 = d.readShort();
                        for (int k = 0; k < num3; k++)
                        {
                            ItemTemplate it = new ItemTemplate((short)k, d.readByte(), d.readByte(), d.readUTF(), d.readUTF(), d.readByte(), d.readInt(), d.readShort(), d.readShort(), d.readBoolean());
                            ItemTemplates.add(it);
                        }
                        if (isSave)
                        {
                            d.reset();
                            sbyte[] data2 = new sbyte[d.available()];
                            d.readFully(ref data2);
                            Rms.saveRMS("NRitem1", data2);
                            sbyte[] data3 = new sbyte[1] { GameScr.vcItem };
                            Rms.saveRMS("NRitemVersion", data3);
                        }
                        LoginScr.isUpdateItem = false;
                        GameScr.gI().readOk();
                        break;
                    }
                case 100:
                    Char.Arr_Head_2Fr = readArrHead(d);
                    if (isSave)
                    {
                        d.reset();
                        sbyte[] data4 = new sbyte[d.available()];
                        d.readFully(ref data4);
                        Rms.saveRMS("NRitem100", data4);
                    }
                    break;
                case 101:
                    try
                    {
                        int num4 = d.readShort();
                        Char.Arr_Head_FlyMove = new short[num4];
                        for (int l = 0; l < num4; l++)
                        {
                            short num5 = d.readShort();
                            Char.Arr_Head_FlyMove[l] = num5;
                        }
                        if (isSave)
                        {
                            d.reset();
                            sbyte[] data5 = new sbyte[d.available()];
                            d.readFully(ref data5);
                            Rms.saveRMS("NRitem101", data5);
                        }
                        break;
                    }
                    catch (Exception)
                    {
                        Char.Arr_Head_FlyMove = new short[0];
                        break;
                    }
            }
        }
        catch (Exception ex3)
        {
            ex3.ToString();
        }
    }

    private void readFrameBoss(Message msg, int mobTemplateId)
    {
        try
        {
            int num = msg.reader().readByte();
            int[][] array = new int[num][];
            for (int i = 0; i < num; i++)
            {
                int num2 = msg.reader().readByte();
                array[i] = new int[num2];
                for (int j = 0; j < num2; j++)
                {
                    array[i][j] = msg.reader().readByte();
                }
            }
            frameHT_NEWBOSS.put(mobTemplateId + string.Empty, array);
        }
        catch (Exception)
        {
        }
    }

    private int[][] readArrHead(myReader d)
    {
        int[][] array = new int[1][] { new int[2] { 542, 543 } };
        try
        {
            int num = d.readShort();
            array = new int[num][];
            for (int i = 0; i < array.Length; i++)
            {
                int num2 = d.readByte();
                array[i] = new int[num2];
                for (int j = 0; j < num2; j++)
                {
                    array[i][j] = d.readShort();
                }
            }
            return array;
        }
        catch (Exception)
        {
            return array;
        }
    }

    public void phuban_Info(Message msg)
    {
        try
        {
            sbyte b = msg.reader().readByte();
            if (b == 0)
            {
                readPhuBan_CHIENTRUONGNAMEK(msg, b);
            }
        }
        catch (Exception)
        {
        }
    }

    private void readPhuBan_CHIENTRUONGNAMEK(Message msg, int type_PB)
    {
        try
        {
            switch (msg.reader().readByte())
            {
                case 0:
                    {
                        short idmapPaint = msg.reader().readShort();
                        string nameTeam = msg.reader().readUTF();
                        string nameTeam2 = msg.reader().readUTF();
                        int maxPoint = msg.reader().readInt();
                        short timeSecond = msg.reader().readShort();
                        int maxLife = msg.reader().readByte();
                        GameScr.phuban_Info = new InfoPhuBan(type_PB, idmapPaint, nameTeam, nameTeam2, maxPoint, timeSecond);
                        GameScr.phuban_Info.maxLife = maxLife;
                        GameScr.phuban_Info.updateLife(type_PB, 0, 0);
                        break;
                    }
                case 1:
                    {
                        int pointTeam = msg.reader().readInt();
                        int pointTeam2 = msg.reader().readInt();
                        if (GameScr.phuban_Info != null)
                        {
                            GameScr.phuban_Info.updatePoint(type_PB, pointTeam, pointTeam2);
                        }
                        break;
                    }
                case 2:
                    {
                        sbyte b2 = msg.reader().readByte();
                        short type = 0;
                        short num = -1;
                        switch (b2)
                        {
                            case 1:
                                type = 1;
                                num = 3;
                                break;
                            case 2:
                                type = 2;
                                break;
                        }
                        num = -1;
                        GameScr.phuban_Info = null;
                        GameScr.addEffectEnd(type, num, 0, GameCanvas.hw, GameCanvas.hh, 0, 0, -1, null);
                        break;
                    }
                case 5:
                    {
                        short timeSecond2 = msg.reader().readShort();
                        if (GameScr.phuban_Info != null)
                        {
                            GameScr.phuban_Info.updateTime(type_PB, timeSecond2);
                        }
                        break;
                    }
                case 4:
                    {
                        int lifeTeam = msg.reader().readByte();
                        int lifeTeam2 = msg.reader().readByte();
                        if (GameScr.phuban_Info != null)
                        {
                            GameScr.phuban_Info.updateLife(type_PB, lifeTeam, lifeTeam2);
                        }
                        break;
                    }
            }
        }
        catch (Exception)
        {
        }
    }

    public void read_cmdExtra(Message msg)
    {
        try
        {
            sbyte b = msg.reader().readByte();
            mSystem.println(">>---read_cmdExtra-sub:" + b);
            switch (b)
            {
                case 0:
                    {
                        short idHat = msg.reader().readShort();
                        Char.myCharz().idHat = idHat;
                        SoundMn.gI().getStrOption();
                        break;
                    }
                case 2:
                    {
                        int num = msg.reader().readInt();
                        sbyte b2 = msg.reader().readByte();
                        short num2 = msg.reader().readShort();
                        string v = num2 + "," + b2;
                        MainImage imagePath = ImgByName.getImagePath("banner_" + num2, ImgByName.hashImagePath);
                        GameCanvas.danhHieu.put(num + string.Empty, v);
                        break;
                    }
                case 3:
                    {
                        short num3 = msg.reader().readShort();
                        SmallImage.createImage(num3);
                        BackgroudEffect.id_water1 = num3;
                        break;
                    }
                case 4:
                    {
                        string o = msg.reader().readUTF();
                        GameCanvas.messageServer.addElement(o);
                        break;
                    }
                case 5:
                    {
                        string text = "------------------|ChienTruong|Log: ";
                        text = "\n|ChienTruong|Log: ";
                        sbyte b3 = msg.reader().readByte();
                        switch (b3)
                        {
                            case 0:
                                {
                                    GameScr.nCT_team = msg.reader().readUTF();
                                    GameScr.nCT_TeamA = (GameScr.nCT_TeamB = msg.reader().readByte());
                                    GameScr.nCT_nBoyBaller = GameScr.nCT_TeamA * 2;
                                    GameScr.isPaint_CT = false;
                                    string text4 = text;
                                    text = text4 + "\tsub    0|  nCT_team= " + GameScr.nCT_team + "|nCT_TeamA =" + GameScr.nCT_TeamA + "  isPaint_CT=false \n";
                                    break;
                                }
                            case 1:
                                {
                                    int num4 = msg.reader().readInt();
                                    sbyte b4 = (GameScr.nCT_floor = msg.reader().readByte());
                                    GameScr.nCT_timeBallte = num4 * 1000 + mSystem.currentTimeMillis();
                                    GameScr.isPaint_CT = true;
                                    string text3 = text;
                                    text = text3 + "\tsub    1 floor= " + b4 + "|timeBallte= " + num4 + "isPaint_CT=true \n";
                                    break;
                                }
                            case 2:
                                {
                                    GameScr.nCT_TeamA = msg.reader().readByte();
                                    GameScr.nCT_TeamB = msg.reader().readByte();
                                    GameScr.res_CT.removeAllElements();
                                    sbyte b5 = msg.reader().readByte();
                                    for (int i = 0; i < b5; i++)
                                    {
                                        string empty = string.Empty;
                                        empty = empty + msg.reader().readByte() + "|";
                                        empty = empty + msg.reader().readUTF() + "|";
                                        empty = empty + msg.reader().readShort() + "|";
                                        empty += msg.reader().readInt();
                                        GameScr.res_CT.addElement(empty);
                                    }
                                    string text2 = text;
                                    text = text2 + "\tsub   2|  A= " + GameScr.nCT_TeamA + "|B =" + GameScr.nCT_TeamB + "  isPaint_CT=true \n";
                                    break;
                                }
                            case 3:
                                Service.gI().sendCT_ready(b, b3);
                                GameScr.nCT_floor = 0;
                                GameScr.nCT_timeBallte = 0L;
                                GameScr.isPaint_CT = false;
                                text += "\tsub    3|  isPaint_CT=false \n";
                                break;
                            case 4:
                                GameScr.nUSER_CT = msg.reader().readByte();
                                GameScr.nUSER_MAX_CT = msg.reader().readByte();
                                break;
                        }
                        text += "END LOG CT.";
                        Res.err(text);
                        break;
                    }
                default:
                    readExtra(b, msg);
                    break;
            }
        }
        catch (Exception)
        {
        }
    }

    public void read_UpdateSkill(Message msg)
    {
        try
        {
            short num = msg.reader().readShort();
            sbyte b = -1;
            try
            {
                b = msg.reader().readSByte();
            }
            catch (Exception)
            {
            }
            switch (b)
            {
                case 0:
                    {
                        short curExp = msg.reader().readShort();
                        for (int i = 0; i < Char.myCharz().vSkill.size(); i++)
                        {
                            Skill skill = (Skill)Char.myCharz().vSkill.elementAt(i);
                            if (skill.skillId == num)
                            {
                                skill.curExp = curExp;
                                break;
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        sbyte b2 = msg.reader().readByte();
                        for (int j = 0; j < Char.myCharz().vSkill.size(); j++)
                        {
                            Skill skill2 = (Skill)Char.myCharz().vSkill.elementAt(j);
                            if (skill2.skillId == num)
                            {
                                for (int k = 0; k < 20; k++)
                                {
                                    string nameImg = "Skills_" + skill2.template.id + "_" + b2 + "_" + k;
                                    MainImage imagePath = ImgByName.getImagePath(nameImg, ImgByName.hashImagePath);
                                }
                                break;
                            }
                        }
                        break;
                    }
                case -1:
                    {
                        Skill skill3 = Skills.get(num);
                        for (int l = 0; l < Char.myCharz().vSkill.size(); l++)
                        {
                            Skill skill4 = (Skill)Char.myCharz().vSkill.elementAt(l);
                            if (skill4.template.id == skill3.template.id)
                            {
                                Char.myCharz().vSkill.setElementAt(skill3, l);
                                break;
                            }
                        }
                        for (int m = 0; m < Char.myCharz().vSkillFight.size(); m++)
                        {
                            Skill skill5 = (Skill)Char.myCharz().vSkillFight.elementAt(m);
                            if (skill5.template.id == skill3.template.id)
                            {
                                Char.myCharz().vSkillFight.setElementAt(skill3, m);
                                break;
                            }
                        }
                        for (int n = 0; n < GameScr.onScreenSkill.Length; n++)
                        {
                            if (GameScr.onScreenSkill[n] != null && GameScr.onScreenSkill[n].template.id == skill3.template.id)
                            {
                                GameScr.onScreenSkill[n] = skill3;
                                break;
                            }
                        }
                        for (int num2 = 0; num2 < GameScr.keySkill.Length; num2++)
                        {
                            if (GameScr.keySkill[num2] != null && GameScr.keySkill[num2].template.id == skill3.template.id)
                            {
                                GameScr.keySkill[num2] = skill3;
                                break;
                            }
                        }
                        if (Char.myCharz().myskill.template.id == skill3.template.id)
                        {
                            Char.myCharz().myskill = skill3;
                        }
                        GameScr.info1.addInfo(mResources.hasJustUpgrade1 + skill3.template.name + mResources.hasJustUpgrade2 + skill3.point, 0);
                        break;
                    }
            }
        }
        catch (Exception)
        {
        }
    }

    public void readExtra(sbyte sub, Message msg)
    {
        try
        {
            if (sub != sbyte.MaxValue)
            {
                return;
            }
            GameCanvas.endDlg();
            try
            {
                string text = (ServerListScreen.linkDefault = msg.reader().readUTF());
                mSystem.AddIpTest();
                ServerListScreen.getServerList(ServerListScreen.linkDefault);
                Res.outz(">>>>read.isEXTRA_LINK " + text);
                sbyte b = msg.reader().readByte();
                if (b > 0)
                {
                    ServerListScreen.typeClass = new sbyte[b];
                    ServerListScreen.listChar = new Char[b];
                    for (int i = 0; i < b; i++)
                    {
                        ServerListScreen.typeClass[i] = msg.reader().readByte();
                        Res.outz(ServerListScreen.nameServer[i] + ">>>>read.isEXTRA_LINK  typeClass: " + ServerListScreen.typeClass[i]);
                        if (ServerListScreen.typeClass[i] > -1)
                        {
                            ServerListScreen.isHaveChar = true;
                            ServerListScreen.listChar[i] = new Char();
                            ServerListScreen.listChar[i].cgender = ServerListScreen.typeClass[i];
                            ServerListScreen.listChar[i].head = msg.reader().readShort();
                            ServerListScreen.listChar[i].body = msg.reader().readShort();
                            ServerListScreen.listChar[i].leg = msg.reader().readShort();
                            ServerListScreen.listChar[i].bag = msg.reader().readShort();
                            ServerListScreen.listChar[i].cName = msg.reader().readUTF();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            isEXTRA_LINK = true;
            ServerListScreen.saveRMS_ExtraLink();
            ServerListScreen.isWait = false;
            Char.isLoadingMap = false;
            LoginScr.isContinueToLogin = false;
            ServerListScreen.waitToLogin = false;
            bool flag = false;
            bool flag2 = false;
            try
            {
                if (!Rms.loadRMSString("acc").Equals(string.Empty))
                {
                    flag = true;
                }
                if (!Rms.loadRMSString("userAo" + ServerListScreen.ipSelect).Equals(string.Empty))
                {
                    flag2 = true;
                }
            }
            catch (Exception)
            {
            }
            if (!ServerListScreen.isHaveChar && !flag && !flag2)
            {
                GameCanvas.serverScreen.Login_New();
                return;
            }
            if (Rms.loadRMSInt(ServerListScreen.RMS_svselect) == -1)
            {
                ServerScr.isShowSv_HaveChar = false;
                GameCanvas.serverScr.switchToMe();
                return;
            }
            ServerListScreen.SetIpSelect(Rms.loadRMSInt(ServerListScreen.RMS_svselect), issave: false);
            if (ServerListScreen.listChar != null && ServerListScreen.listChar[ServerListScreen.ipSelect] != null)
            {
                GameCanvas._SelectCharScr.SetInfoChar(ServerListScreen.listChar[ServerListScreen.ipSelect]);
            }
            else
            {
                GameCanvas.serverScreen.Login_New();
            }
        }
        catch (Exception)
        {
            Res.outz(">>>>read.isEXTRA_LINK  errr:");
            GameCanvas.serverScr.switchToMe();
        }
    }

    public ItemOption readItemOption(Message msg)
    {
        ItemOption result = null;
        try
        {
            int num = msg.reader().readShort();
            int param = msg.reader().readInt();
            if (num != -1)
            {
                result = new ItemOption(num, param);
                return result;
            }
            return result;
        }
        catch (Exception)
        {
            Res.err(">>>>read.ItemOption  errr:");
            return result;
        }
    }

    public void read_cmdExtraBig(Message msg)
    {
        try
        {
            sbyte b = msg.reader().readByte();
            mSystem.println(">>---read_cmdExtraBig-sub:" + b);
            if (b == 0)
            {
                loadItemNew(msg.reader(), 1, isSave: true);
            }
        }
        catch (Exception)
        {
        }
    }
}
