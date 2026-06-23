package mod.Auto.Modules;

import main.GameCanvas;
import mod.Auto.Base.AutoModule;
import mod.DataBase.AutoConfig;
import mod.Auto.Utils.AutoTimerService;
import mod.UI.Panels.SettingPanel;
import mod.UtilsGame.PathUtils;
import mod.Xmap.XmapController;
import nro.*;
import nro.Item;

import java.util.Vector;

public class AutoMuaBua extends AutoModule implements IActionListener {

    private static final int NPC_ID = 21;
    private static final String MENU_BUA = "Cửa hàng Bùa";
    private static final int RETRY_MAX = 8;
    private static final long TICK_MS = 800L;
    private static final long CONFIRM_DELAY_MS = 350L;
    private static final float BUY_BUFFER_MIN = 2f;

    public static final String DUR_1H = "1 giờ";
    public static final String DUR_8H = "8 giờ";
    public static final String DUR_1M = "1 tháng";

    private static final String MENU_DUR_1H = "Bùa dùng 1 giờ";
    private static final String MENU_DUR_8H = "Bùa dùng 8 giờ";
    private static final String MENU_DUR_1M = "Bùa dùng 1 tháng";

    public static class BuaEntry {
        public int itemId;
        public boolean enabled;

        public float remainMinutes;
        public long readAtMs;
        public boolean boughtThisCycle;

        public BuaEntry(int id, boolean on) {
            itemId = id;
            enabled = on;
            remainMinutes = -1f;
            readAtMs = 0;
            boughtThisCycle = false;
        }

        public float getCurrentRemain() {
            if (remainMinutes <= 0)
                return 0f;
            float elapsed = (System.currentTimeMillis() - readAtMs) / 60000f;
            float r = remainMinutes - elapsed;
            return r > 0 ? r : 0f;
        }

        public void setRemain(float minutes) {
            remainMinutes = minutes;
            readAtMs = System.currentTimeMillis();
        }

        public boolean needBuy() {
            return getCurrentRemain() <= BUY_BUFFER_MIN;
        }

        public void resetCycle() {
            boughtThisCycle = false;
        }
    }

    private String globalDuration = DUR_1H;

    public Vector buaList = new Vector();

    private int step = 0;
    private int entryIdx = -1;
    private int retryCount = 0;
    private long lastConfirmMs = 0;
    private boolean menuOpened = false;
    private boolean menuConfirmed = false;

    private boolean cacheReady = false;

    private int savedMap = -1, savedZone = 0, savedX = 0, savedY = 0;
    private boolean savedTrain = false, savedGoback = false;
    private boolean savedBuy = false, savedSell = false;

    private static AutoMuaBua instance;

    public static AutoMuaBua gI() {
        if (instance == null)
            instance = new AutoMuaBua();
        return instance;
    }

    private AutoMuaBua() {
        super();
    }

    private final AutoTimerService timerService = new AutoTimerService();

    private static final int ACT_TOGGLE = 1;
    private static final int ACT_BUA_TOGGLE = 2;
    private static final int ACT_DUR = 3;
    private static final int ACT_BACK = 4;

    public float durationMinutes() {
        if (DUR_8H.equals(globalDuration))
            return 480f;
        if (DUR_1M.equals(globalDuration))
            return 43200f;
        return 60f;
    }

    private String durationMenuText() {
        if (DUR_8H.equals(globalDuration))
            return MENU_DUR_8H;
        if (DUR_1M.equals(globalDuration))
            return MENU_DUR_1M;
        return MENU_DUR_1H;
    }

    private int buaSize() {
        return buaList.size();
    }

    private BuaEntry buaGet(int i) {
        return (BuaEntry) buaList.elementAt(i);
    }

    protected void onInit() {
        initBuaList();
        loadConfig();
        reset();
        timerService.addTask(new AutoTimerService.Task(TICK_MS) {
            protected void execute() {
                if (!autoEnabled)
                    return;
                runStep();
            }
        });
        if (AutoConfig.loadBool("buabua.auto", false)) {
            startAuto();
        }
    }

