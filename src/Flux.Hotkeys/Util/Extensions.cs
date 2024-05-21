using System;
using System.Linq;
using System.Collections.Generic;
using Flux.Hotkeys.Actions;

namespace Flux.Hotkeys.Util;

public static class Extensions
{
    public static IEnumerable<IAction> ToActions(this IEnumerable<string> actions)
    {
        return actions.Select(Ahk.Snippet).Cast<IAction>().ToArray();
    }
    public static string Indent(this string str, int indent)
    {
        return $"{string.Join(" ", Enumerable.Repeat(" ", indent))}{str}";
    }
    
    public static TimeSpan Minutes(this int duration) => TimeSpan.FromMinutes(duration);
    public static TimeSpan Minutes(this float duration) => TimeSpan.FromMinutes(duration);
    public static TimeSpan Minutes(this double duration) => TimeSpan.FromMinutes(duration);

    public static TimeSpan Seconds(this int duration) => TimeSpan.FromSeconds(duration);
    public static TimeSpan Seconds(this float duration) => TimeSpan.FromSeconds(duration);
    public static TimeSpan Seconds(this double duration) => TimeSpan.FromSeconds(duration);

    public static TimeSpan Milliseconds(this int duration) => TimeSpan.FromMilliseconds(duration);
    public static TimeSpan Milliseconds(this float duration) => TimeSpan.FromMilliseconds(duration);
    public static TimeSpan Milliseconds(this double duration) => TimeSpan.FromMilliseconds(duration);
    
    public static string Quote(this string str)
    {
        return Wrap(str, '"'.ToString(), '"'.ToString());
    }
    
    public static string Wrap(this string str, string left = "{", string right = "}")
    {
        return $"{left}{str}{right}";
    }
    
    public static bool IsValid(this InputDirection direction)
    {
        return direction is InputDirection.Down or InputDirection.Up;
    }
    
    public static string ToAhkLabel(this InputDirection direction)
    {
        return direction is InputDirection.Both ? "" : direction.ToString("G");
    }
    
    private static readonly Lazy<Dictionary<Key, string>> s_ahkLabelMap = new Lazy<Dictionary<Key, string>>(new Dictionary<Key, string>
    {
        {Key.None, ""},

        {Key.LeftButton, "LButton"},
        {Key.RightButton, "RButton"},
        {Key.MiddleButton, "MButton"},
        
        {Key.XButton1, "XButton1"},
        {Key.XButton2, "XButton2"},
        
        {Key.WheelDown, "WheelDown"}, 
        {Key.WheelUp, "WheelUp"},
        {Key.WheelLeft, "WheelLeft"}, 
        {Key.WheelRight, "WheelRight"}, 
        
        
        {Key.Num0, "0"},
        {Key.Num1, "1"},
        {Key.Num2, "2"},
        {Key.Num3, "3"},
        {Key.Num4, "4"},
        {Key.Num5, "5"},
        {Key.Num6, "6"},
        {Key.Num7, "7"},
        {Key.Num8, "8"},
        {Key.Num9, "9"},
        
        {Key.A, "a"},
        {Key.B, "b"},
        {Key.C, "c"},
        {Key.D, "d"},
        {Key.E, "e"},
        {Key.F, "f"},
        {Key.G, "g"},
        {Key.H, "h"},
        {Key.I, "i"},
        {Key.J, "j"},
        {Key.K, "k"},
        {Key.L, "l"},
        {Key.M, "m"},
        {Key.N, "n"},
        {Key.O, "o"},
        {Key.P, "p"},
        {Key.Q, "q"},
        {Key.R, "r"},
        {Key.S, "s"},
        {Key.T, "t"},
        {Key.U, "u"},
        {Key.V, "v"},
        {Key.W, "w"},
        {Key.X, "x"},
        {Key.Y, "y"},
        {Key.Z, "z"},
        
        
        {Key.Cancel, "Cancel"},
        {Key.Backspace, "Back"},
        {Key.Tab, "Tab"},
        {Key.Clear, "Clear"},
        {Key.Enter, "Enter"},
        {Key.Shift, "Shift"},
        {Key.Control, "Control"},
        {Key.Alt, "Alt"},
        {Key.Pause, "Pause"},
        {Key.CapsLock, "CapsLock"},
        {Key.Escape, "Escape"},
        
        
        {Key.Space, "Space"},
        {Key.PageUp, "PgUp"},
        {Key.PageDown, "PgDown"},
        {Key.End, "End"},
        {Key.Home, "Home"},
        {Key.LeftArrow, "Left"},
        {Key.UpArrow, "Up"},
        {Key.RightArrow, "Right"},
        {Key.DownArrow, "Down"},
        
        {Key.PrintScreen, "PrtSc"},
        
        {Key.Insert, "Insert"},
        {Key.Delete, "Delete"},
        {Key.Help, "Help"},
        {Key.LeftWindows, "LWin"},
        {Key.RightWindows, "RWin"},
        {Key.Apps, "Apps"},
        {Key.Sleep, "Sleep"},
        
        {Key.Numpad0, "Numpad0"},
        {Key.Numpad1, "Numpad1"},
        {Key.Numpad2, "Numpad2"},
        {Key.Numpad3, "Numpad3"},
        {Key.Numpad4, "Numpad4"},
        {Key.Numpad5, "Numpad5"},
        {Key.Numpad6, "Numpad6"},
        {Key.Numpad7, "Numpad7"},
        {Key.Numpad8, "Numpad8"},
        {Key.Numpad9, "Numpad9"},
        {Key.NumpadMultiply, "NumpadMult"},
        {Key.NumpadAdd, "NumpadAdd"},
        {Key.NumpadSubtract, "NumpadSub"},
        {Key.NumpadDot, "NumpadDot"},
        {Key.NumpadDivide, "NumpadDiv"},
        {Key.NumpadEnter, "NumpadEnter"},
        
        {Key.F1, "F1"},
        {Key.F2, "F2"},
        {Key.F3, "F3"},
        {Key.F4, "F4"},
        {Key.F5, "F5"},
        {Key.F6, "F6"},
        {Key.F7, "F7"},
        {Key.F8, "F8"},
        {Key.F9, "F9"},
        {Key.F10, "F10"},
        {Key.F11, "F11"},
        {Key.F12, "F12"},
        {Key.F13, "F13"},
        {Key.F14, "F14"},
        {Key.F15, "F15"},
        {Key.F16, "F16"},
        {Key.F17, "F17"},
        {Key.F18, "F18"},
        {Key.F19, "F19"},
        {Key.F20, "F20"},
        {Key.F21, "F21"},
        {Key.F22, "F22"},
        {Key.F23, "F23"},
        {Key.F24, "F24"},
        
        {Key.NumLock, "NumLock"},
        {Key.ScrollLock, "ScrollLock"},
        
        {Key.LeftShift, "LShift"},
        {Key.RightShift, "RShift"},
        {Key.LeftControl, "LControl"},
        {Key.RightControl, "RControl"},
        {Key.LeftAlt, "LAlt"},
        {Key.RightAlt, "RAlt"},
        
        {Key.BrowserBack, "Browser_Back"},
        {Key.BrowserForward, "Browser_Forward"},
        {Key.BrowserRefresh, "Browser_Refresh"},
        {Key.BrowserStop, "Browser_Stop"},
        {Key.BrowserSearch, "Browser_Search"},
        {Key.BrowserFavorites, "Browser_Favorites"},
        {Key.BrowserHome, "Browser_Home"},
        {Key.VolumeMute, "Volume_Mute"},
        {Key.VolumeDown, "Volume_Down"},
        {Key.VolumeUp, "Volume_Up"},
        {Key.MediaNext, "Media_Next"},
        {Key.MediaPrev, "Media_Prev"},
        {Key.MediaStop, "Media_Stop"},
        {Key.MediaPlayPause, "Media_Play_Pause"},
        {Key.LaunchMail, "Launch_Mail"},
        {Key.LaunchMediaSelect, "Launch_Media_Select"},
        {Key.LaunchApp1, "Launch_App1"},
        {Key.LaunchApp2, "Launch_App2"},
    });
    
