using System;
using Flux.Hotkeys.Actions;

namespace Flux.Hotkeys;

public static class Keyboard
{
    public static SendAction Send(params Key[] keys)
    {
        return SendAction.Multi(keys);
    }
    
    public static SendAction Down(Key key, TimeSpan? duration = null, bool autoRelease = true)
    {
        return SendAction.Down(key, duration, autoRelease);
    }

    public static SendAction Up(Key key)
    {
        return SendAction.Up(key);
    }
    
    public static SendAction Press(Key key)
    {
        return SendAction.Press(key);
    }
}