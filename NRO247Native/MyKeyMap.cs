using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class MyKeyMap
{
    private static readonly Dictionary<int, int> _keyMap;
    private static readonly Dictionary<int, int> _shiftKeyMap;

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    static MyKeyMap()
    {
        _keyMap = new Dictionary<int, int>(64)
        {
            // A-Z keys (VK_A = 0x41 to VK_Z = 0x5A)
            { 0x41, 97 },  // a
            { 0x42, 98 },  // b
            { 0x43, 99 },  // c
            { 0x44, 100 }, // d
            { 0x45, 101 }, // e
            { 0x46, 102 }, // f
            { 0x47, 103 }, // g
            { 0x48, 104 }, // h
            { 0x49, 105 }, // i
            { 0x4A, 106 }, // j
            { 0x4B, 107 }, // k
            { 0x4C, 108 }, // l
            { 0x4D, 109 }, // m
            { 0x4E, 110 }, // n
            { 0x4F, 111 }, // o
            { 0x50, 112 }, // p
            { 0x51, 113 }, // q
            { 0x52, 114 }, // r
            { 0x53, 115 }, // s
            { 0x54, 116 }, // t
            { 0x55, 117 }, // u
            { 0x56, 118 }, // v
            { 0x57, 119 }, // w
            { 0x58, 120 }, // x
            { 0x59, 121 }, // y
            { 0x5A, 122 }, // z
            // Number keys
            { 0x30, 48 },  // 0
            { 0x31, 49 },  // 1
            { 0x32, 50 },  // 2
            { 0x33, 51 },  // 3
            { 0x34, 52 },  // 4
            { 0x35, 53 },  // 5
            { 0x36, 54 },  // 6
            { 0x37, 55 },  // 7
            { 0x38, 56 },  // 8
            { 0x39, 57 },  // 9
            // Special keys
            { 0x20, 32 },   // SPACE
            { 0x70, -21 },  // F1
            { 0x71, -22 },  // F2
            { 0x72, -23 },  // F3
            { 0xBB, 61 },   // = (OEM_PLUS)
            { 0xBD, 45 },   // - (OEM_MINUS)
            { 0x26, -1 },   // UP
            { 0x28, -2 },   // DOWN
            { 0x25, -3 },   // LEFT
            { 0x27, -4 },   // RIGHT
            { 0x08, -8 },   // BACKSPACE
            { 0x0D, -5 },   // ENTER
            { 0xBE, 46 },   // . (OEM_PERIOD)
            { 0x09, -26 },  // TAB
            { 0xBF, 47 },   // / (OEM_2)
            { 0xDE, 39 },   // ' (OEM_7)
            { 0xBC, 44 },   // , (OEM_COMMA)
            { 0xC0, 96 },   // ` (OEM_3)
            { 0xDB, 91 },   // [ (OEM_4)
            { 0xDD, 93 },   // ] (OEM_6)
            { 0xDC, 92 },   // \ (OEM_5)
            { 0xBA, 59 },   // ; (OEM_1)
        };

        // Map khi nhấn Shift
        _shiftKeyMap = new Dictionary<int, int>(64)
        {
            // A-Z keys (chữ hoa)
            { 0x41, 65 },  // A
            { 0x42, 66 },  // B
            { 0x43, 67 },  // C
            { 0x44, 68 },  // D
            { 0x45, 69 },  // E
            { 0x46, 70 },  // F
            { 0x47, 71 },  // G
            { 0x48, 72 },  // H
            { 0x49, 73 },  // I
            { 0x4A, 74 },  // J
            { 0x4B, 75 },  // K
            { 0x4C, 76 },  // L
            { 0x4D, 77 },  // M
            { 0x4E, 78 },  // N
            { 0x4F, 79 },  // O
            { 0x50, 80 },  // P
            { 0x51, 81 },  // Q
            { 0x52, 82 },  // R
            { 0x53, 83 },  // S
            { 0x54, 84 },  // T
            { 0x55, 85 },  // U
            { 0x56, 86 },  // V
            { 0x57, 87 },  // W
            { 0x58, 88 },  // X
            { 0x59, 89 },  // Y
            { 0x5A, 90 },  // Z
            // Number keys với Shift (ký tự đặc biệt)
            { 0x30, 41 },  // ) - Shift + 0
            { 0x31, 33 },  // ! - Shift + 1
            { 0x32, 64 },  // @ - Shift + 2
            { 0x33, 35 },  // # - Shift + 3
            { 0x34, 36 },  // $ - Shift + 4
            { 0x35, 37 },  // % - Shift + 5
            { 0x36, 94 },  // ^ - Shift + 6
            { 0x37, 38 },  // & - Shift + 7
            { 0x38, 42 },  // * - Shift + 8
            { 0x39, 40 },  // ( - Shift + 9
            // Special keys với Shift
            { 0xBB, 43 },   // + (Shift + =)
            { 0xBD, 95 },   // _ (Shift + -)
            { 0xBE, 62 },   // > (Shift + .)
            { 0xBF, 63 },   // ? (Shift + /)
            { 0xDE, 34 },   // " (Shift + ')
            { 0xBC, 60 },   // < (Shift + ,)
            { 0xC0, 126 },  // ~ (Shift + `)
            { 0xDB, 123 },  // { (Shift + [)
            { 0xDD, 125 },  // } (Shift + ])
            { 0xDC, 124 },  // | (Shift + \)
            { 0xBA, 58 },   // : (Shift + ;)
        };
    }

    // Kiểm tra xem Shift có đang được nhấn không
    private static bool IsShiftPressed()
    {
        // VK_SHIFT = 0x10
        return (GetKeyState(0x10) & 0x8000) != 0;
    }

    public static int MapVirtualKey(int vkCode)
    {
        bool shiftPressed = IsShiftPressed();

        // Ưu tiên map với Shift nếu Shift đang được nhấn
        if (shiftPressed && _shiftKeyMap.TryGetValue(vkCode, out int shiftValue))
        {
            return shiftValue;
        }

        // Nếu không có Shift hoặc không tìm thấy trong shift map
        return _keyMap.TryGetValue(vkCode, out int value) ? value : 0;
    }

    // Alias cho compatibility
    public static int map(int vkCode) => MapVirtualKey(vkCode);
}