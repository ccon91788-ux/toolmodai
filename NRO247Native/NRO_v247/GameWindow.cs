using System;
using System.Runtime.InteropServices;
using SkiaSharp;
using System.Threading;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace NRO_v247;

public unsafe class GameWindow : IDisposable
{
    private const string CLASS_NAME = "GameWindowClass";
    private int _targetFPS;
    private int _frameTimeMs;

    private IntPtr _hwnd, _hdc, _memDC, _memBitmap, _oldBitmap;
    private readonly int _width, _height;
    private int _clientWidth, _clientHeight;
    private int _nonClientW, _nonClientH;
    private bool _running;

    private SKSurface _surface;
    private SKBitmap _bitmap;
    private readonly mGraphics _graphics;
    private readonly WndProcDelegate _wndProcDelegate;
    private static IntPtr _currentHwnd;

    private long _nextFrameTime;
    private bool _leftMouseDown;
    private int _mouseX, _mouseY;

    private BITMAPINFO _bmi;
    private MSG _msg;
    private bool _needsRender;
    private bool _inSizeMove;
    private long _lastSizingRenderAt;

    /* public static float GameSpeed = 2f;
    private float _updateAccumulator = 0f; */

    public static bool IsRenderingEnabled = true;
    public static SKBitmap CurrentBitmap { get; private set; }
    private int _hiddenFrameCount = 0;

    public GameWindow(int width, int height, string title)
    {
        _width = width;
        _height = height;
        _clientWidth = width;
        _clientHeight = height;
        _wndProcDelegate = WndProc;
        
        _targetFPS = Math.Max(15, Math.Min(60, AutoLogin.TargetFPS));
        _frameTimeMs = 1000 / _targetFPS;

        RegisterWindowClass();
        CreateWindowHandle(title);
        InitializeGraphics();

        ScaleGUI.WIDTH = width;
        ScaleGUI.HEIGHT = height;
        _graphics = new mGraphics();

        _bmi = new BITMAPINFO
        {
            biSize = 40,
            biWidth = width,
            biHeight = -height,
            biPlanes = 1,
            biBitCount = 32
        };

        var main = new Main();
        main.InitializeConsole(this);

        // Giảm GC overhead
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
    }

    private void RegisterWindowClass()
    {
        var wc = new WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<WNDCLASSEX>(),
            style = 0x0023, // CS_HREDRAW | CS_VREDRAW | CS_OWNDC
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = GetModuleHandle(null),
            hCursor = LoadCursor(IntPtr.Zero, 32512),
            hbrBackground = (IntPtr)6,
            lpszClassName = CLASS_NAME
        };

