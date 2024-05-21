using System;
using System.Numerics;
using Flux.Hotkeys.Util;

namespace Flux.Hotkeys.Actions;

[PublicAPI]
public class ClickAction : IInputAction
{
    private ClickAction(Key key, bool autoRelease, bool inPressMode, TimeSpan? duration, InputDirection direction, Vector2 coordinates)
    {
        Key = key;
        AutoRelease = autoRelease;
        InPressMode = inPressMode;
        Duration = duration ?? TimeSpan.Zero;
        Direction = direction;
        Coordinates = coordinates;
    }
    
    public string Build()
    {
        if (!Key.IsMouse())
        {
            return "";
        }
        
        if (InPressMode)
        {
            return AhkFmt.Click(Key, InputDirection.Both);
        }

        if (Duration != TimeSpan.Zero)
        {
            return AhkFmt.Actions(0,
                Ahk.Snippet(AhkFmt.Click(Key, InputDirection.Down)),
                Ahk.Sleep(Duration),
                Ahk.Snippet(AhkFmt.Click(Key, InputDirection.Up))    
            );
        }

        return AhkFmt.Click(Key, Direction);
    }

    public static ClickAction Down(Key key, TimeSpan? duration = null, bool autoRelease = true, Vector2 coordinates = default)
    {
        return new ClickAction(key, autoRelease, false, duration, InputDirection.Down, coordinates);
    }
    
    public static ClickAction Up(Key key, Vector2 coordinates = default)
    {
        return new ClickAction(key, false, false, null, InputDirection.Up, coordinates);
    }

    public static ClickAction Press(Key key, Vector2 coordinates = default)
    {
        return new ClickAction(key, true, true, null, InputDirection.Down, coordinates);
    }

    public Key Key { get; }
    public bool IsDown => Direction is InputDirection.Down;
    public bool IsUp => Direction is InputDirection.Up;
    public bool AutoRelease { get; }
    public bool InPressMode { get; }
    public bool InMultiMode => false;
    public TimeSpan Duration { get; }
    public InputDirection Direction { get; }
    public Vector2 Coordinates { get; }
}