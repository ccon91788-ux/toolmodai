using System;
using System.Linq;
using System.Text;
using Assets.src.g;
using NRO_v247.Mods;

namespace NRO_v247.Mods.UpZin;

public class AutoUpZinFeature : IAutoFeature
{
    private readonly Random _random = new();

    private bool _enabled;
    private string _prefix = string.Empty;
    private int _targetClass = -1;

    private long _nextCreateAttemptAtMs;
    private int _retryCount;
    private string _lastRequestedName = string.Empty;
    private long _nextInfoSubmitAtMs;

    private const int MaxRetry = 20;
    private const long RetryDelayMs = 1400L;
    private const long InfoSubmitDelayMs = 1800L;

    public bool IsActive => _enabled;
    public string CurrentState
    {
        get
        {
            if (!_enabled) return "Tắt";
            string scrName = GameCanvas.currentScreen?.GetType().Name ?? "null";
            if (GameCanvas.currentScreen is CreateCharScr)
                return $"Đang tạo NV ({_retryCount + 1})";
            if (GameCanvas.currentScreen is RegisterScreen)
                return "Đang ĐK thông tin...";
            return $"Chờ ở {scrName}";
        }
    }
    public bool IsUtilityTask => false;
    public int Priority => 1000;
    public bool IsRequested => _enabled 
        && !(GameCanvas.currentScreen is GameScr)
        && !(GameCanvas.currentScreen is LoginScr)
        && !(GameCanvas.currentScreen is ServerListScreen)
        && !(GameCanvas.currentScreen is ClientInput);

    public void ApplySettingsFromPanel(bool enabled, string prefix, int targetClass)
    {
        _enabled = enabled;
        _prefix = NormalizePrefix(prefix);
        _targetClass = targetClass;
        ResetRuntime();

        if (_enabled)
        {
            string classStr = _targetClass switch { 0 => "Trái Đất", 1 => "Namek", 2 => "Xayda", _ => "Ngẫu nhiên" };
            LogToPanel($"Up Zin ON - prefix: {(string.IsNullOrEmpty(_prefix) ? "(fallback)" : _prefix)} - Phái: {classStr}");
        }
        else
            LogToPanel("Up Zin OFF");
    }

    public void DisableFromPanel()
    {
        _enabled = false;
        ResetRuntime();
        LogToPanel("Up Zin OFF");
    }

    public void Update()
    {
        if (!_enabled) return;

        if (GameCanvas.currentScreen is RegisterScreen registerScreen)
        {
            HandleRegisterScreen(registerScreen);
            return;
        }

        if (GameCanvas.currentScreen is not CreateCharScr)
        {
            if (_retryCount != 0 || !string.IsNullOrEmpty(_lastRequestedName))
                ResetCreateRuntime();
            return;
        }

        long now = mSystem.currentTimeMillis();
        if (now < _nextCreateAttemptAtMs)
            return;

        if (_retryCount >= MaxRetry)
        {
            LogToPanel("Up Zin: Dừng tạo char (vượt quá số lần thử).");
            _enabled = false;
            return;
        }

        string nextName = BuildCharacterName();
        if (string.IsNullOrEmpty(nextName))
        {
            _nextCreateAttemptAtMs = now + RetryDelayMs;
            return;
        }

        _lastRequestedName = nextName;
        _retryCount++;
        _nextCreateAttemptAtMs = now + RetryDelayMs;

        try
        {
            if (CreateCharScr.tAddName != null)
                CreateCharScr.tAddName.setText(nextName);

            int gender = _targetClass;
            if (gender < 0 || gender > 2)
            {
                gender = _random.Next(3);
            }
            int hairIndex = _random.Next(CreateCharScr.hairID[gender].Length);
            short hairId = (short)CreateCharScr.hairID[gender][hairIndex];

            Service.gI().createChar(nextName, (sbyte)gender, hairId);
            
            string classStr = gender switch { 0 => "Trái Đất", 1 => "Namek", 2 => "Xayda", _ => "?" };
            LogToPanel($"Up Zin: Tạo char thử lần {_retryCount} - {nextName} ({classStr})");
        }
        catch (Exception ex)
        {
            LogToPanel("Up Zin: Lỗi gửi tạo char - " + ex.Message);
        }
    }

    public void OnServerMessage(string fullText)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(fullText))
            return;

        string msg = fullText.ToLowerInvariant();
        if (!IsDuplicateNameMessage(msg))
            return;

        if (GameCanvas.currentScreen is not CreateCharScr)
            return;

        _nextCreateAttemptAtMs = mSystem.currentTimeMillis() + RetryDelayMs;
        LogToPanel("Up Zin: Tên đã tồn tại, đang random lại...");
    }

    private bool IsDuplicateNameMessage(string message)
    {
        return (message.Contains("tồn tại") && message.Contains("tên"))
            || message.Contains("name already")
            || message.Contains("already exists")
            || message.Contains("duplicate name");
    }

    private string BuildCharacterName()
    {
        string prefix = _prefix;
        if (prefix.Length < 3)
            prefix = "zin";

        if (prefix.Length > 4)
            prefix = prefix.Substring(0, 4);

        int minSuffix = Math.Max(1, 5 - prefix.Length);
        int maxSuffix = Math.Max(minSuffix, 15 - prefix.Length);

        int suffixLen = _random.Next(minSuffix, Math.Min(maxSuffix, 8) + 1);
        return prefix + RandomAlphaNumeric(suffixLen);
    }

    private string NormalizePrefix(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        char[] chars = raw.Trim()
            .Where(char.IsLetterOrDigit)
            .Take(4)
            .ToArray();

        return new string(chars).ToLowerInvariant();
    }

    private string RandomAlphaNumeric(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append(chars[_random.Next(chars.Length)]);
        return sb.ToString();
    }

    private void ResetRuntime()
    {
        ResetCreateRuntime();
        _nextInfoSubmitAtMs = 0L;
    }

    private void ResetCreateRuntime()
    {
        _retryCount = 0;
        _nextCreateAttemptAtMs = 0L;
        _lastRequestedName = string.Empty;
    }

    private void HandleRegisterScreen(RegisterScreen registerScreen)
    {
        long now = mSystem.currentTimeMillis();
        if (now < _nextInfoSubmitAtMs)
            return;

        try
        {
            registerScreen.tfUser?.setText("Phạm Văn A");
            registerScreen.tfSodt?.setText("0312345678");
            registerScreen.tfNgay?.setText("1");
            registerScreen.tfThang?.setText("1");
            registerScreen.tfNam?.setText("2000");

            registerScreen.perform(2008, null);
            _nextInfoSubmitAtMs = now + InfoSubmitDelayMs;
            LogToPanel("Up Zin: Đã điền thông tin và bấm OK.");
        }
        catch (Exception ex)
        {
            _nextInfoSubmitAtMs = now + InfoSubmitDelayMs;
            LogToPanel("Up Zin: Lỗi điền form thông tin - " + ex.Message);
        }
    }

    private void LogToPanel(string message)
    {
        if (string.IsNullOrEmpty(AutoLogin.idClientSocket))
            return;

        SocketGame.SendMessage($"LOG|{AutoLogin.idClientSocket}|{message}");
    }
}