    private void initBuaList() {
        buaList.removeAllElements();
        buaList.addElement(new BuaEntry(213, false)); // Bùa Trí Tuệ
        buaList.addElement(new BuaEntry(214, false)); // Bùa Mạnh Mẽ
        buaList.addElement(new BuaEntry(215, false)); // Bùa Da Trâu
        buaList.addElement(new BuaEntry(216, false)); // Bùa Oai Hùng
        buaList.addElement(new BuaEntry(217, false)); // Bùa Bất Tử
        buaList.addElement(new BuaEntry(218, false)); // Bùa Dẻo Dai
        buaList.addElement(new BuaEntry(219, false)); // Bùa Thu Hút
        buaList.addElement(new BuaEntry(522, false)); // Bùa Đệ Tử
        buaList.addElement(new BuaEntry(671, false)); // Bùa Trí Tuệ x3
        buaList.addElement(new BuaEntry(672, false)); // Bùa Trí Tuệ x4
    }

    protected void onDispose() {
        timerService.clear();
        saveConfig();
    }

    protected void performAuto() {
        timerService.update();
    }

    protected void onAutoStart() {
        cacheReady = false;
    }

    protected void onAutoStop() {
        reset();
    }

    private void runStep() {
        switch (step) {
            case 0:
                stepIdle();
                break;
            case 1:
                stepGoMap();
                break;
            case 2:
                stepTeleNpc();
                break;
            case 3:
                stepOpenMenu();
                break;
            case 4:
                stepWaitShop();
                break;
            case 5:
                stepReadTime();
                break;
            case 6:
                stepSelectAndBuy();
                break;
            case 7:
                stepWaitDurMenu();
                break;
            case 8:
                stepAfterBuy();
                break;
            case 9:
                stepGoBack();
                break;
        }
    }

    private void stepIdle() {
        if (Char.myCharz() == null)
            return;
        if (!(GameCanvas.currentScreen instanceof GameScr))
            return;
        if (Char.isLoadingMap)
            return;
        if (AutoBuy.gI().isRunning() || AutoSellTrash.gI().isRunning())
            return;

        boolean needGo = false;
        if (!cacheReady) {
            needGo = true;
        } else {

            float minRemain = Float.MAX_VALUE;
            int minIdx = -1;
            for (int i = 0; i < buaSize(); i++) {
                BuaEntry e = buaGet(i);
                if (!e.enabled)
                    continue;
                float remain = e.getCurrentRemain();
                if (remain < minRemain) {
                    minRemain = remain;
                    minIdx = i;
                }
            }

            if (minIdx >= 0 && minRemain <= BUY_BUFFER_MIN) {
                needGo = true;
            }
        }

        if (needGo) {
            for (int i = 0; i < buaSize(); i++)
                buaGet(i).resetCycle();
            savePos();
            pauseOthers();
            goTo(1);
        }
    }

    private void stepGoMap() {
        if (Char.myCharz() == null)
            return;
        int target = Char.myCharz().cgender + 42;
        if (TileMap.mapID == target) {
            retryCount = 0;
            goTo(2);
            return;
        }
        if (!XmapController.isNavigating()) {
            if (retryCount++ >= RETRY_MAX) {
                abort("Không xmap được");
                return;
            }
            XmapController.startXmap(target);
        }
    }

    private void stepTeleNpc() {
        if (Char.isLoadingMap)
            return;
        Npc npc = findNpc(NPC_ID);
        if (npc == null) {
            if (retryCount++ >= RETRY_MAX) {
                abort("Không thấy NPC " + NPC_ID);
                return;
            }
            return;
        }
        if (dist(Char.myCharz().cx, Char.myCharz().cy, npc.cx, npc.cy) > 8) {
            PathUtils.teleportTo(npc.cx, npc.cy - 3);
            retryCount = 0;
            return;
        }
        Char.myCharz().npcFocus = npc;
        menuOpened = false;
        menuConfirmed = false;
        retryCount = 0;
        goTo(3);
    }