    private static readonly Lazy<Dictionary<Key, string>> s_ahkMouseLabelMap = new Lazy<Dictionary<Key, string>>(new Dictionary<Key, string>
    {
        {Key.LeftButton, "Left"},
        {Key.RightButton, "Right"},
        {Key.MiddleButton, "Middle"},
        {Key.XButton1, "X1"},
        {Key.XButton2, "X2"},
    });
    
    private static readonly Lazy<Dictionary<Key, string>> s_ahkModifierLabelMap = new Lazy<Dictionary<Key, string>>(new Dictionary<Key, string>
    {
        {Key.Shift, "Shift"},
        {Key.Control, "Control"},
        {Key.Alt, "Alt"},
        {Key.Windows, "LWin"},
        {Key.LeftWindows, "LWin"},
        {Key.RightWindows, "RWin"},
        
        {Key.LeftShift, "LShift"},
        {Key.RightShift, "RShift"},
        {Key.LeftControl, "LControl"},
        {Key.RightControl, "RControl"},
        {Key.LeftAlt, "LAlt"},
        {Key.RightAlt, "RAlt"},
    });

    private static readonly Lazy<Dictionary<Key, string>> s_ahkModifierSymbolMap = new Lazy<Dictionary<Key, string>>(new Dictionary<Key, string>
    {
        {Key.Shift, "+"},
        {Key.Control, "^"},
        {Key.Alt, "!"},
        {Key.Windows, "#"},
        
        {Key.LeftWindows, "<#"},
        {Key.RightWindows, ">#"},
        
        {Key.LeftShift, "<+"},
        {Key.RightShift, ">+"},
        {Key.LeftControl, "<^"},
        {Key.RightControl, ">^"},
        {Key.LeftAlt, "<!"},
        {Key.RightAlt, ">!"},
    });
    
    public static bool TryGetAhkLabel(this Key key, out string label)
    {
        var result = s_ahkLabelMap.Value.TryGetValue(key, out var l);
        label = l ?? "";
        return result;
    }

    public static bool TryGetMouseClickLabel(this Key key, out string label)
    {
        var result = s_ahkMouseLabelMap.Value.TryGetValue(key, out var l);
        label = l ?? "";
        return result;
    }
    
    public static bool TryGetModifierSymbol(this Key key, out string symbol)
    {
        var result = s_ahkModifierSymbolMap.Value.TryGetValue(key, out var s);
        symbol = s ?? "";
        return result;
    }

    public static bool IsModifier(this Key key)
    {
        return s_ahkModifierLabelMap.Value.ContainsKey(key);
    }

    public static bool IsKeyboard(this Key key)
    {
        return s_ahkLabelMap.Value.ContainsKey(key) && !s_ahkMouseLabelMap.Value.ContainsKey(key);
    }

    public static bool IsMouse(this Key key)
    {
        return s_ahkMouseLabelMap.Value.ContainsKey(key);
    }
}