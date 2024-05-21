using System;
using System.Numerics;
using Flux.Hotkeys.Actions;

namespace Flux.Hotkeys;

public static class Mouse
{
    public static ClickAction Down(Key key, TimeSpan? duration = null, bool autoRelease = true, Vector2 coordinates = default)
    {
        return ClickAction.Down(key, duration, autoRelease, coordinates);
    }

    public static ClickAction Up(Key key, Vector2 coordinates = default)
    {
        return ClickAction.Up(key, coordinates);
    }
    
    public static ClickAction Press(Key key, Vector2 coordinates = default)
    {
        return ClickAction.Press(key, coordinates);
    }
}