    private void stepOpenMenu() {
        if (isShopOpen()) {
            clearStaleMenu();
            menuOpened = false;
            menuConfirmed = false;
            retryCount = 0;
            goTo(4);
            return;
        }

        long now = System.currentTimeMillis();

        if (!menuOpened) {
            Service.gI().openMenu(NPC_ID);
            menuOpened = true;
            menuConfirmed = false;
            lastConfirmMs = now;
            return;
        }

        if (now - lastConfirmMs < CONFIRM_DELAY_MS)
            return;

        if (now - lastConfirmMs >= 2000L) {
            if (retryCount++ >= RETRY_MAX) {
                abort("Không mở được shop");
                return;
            }
            Service.gI().confirmMenu((short) NPC_ID, (byte) 0);
            GameCanvas.menu.doCloseMenu();
            Char.chatPopup = null;
            menuOpened = false;
            menuConfirmed = false;
            return;
        }

        if (!menuConfirmed) {
            if (confirmByName(MENU_BUA)) {
                menuConfirmed = true;
                lastConfirmMs = now;
            }
        }
    }

    private void stepWaitShop() {
        if (shopReady()) {
            clearStaleMenu();
            retryCount = 0;
            goTo(5);
            return;
        }
        if (retryCount++ >= RETRY_MAX) {
            abort("Shop không load");
        }
    }

    private void stepReadTime() {
        for (int i = 0; i < buaSize(); i++) {
            BuaEntry e = buaGet(i);
            Item it = findInShop(e.itemId);

            if (it == null) {
                if (e.boughtThisCycle) {
                    e.setRemain(durationMinutes());
                } else {
                    e.setRemain(0f);
                }
                continue;
            }

            try {
                if (it.itemOption != null && it.itemOption.length > 0 && it.itemOption[0] != null) {
                    String str = "";
                    try {
                        str = it.itemOption[0].getOptionString();
                    } catch (Exception ex) {
                    }
                    float parsed = parseRemainStr(str);

                    if (e.boughtThisCycle && parsed <= BUY_BUFFER_MIN) {
                        e.setRemain(durationMinutes());
                    } else {
                        e.setRemain(parsed);
                    }
                } else {
                    if (e.boughtThisCycle) {
                        e.setRemain(durationMinutes());
                    } else {
                        e.setRemain(0f);
                    }
                }
            } catch (Exception ignored) {
                e.setRemain(e.boughtThisCycle ? durationMinutes() : 0f);
            }
        }

        cacheReady = true;
        goTo(6);
    }

    private float parseRemainStr(String str) {
        if (str == null || str.length() == 0)
            return 0f;
        String s = str.toLowerCase();
        if (s.indexOf("ch\u01B0a") >= 0 || s.indexOf("chua") >= 0)
            return 0f;

        int num = 0;
        boolean found = false;
        for (int i = 0; i < s.length(); i++) {
            char c = s.charAt(i);
            if (c >= '0' && c <= '9') {
                num = num * 10 + (c - '0');
                found = true;
            } else if (found)
                break;
        }
        if (!found || num == 0)
            return 0f;

        if (s.indexOf("ng\u00E0y") >= 0 || s.indexOf("ngay") >= 0)
            return num * 1440f;
        if (s.indexOf("gi\u1EDD") >= 0 || s.indexOf("gio") >= 0)
            return num * 60f;
        return (float) num;
    }

