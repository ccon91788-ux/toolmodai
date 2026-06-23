using System.Collections.Generic;

namespace Panel.Models;

public class AppConfig
{
    public bool HideAccount { get; set; } = false;
    public bool AutoLogin { get; set; } = false;
    public bool AutoHideClient { get; set; } = false;
    public int AutoLoginThread { get; set; } = 2;

    // Captcha Settings
    public string CaptchaApiServer { get; set; } = "http://api.tooltvt.com/api?token=";
    public string CaptchaApiKey { get; set; } = "";



    // Window Settings
    public int WindowWidth { get; set; } = 267;
    public int WindowHeight { get; set; } = 300;
    public int TargetFPS { get; set; } = 45;

    // Auto Clean RAM
    public bool AutoCleanRam { get; set; } = true;
    public int AutoCleanRamIntervalMinutes { get; set; } = 1;

    // Main Tab Visibility
    public List<string> VisibleMainTabs { get; set; } = new();
}