        if (RegisterClassEx(ref wc) == 0)
            throw new Exception("Failed to register window class");
    }

    private void CreateWindowHandle(string title)
    {
        const uint windowStyle = 0x00CF0000; // WS_OVERLAPPEDWINDOW (cho phep resize/maximize)

        var rect = new RECT { right = _width, bottom = _height };
        AdjustWindowRect(ref rect, windowStyle, false);

        int w = rect.right - rect.left;
        int h = rect.bottom - rect.top;
        _nonClientW = w - _width;
        _nonClientH = h - _height;
        int x = (GetSystemMetrics(0) - w) / 2;
        int y = (GetSystemMetrics(1) - h) / 2;

        _hwnd = CreateWindowEx(0, CLASS_NAME, title, windowStyle,

                    x, y, w, h, IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);

        _currentHwnd = _hwnd;

        if (_hwnd == IntPtr.Zero)
            throw new Exception("Failed to create window");

        ShowWindow(_hwnd, 5);
        UpdateWindow(_hwnd);
    }

    public static void SetTitle(string title)
    {
        if (_currentHwnd != IntPtr.Zero)
            SetWindowText(_currentHwnd, title);
    }

    private void InitializeGraphics()
    {
        _hdc = GetDC(_hwnd);
        _memDC = CreateCompatibleDC(_hdc);
        _memBitmap = CreateCompatibleBitmap(_hdc, _width, _height);
        _oldBitmap = SelectObject(_memDC, _memBitmap);

        var info = new SKImageInfo(_width, _height, SKColorType.Bgra8888, SKAlphaType.Premul);
        _bitmap = new SKBitmap(info);
        _surface = SKSurface.Create(info, _bitmap.GetPixels());
        CurrentBitmap = _bitmap;
    }

    private void UpdateClientSize(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;
        if (width == _clientWidth && height == _clientHeight)
            return;
        _clientWidth = width;
        _clientHeight = height;
        _needsRender = true;
    }

    private int ToGameX(int clientX)
    {
        if (_clientWidth <= 0)
            return clientX;
        int x = clientX * _width / _clientWidth;
        if (x < 0) return 0;
        if (x >= _width) return _width - 1;
        return x;
    }

    private int ToGameY(int clientY)
    {
        if (_clientHeight <= 0)
            return clientY;
        int y = clientY * _height / _clientHeight;
        if (y < 0) return 0;
        if (y >= _height) return _height - 1;
        return y;
    }

    public void Run()
    {
        _running = true;
        _nextFrameTime = Environment.TickCount64;

        while (_running)
        {
            long currentTime = Environment.TickCount64;
            
            // Xử lý messages
            bool hasMessage = false;
            while (PeekMessage(out _msg, IntPtr.Zero, 0, 0, 1))
            {
                hasMessage = true;
                if (_msg.message == 0x0012)
                {
                    _running = false;
                    break;
                }
                TranslateMessage(ref _msg);
                DispatchMessage(ref _msg);
            }

            if (!_running) break;

            // Chỉ update khi đến lúc
            if (currentTime >= _nextFrameTime)
            {
                Update();
                
                if (_needsRender)
                {
                    Render();
                    _needsRender = false;
                }
                
                _nextFrameTime = currentTime + _frameTimeMs;
            }
            else
            {
                // Sleep để giảm CPU
                long sleepTime = _nextFrameTime - currentTime;
                if (sleepTime > 2)
                    Thread.Sleep((int)(sleepTime - 1));
                else if (sleepTime > 0)
                    Thread.Sleep(1);
                else if (!hasMessage)
                    Thread.Yield(); // Không chặn thread
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Update()
    {
        try
        {
            Session_ME.update();
            Session_ME2.update();
            GameMidlet.gameCanvas?.update();
            Main.f = (Main.f + 1) & 7;
            _needsRender = true;
        }
        catch { }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Render()
    {
        try
        {
            _hiddenFrameCount++;
            if (_hiddenFrameCount >= _targetFPS) // Clean RAM every 1 second
            {
                _hiddenFrameCount = 0;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }
            }

            if (!IsRenderingEnabled)
            {
                return;
            }

            var canvas = _surface.Canvas;
            canvas.Clear(SKColors.Black);

            _graphics.SetGraphics(canvas);
            GameMidlet.gameCanvas?.paint(_graphics);
            _graphics.reset();

            // Không flush nếu không cần thiết
            canvas.Flush();
            PresentBackbuffer();
        }
        catch { }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PresentBackbuffer()
    {
        if (_bitmap == null || _memDC == IntPtr.Zero || _hdc == IntPtr.Zero)
            return;

        SetDIBitsToDevice(_memDC, 0, 0, _width, _height, 0, 0, 0,
            (uint)_height, _bitmap.GetPixels(), ref _bmi, 0);

        StretchBlt(_hdc, 0, 0, _clientWidth, _clientHeight, _memDC, 0, 0, _width, _height, 0x00CC0020);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderDuringResize()
    {
        long now = Environment.TickCount64;
        if (now - _lastSizingRenderAt < _frameTimeMs)
            return;
        _lastSizingRenderAt = now;

        try
        {
            Session_ME.update();
            Session_ME2.update();
            GameMidlet.gameCanvas?.update();
            Main.f = (Main.f + 1) & 7;

            if (!IsRenderingEnabled)
                return;

            var canvas = _surface.Canvas;
            canvas.Clear(SKColors.Black);

            _graphics.SetGraphics(canvas);
            GameMidlet.gameCanvas?.paint(_graphics);
            _graphics.reset();
            canvas.Flush();

            PresentBackbuffer();
        }
        catch { }
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case 0x0231: // WM_ENTERSIZEMOVE
                _inSizeMove = true;
                _lastSizingRenderAt = 0;
                return IntPtr.Zero;
            case 0x0232: // WM_EXITSIZEMOVE
                _inSizeMove = false;
                _needsRender = true;
                return IntPtr.Zero;
            case 0x0214: // WM_SIZING: giu ti le khung hinh khi keo resize
                {
                    const int WMSZ_LEFT = 1;
                    const int WMSZ_RIGHT = 2;
                    const int WMSZ_TOP = 3;
                    const int WMSZ_TOPLEFT = 4;
                    const int WMSZ_TOPRIGHT = 5;
                    const int WMSZ_BOTTOM = 6;
                    const int WMSZ_BOTTOMLEFT = 7;
                    const int WMSZ_BOTTOMRIGHT = 8;

                    RECT r = Marshal.PtrToStructure<RECT>(lParam);
                    int edge = (int)wParam;
                    double aspect = (double)_width / _height;

                    int outerW = Math.Max(1, r.right - r.left);
                    int outerH = Math.Max(1, r.bottom - r.top);
                    int clientW = Math.Max(1, outerW - _nonClientW);
                    int clientH = Math.Max(1, outerH - _nonClientH);

                    int targetClientW;
                    int targetClientH;
                    bool heightDriven = edge == WMSZ_TOP || edge == WMSZ_BOTTOM;

                    if (heightDriven)
                    {
                        targetClientH = clientH;
                        targetClientW = Math.Max(1, (int)Math.Round(targetClientH * aspect));
                    }
                    else
                    {
                        targetClientW = clientW;
                        targetClientH = Math.Max(1, (int)Math.Round(targetClientW / aspect));
                    }

                    int targetOuterW = targetClientW + _nonClientW;
                    int targetOuterH = targetClientH + _nonClientH;
                    _clientWidth = targetClientW;
                    _clientHeight = targetClientH;
                    _needsRender = true;
                    RenderDuringResize();

                    bool fromLeft = edge == WMSZ_LEFT || edge == WMSZ_TOPLEFT || edge == WMSZ_BOTTOMLEFT;
                    bool fromTop = edge == WMSZ_TOP || edge == WMSZ_TOPLEFT || edge == WMSZ_TOPRIGHT;

                    if (fromLeft) r.left = r.right - targetOuterW;
                    else r.right = r.left + targetOuterW;

                    if (fromTop) r.top = r.bottom - targetOuterH;
                    else r.bottom = r.top + targetOuterH;

                    Marshal.StructureToPtr(r, lParam, fDeleteOld: false);
                    return new IntPtr(1);
                }
            case 0x0014: // WM_ERASEBKGND
                // Chan Windows tu to nen trang khi dang resize.
                return new IntPtr(1);
            case 0x0002: // WM_DESTROY
                Main.exit();      
                break;
            case 0x0005: // WM_SIZE
                int newClientW = (int)lParam & 0xFFFF;
                int newClientH = ((int)lParam >> 16) & 0xFFFF;
                UpdateClientSize(newClientW, newClientH);
                return IntPtr.Zero;
            case 0x0201: // WM_LBUTTONDOWN
                _leftMouseDown = true;
                _mouseX = ToGameX((int)lParam & 0xFFFF);
                _mouseY = ToGameY(((int)lParam >> 16) & 0xFFFF);
                GameMidlet.gameCanvas?.pointerPressed(_mouseX, _mouseY);
                return IntPtr.Zero;

            case 0x0202: // WM_LBUTTONUP
                _leftMouseDown = false;
                _mouseX = ToGameX((int)lParam & 0xFFFF);
                _mouseY = ToGameY(((int)lParam >> 16) & 0xFFFF);
                GameMidlet.gameCanvas?.pointerReleased(_mouseX, _mouseY);
                return IntPtr.Zero;

            case 0x0200: // WM_MOUSEMOVE
                _mouseX = ToGameX((int)lParam & 0xFFFF);
                _mouseY = ToGameY(((int)lParam >> 16) & 0xFFFF);
                if (_leftMouseDown)
                    GameMidlet.gameCanvas?.pointerDragged(_mouseX, _mouseY);
                GameMidlet.gameCanvas?.pointerMouse(_mouseX, _mouseY);
                return IntPtr.Zero;

            case 0x020A: // WM_MOUSEWHEEL
                GameMidlet.gameCanvas?.scrollMouse((((int)wParam >> 16) & 0xFFFF) / 120);
                return IntPtr.Zero;

            case 0x0100: // WM_KEYDOWN
            case 0x0101: // WM_KEYUP
                int key = MyKeyMap.MapVirtualKey((int)wParam);
                if (key != 0)
                {
                    if (msg == 0x0100)
                        GameMidlet.gameCanvas?.keyPressedz(key);
                    else
                        GameMidlet.gameCanvas?.keyReleasedz(key);
                }
                return IntPtr.Zero;

            case 0x000F: // WM_PAINT
                BeginPaint(hWnd, out var ps);
                if (_inSizeMove)
                    RenderDuringResize();
                else
                    PresentBackbuffer();
                EndPaint(hWnd, ref ps);
                return IntPtr.Zero;
            case 0x0104: // WM_SYSKEYDOWN
            case 0x0105: // WM_SYSKEYUP
                if ((int)wParam == 13) // VK_RETURN
                    return IntPtr.Zero; // chặn ALT+ENTER
                break;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        _running = false;
        PostQuitMessage(0);

        _surface?.Dispose();
        _bitmap?.Dispose();

        if (_memDC != IntPtr.Zero)
        {
            SelectObject(_memDC, _oldBitmap);
            DeleteObject(_memBitmap);
            DeleteDC(_memDC);
        }

        if (_hdc != IntPtr.Zero)
            ReleaseDC(_hwnd, _hdc);

        if (_hwnd != IntPtr.Zero)
            DestroyWindow(_hwnd);

        GC.SuppressFinalize(this);
    }

    // Win32 API
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")] private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);
    [DllImport("user32.dll")] private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool UpdateWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern void PostQuitMessage(int nExitCode);
    [DllImport("user32.dll")] private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
    [DllImport("user32.dll")] private static extern bool TranslateMessage(ref MSG lpMsg);
    [DllImport("user32.dll")] private static extern IntPtr DispatchMessage(ref MSG lpMsg);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("user32.dll")] private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
    [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);
    [DllImport("user32.dll")] private static extern bool AdjustWindowRect(ref RECT lpRect, uint dwStyle, bool bMenu);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern bool SetWindowText(IntPtr hWnd, string text);
    [DllImport("user32.dll")] private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);
    [DllImport("user32.dll")] private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);
    [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
    [DllImport("gdi32.dll")] private static extern bool StretchBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidthDest, int nHeightDest, IntPtr hdcSrc, int nXSrc, int nYSrc, int nWidthSrc, int nHeightSrc, uint dwRop);
    [DllImport("gdi32.dll")] private static extern int SetDIBitsToDevice(IntPtr hdc, int xDest, int yDest, int dwWidth, int dwHeight, int xSrc, int ySrc, uint uStartScan, uint cScanLines, IntPtr lpvBits, ref BITMAPINFO lpbmi, uint fuColorUse);
    [DllImport("kernel32.dll")] private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

    [StructLayout(LayoutKind.Sequential)]
    private struct WNDCLASSEX
    {
        public int cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int x, y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
}