    private void stepSelectAndBuy() {

        entryIdx = -1;
        for (int i = 0; i < buaSize(); i++) {
            BuaEntry e = buaGet(i);
            if (e.enabled && e.remainMinutes <= BUY_BUFFER_MIN && !e.boughtThisCycle) {
                entryIdx = i;
                break;
            }
        }

        if (entryIdx < 0) {
            if (GameCanvas.panel != null && GameCanvas.panel.isShow)
                GameCanvas.panel.hide();
            goTo(9);
            return;
        }

        BuaEntry entry = buaGet(entryIdx);

        clearStaleMenu();

        int[] ts = findTabSlot(entry.itemId);
        if (ts == null) {
            entry.setRemain(9999f);
            return;
        }

        Panel panel = GameCanvas.panel;
        if (panel != null && panel.isShow) {
            panel.currentTabIndex = ts[0];
            panel.selected = ts[1];
            Service.gI().buyItem((byte) 1, entry.itemId, 0);
            entry.boughtThisCycle = true;
            lastConfirmMs = System.currentTimeMillis();
            retryCount = 0;
            goTo(7);
        }
    }

    private void stepWaitDurMenu() {
        long now = System.currentTimeMillis();
        if (now - lastConfirmMs < CONFIRM_DELAY_MS)
            return;

        if (GameCanvas.menu != null && GameCanvas.menu.menuItems != null
                && GameCanvas.menu.menuItems.size() > 0) {

            String durText = durationMenuText();
            if (confirmByName(durText)) {
                lastConfirmMs = now;
                goTo(8);
            } else {
                if (retryCount++ >= RETRY_MAX) {
                    abort("Không confirm '" + durText + "'");
                }
            }
            return;
        }

        if (now - lastConfirmMs >= 5000L) {
            if (retryCount++ >= RETRY_MAX) {
                abort("Không có menu duration");
                return;
            }
            goTo(6);
        }
    }

    private void stepAfterBuy() {

        if (ClientInput.instance != null) {
            try {
                ClientInput.instance.tf[0].setText("1");
                Service.gI().sendClientInput(ClientInput.instance.tf);
                GameScr.instance.switchToMe();
                ClientInput.instance = null;
            } catch (Exception ignored) {
            }
            return;
        }

        if (entryIdx >= 0 && entryIdx < buaSize()) {
            BuaEntry entry = buaGet(entryIdx);
            entry.setRemain(durationMinutes());
        }

        retryCount = 0;
        goTo(6);
    }

    private void stepGoBack() {
        if (savedMap < 0) {
            done();
            return;
        }
        if (TileMap.mapID != savedMap) {
            if (!XmapController.isNavigating()) {
                if (retryCount++ >= RETRY_MAX) {
                    done();
                    return;
                }
                XmapController.startXmap(savedMap);
            }
            return;
        }
        if (TileMap.zoneID != savedZone) {
            Service.gI().requestChangeZone(savedZone);
            return;
        }
        if (dist(Char.myCharz().cx, Char.myCharz().cy, savedX, savedY) > 15) {
            PathUtils.teleportTo(savedX, savedY);
            return;
        }
        done();
    }

    private void clearStaleMenu() {
        try {
            if (GameCanvas.menu != null && GameCanvas.menu.menuItems != null
                    && GameCanvas.menu.menuItems.size() > 0) {
                GameCanvas.menu.doCloseMenu();
                Char.chatPopup = null;
            }
        } catch (Exception ignored) {
        }
    }

