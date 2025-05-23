﻿namespace Smithbox.Core.Interface.Input
{
    /// <summary>
    /// Enumeration for virtual keys.
    /// </summary>
    public enum Key
    {
        Unknown = 0,
        Return = 13,
        Escape = 27,
        Backspace = 8,
        Tab = 9,
        Space = 0x20,
        Exclaim = 33,
        Quotedbl = 34,
        Hash = 35,
        Percent = 37,
        Dollar = 36,
        Ampersand = 38,
        Quote = 39,
        Leftparen = 40,
        Rightparen = 41,
        Asterisk = 42,
        Plus = 43,
        Comma = 44,
        Minus = 45,
        Period = 46,
        Slash = 47,
        N0 = 48,
        N1 = 49,
        N2 = 50,
        N3 = 51,
        N4 = 52,
        N5 = 53,
        N6 = 54,
        N7 = 55,
        N8 = 56,
        N9 = 57,
        Colon = 58,
        Semicolon = 59,
        Less = 60,
        Equals = 61,
        Greater = 62,
        Question = 0x3F,
        AT = 0x40,
        Leftbracket = 91,
        Backslash = 92,
        Rightbracket = 93,
        Caret = 94,
        Underscore = 95,
        Backquote = 96,
        A = 97,
        B = 98,
        C = 99,
        D = 100,
        E = 101,
        F = 102,
        G = 103,
        H = 104,
        I = 105,
        J = 106,
        K = 107,
        L = 108,
        M = 109,
        N = 110,
        O = 111,
        P = 112,
        Q = 113,
        R = 114,
        S = 115,
        T = 116,
        U = 117,
        V = 118,
        W = 119,
        X = 120,
        Y = 121,
        Z = 122,
        Capslock = 1073741881,
        F1 = 1073741882,
        F2 = 1073741883,
        F3 = 1073741884,
        F4 = 1073741885,
        F5 = 1073741886,
        F6 = 1073741887,
        F7 = 1073741888,
        F8 = 1073741889,
        F9 = 1073741890,
        F10 = 1073741891,
        F11 = 1073741892,
        F12 = 1073741893,
        Printscreen = 1073741894,
        Scrolllock = 1073741895,
        Pause = 1073741896,
        Insert = 1073741897,
        Home = 1073741898,
        Pageup = 1073741899,
        Delete = 0x7F,
        End = 1073741901,
        Pagedown = 1073741902,
        Right = 1073741903,
        Left = 1073741904,
        Down = 1073741905,
        Up = 1073741906,
        Numlockclear = 1073741907,
        NumDivide = 1073741908,
        NumMultiply = 1073741909,
        NumMinus = 1073741910,
        NumPlus = 1073741911,
        NumEnter = 1073741912,
        Num1 = 1073741913,
        Num2 = 1073741914,
        Num3 = 1073741915,
        Num4 = 1073741916,
        Num5 = 1073741917,
        Num6 = 1073741918,
        Num7 = 1073741919,
        Num8 = 1073741920,
        Num9 = 1073741921,
        Num0 = 1073741922,
        NumPeriod = 1073741923,
        Application = 1073741925,
        Power = 1073741926,
        NumEquals = 1073741927,
        F13 = 1073741928,
        F14 = 1073741929,
        F15 = 1073741930,
        F16 = 1073741931,
        F17 = 1073741932,
        F18 = 1073741933,
        F19 = 1073741934,
        F20 = 1073741935,
        F21 = 1073741936,
        F22 = 1073741937,
        F23 = 1073741938,
        F24 = 1073741939,
        Execute = 1073741940,
        Help = 1073741941,
        Menu = 1073741942,
        Select = 1073741943,
        Stop = 1073741944,
        Again = 1073741945,
        Undo = 1073741946,
        Cut = 1073741947,
        Copy = 1073741948,
        Paste = 1073741949,
        Find = 1073741950,
        Mute = 1073741951,
        Volumeup = 1073741952,
        Volumedown = 1073741953,
        NumComma = 1073741957,
        NumEqualsas400 = 1073741958,
        Alterase = 1073741977,
        Sysreq = 1073741978,
        Cancel = 1073741979,
        Clear = 1073741980,
        Prior = 1073741981,
        Return2 = 1073741982,
        Separator = 1073741983,
        Out = 1073741984,
        Oper = 1073741985,
        Clearagain = 1073741986,
        Crsel = 1073741987,
        Exsel = 1073741988,
        Num00 = 1073742000,
        Num000 = 1073742001,
        Thousandsseparator = 1073742002,
        Decimalseparator = 1073742003,
        Currencyunit = 1073742004,
        Currencysubunit = 1073742005,
        NumLeftparen = 1073742006,
        NumRightparen = 1073742007,
        NumLeftbrace = 1073742008,
        NumRightbrace = 1073742009,
        NumTab = 1073742010,
        NumBackspace = 1073742011,
        NumA = 1073742012,
        NumB = 1073742013,
        NumC = 1073742014,
        NumD = 1073742015,
        NumE = 1073742016,
        NumF = 1073742017,
        NumXor = 1073742018,
        NumPower = 1073742019,
        NumPercent = 1073742020,
        NumLess = 1073742021,
        NumGreater = 1073742022,
        NumAmpersand = 1073742023,
        NumDblampersand = 1073742024,
        NumVerticalbar = 1073742025,
        NumDblverticalbar = 1073742026,
        NumColon = 1073742027,
        NumHash = 1073742028,
        NumSpace = 1073742029,
        NumAT = 1073742030,
        NumExclam = 1073742031,
        NumMemstore = 1073742032,
        NumMemrecall = 1073742033,
        NumMemclear = 1073742034,
        NumMemadd = 1073742035,
        NumMemsubtract = 1073742036,
        NumMemmultiply = 1073742037,
        NumMemdivide = 1073742038,
        NumPlusminus = 1073742039,
        NumClear = 1073742040,
        NumClearentry = 1073742041,
        NumBinary = 1073742042,
        NumOctal = 1073742043,
        NumDecimal = 1073742044,
        NumHexadecimal = 1073742045,
        LCtrl = 1073742048,
        LShift = 1073742049,
        LAlt = 1073742050,
        LGui = 1073742051,
        RCtrl = 1073742052,
        RShift = 1073742053,
        RAlt = 1073742054,
        RGui = 1073742055,
        Mode = 1073742081,
        AudioNext = 1073742082,
        AudioPrev = 1073742083,
        AudioStop = 1073742084,
        AudioPlay = 1073742085,
        AudioMute = 1073742086,
        MediaSelect = 1073742087,
        WWW = 1073742088,
        Mail = 1073742089,
        Calculator = 1073742090,
        Computer = 1073742091,
        ACSearch = 1073742092,
        ACHome = 1073742093,
        ACBack = 1073742094,
        ACForward = 1073742095,
        ACStop = 1073742096,
        ACRefresh = 1073742097,
        ACBookmarks = 1073742098,
        BrightnessDown = 1073742099,
        BrightnessUp = 1073742100,
        DisplaySwitch = 1073742101,
        KeyboardIlluminationToggle = 1073742102,
        KeyboardIlluminationDown = 1073742103,
        KeyboardIlluminationUp = 1073742104,
        Eject = 1073742105,
        Sleep = 1073742106,
        App1 = 1073742107,
        App2 = 1073742108,
        AudioRewind = 1073742109,
        AudioFastForward = 1073742110
    }
}