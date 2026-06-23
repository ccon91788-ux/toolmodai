using System;
using System.Threading;
using LoadAssets;
using NRO_v247;

public class Main
{
    public static Main main;
    public static mGraphics g;
    public static GameMidlet midlet;
    public static int f;
    public static int numberQuit = 1;
    public static int typeClient = 4;
    public static string res = "Assets";
    public static bool isPC = true;
    public static bool isMiniApp = true;
    public static bool isWindowsPhone, isIPhone, IphoneVersionApp;
    public static string mainThreadName;
    public static bool started;

    private bool isRun;
    public static GameWindow WindowInstance;

    public void InitializeConsole(GameWindow wnd)
    {
        WindowInstance = wnd;
        if (started) return;

        if (Thread.CurrentThread.Name != "Main")
            Thread.CurrentThread.Name = "Main";

        mainThreadName = Thread.CurrentThread.Name;
        isPC = true;
        started = true;

        SetInit();
    }

    private void SetInit()
    {
        if (isRun) return;

        isRun = true;
        main = this;
        typeClient = 4;
        g = new mGraphics();
        midlet = new GameMidlet();

        try
        {
            TileMap.loadBg();
            PaintG.loadbg();
            PopUp.loadBg();
            GameScr.loadBg();
            InfoMe.gI().loadCharId();
            PanelG.loadBg();
            Menu.loadBg();
            Key.mapKeyPC();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading: {ex.Message}");
        }

        NRO_v247.Mods.ModManager.Init();
    }

    public void doClearRMS()
    {
        Rms.clearAll();
        Rms.saveRMSInt("lastZoomlevel", 1);
        Rms.saveRMSInt("levelScreenKN", 0);
    }

    public void OnApplicationQuit()
    {
        try
        {
            GameCanvas.bRun = false;
            Session_ME.gI()?.close();
            Session_ME2.gI()?.close();
            g?.DisposeResources();
            WindowInstance.Dispose();
            AssetBundle.DisposeAll();
            Environment.Exit(0);
        }
        catch { }
    }

    public static void exit()
    {
        main?.OnApplicationQuit();
    }
}