    private boolean confirmByName(String menuName) {
        if (menuName == null || GameCanvas.menu == null || GameCanvas.menu.menuItems == null)
            return false;
        String search = toLowerCase(replaceChars(menuName)).trim();
        for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++) {
            try {
                nro.Command cmd = (nro.Command) GameCanvas.menu.menuItems.elementAt(i);
                if (cmd == null || cmd.caption == null)
                    continue;
                String text = toLowerCase(replaceChars(cmd.caption)).trim();
                if (text.equals(search) || text.indexOf(search) >= 0) {
                    Service.gI().confirmMenu((short) NPC_ID, (byte) i);
                    GameCanvas.menu.doCloseMenu();
                    Char.chatPopup = null;
                    return true;
                }
            } catch (Exception ignored) {
            }
        }
        return false;
    }

    private String toLowerCase(String s) {
        return s == null ? "" : s.toLowerCase();
    }

    private String replaceChars(String s) {
        return s == null ? "" : s.replace('\r', ' ').replace('\n', ' ');
    }

    private Item findInShop(int itemId) {
        try {
            Item[][] shop = Char.myCharz().arrItemShop;
            if (shop == null)
                return null;
            for (Item[] tab : shop) {
                if (tab == null)
                    continue;
                for (Item it : tab) {
                    if (it != null && it.template != null && it.template.id == itemId)
                        return it;
                }
            }
        } catch (Exception ignored) {
        }
        return null;
    }

    private int[] findTabSlot(int itemId) {
        try {
            Item[][] shop = Char.myCharz().arrItemShop;
            if (shop == null)
                return null;
            for (int t = 0; t < shop.length; t++) {
                if (shop[t] == null)
                    continue;
                for (int s = 0; s < shop[t].length; s++) {
                    Item it = shop[t][s];
                    if (it != null && it.template != null && it.template.id == itemId)
                        return new int[] { t, s };
                }
            }
        } catch (Exception ignored) {
        }
        return null;
    }

    private boolean isShopOpen() {
        return GameCanvas.panel != null && GameCanvas.panel.isShow
                && Char.myCharz() != null && Char.myCharz().arrItemShop != null;
    }

    private boolean shopReady() {
        try {
            Item[][] shop = Char.myCharz().arrItemShop;
            return shop != null && shop.length > 0 && shop[0] != null && shop[0].length > 0;
        } catch (Exception e) {
            return false;
        }
    }

    private Npc findNpc(int id) {
        try {
            for (int i = 0; i < GameScr.vNpc.size(); i++) {
                Npc n = (Npc) GameScr.vNpc.elementAt(i);
                if (n != null && n.template != null && n.template.npcTemplateId == id)
                    return n;
            }
        } catch (Exception ignored) {
        }
        return null;
    }

    private int dist(int x1, int y1, int x2, int y2) {
        int dx = x1 - x2, dy = y1 - y2;
        return (int) Math.sqrt(dx * dx + dy * dy);
    }

    private void savePos() {
        if (Char.myCharz() == null)
            return;
        savedMap = TileMap.mapID;
        savedZone = TileMap.zoneID;
        savedX = Char.myCharz().cx;
        savedY = Char.myCharz().cy;
    }

    private void pauseOthers() {
        try {
            savedTrain = AutoTrain.gI().isAutoEnabled();
            if (savedTrain)
                AutoTrain.gI().stopAuto();
        } catch (Exception ignored) {
        }
        try {
            savedGoback = Goback.gI().isAutoEnabled();
            if (savedGoback)
                Goback.gI().stopAuto();
        } catch (Exception ignored) {
        }
        try {
            savedBuy = AutoBuy.gI().isBuyItem;
            if (savedBuy)
                AutoBuy.gI().isBuyItem = false;
        } catch (Exception ignored) {
        }
        try {
            savedSell = AutoSellTrash.gI().isBanDo;
            if (savedSell)
                AutoSellTrash.gI().isBanDo = false;
        } catch (Exception ignored) {
        }
    }

    private void resumeOthers() {
        try {
            if (savedTrain)
                AutoTrain.gI().startAuto();
        } catch (Exception ignored) {
        }
        try {
            if (savedGoback)
                Goback.gI().startAuto();
        } catch (Exception ignored) {
        }
        try {
            if (savedBuy)
                AutoBuy.gI().isBuyItem = true;
        } catch (Exception ignored) {
        }
        try {
            if (savedSell)
                AutoSellTrash.gI().isBanDo = true;
        } catch (Exception ignored) {
        }
        savedTrain = savedGoback = savedBuy = savedSell = false;
    }

    private void goTo(int s) {
        step = s;
        retryCount = 0;
        lastConfirmMs = 0;
    }

    private void abort(String msg) {
        resumeOthers();
        reset();
    }

    private void done() {
        resumeOthers();
        reset();
    }

    private void reset() {
        step = retryCount = 0;
        entryIdx = -1;
        lastConfirmMs = 0;
        menuOpened = false;
        menuConfirmed = false;
        savedMap = -1;
        savedZone = savedX = savedY = 0;
    }

    private void loadConfig() {
        for (int i = 0; i < buaSize(); i++) {
            buaGet(i).enabled = AutoConfig.loadBool("buabua." + i + ".on", buaGet(i).enabled);
        }
        globalDuration = AutoConfig.loadString("buabua.duration", DUR_1H);
    }

    private void saveConfig() {
        for (int i = 0; i < buaSize(); i++) {
            AutoConfig.saveBool("buabua." + i + ".on", buaGet(i).enabled);
        }
        AutoConfig.saveString("buabua.duration", globalDuration);
        AutoConfig.saveBool("buabua.auto", autoEnabled);
    }

    public void showMenu() {
        SettingPanel menu = SettingPanel.getInstance().create()
                .title("Auto Mua Bùa");

        menu.addAction("Trạng thái", autoEnabled ? "Bật" : "Tắt", ACT_TOGGLE, null);

        for (int i = 0; i < buaSize(); i++) {
            BuaEntry e = buaGet(i);
            String name = getItemName(e.itemId);
            String cache = cacheReady ? " (" + formatMinutes(e.getCurrentRemain()) + ")" : "";
            menu.addAction(name + cache, e.enabled ? "Bật" : "Tắt", ACT_BUA_TOGGLE, i);
        }

        menu.addAction("Thời lượng: " + globalDuration, "", ACT_DUR, null);
        menu.addAction("Quay lại", "", ACT_BACK, null);
        menu.listener(this);
        menu.show();
    }

    private String formatMinutes(float m) {
        if (m <= 0)
            return "Chưa có";
        if (m >= 1440)
            return (int) (m / 1440) + " ngày " + (int) ((m % 1440) / 60) + " giờ";
        if (m >= 60)
            return (int) (m / 60) + " giờ " + (int) (m % 60) + " phút";
        return (int) m + " phút";
    }

    private String getItemName(int itemId) {
        Item it = findInShop(itemId);
        if (it != null && it.template != null)
            return it.template.name;

        switch (itemId) {
            case 213:
                return "Bùa Trí Tuệ";
            case 214:
                return "Bùa Mạnh Mẽ";
            case 215:
                return "Bùa Da Trâu";
            case 216:
                return "Bùa Oai Hùng";
            case 217:
                return "Bùa Bất Tử";
            case 218:
                return "Bùa Dẻo Dai";
            case 219:
                return "Bùa Thu Hút";
            case 522:
                return "Bùa Đệ Tử";
            case 671:
                return "Bùa Trí Tuệ x3";
            case 672:
                return "Bùa Trí Tuệ x4";
            default:
                return "id=" + itemId;
        }
    }

    public void perform(int idAction, Object p) {
        switch (idAction) {
            case ACT_TOGGLE:
                toggleAuto();
                cacheReady = false;
                saveConfig();
                GameScr.info1.addInfo("Auto Mua Bùa: " + (autoEnabled ? "Bật" : "Tắt"), 0);
                showMenu();
                break;

            case ACT_BUA_TOGGLE:
                if (p instanceof Integer) {
                    int idx = (Integer) p;
                    if (idx >= 0 && idx < buaSize()) {
                        BuaEntry e = buaGet(idx);
                        e.enabled = !e.enabled;
                        saveConfig();
                        GameScr.info1.addInfo(getItemName(e.itemId) + ": " + (e.enabled ? "Bật" : "Tắt"), 0);
                    }
                }
                showMenu();
                break;

            case ACT_DUR:
                if (DUR_1H.equals(globalDuration))
                    globalDuration = DUR_8H;
                else if (DUR_8H.equals(globalDuration))
                    globalDuration = DUR_1M;
                else
                    globalDuration = DUR_1H;
                cacheReady = false;
                saveConfig();
                GameScr.info1.addInfo("Thời lượng: " + globalDuration, 0);
                showMenu();
                break;

            case ACT_BACK:
                MainMod.gI().ShowMain();
                break;
        }
    }

    public boolean isRunning() {
        return step > 0;
    }
}