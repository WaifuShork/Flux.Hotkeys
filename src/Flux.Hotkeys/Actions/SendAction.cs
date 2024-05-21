using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using Flux.Hotkeys.Util;

namespace Flux.Hotkeys.Actions;

[PublicAPI]
public class SendAction : IInputAction
{
    private SendAction(Key key, bool autoRelease, bool inPressMode, TimeSpan? duration, InputDirection direction)
    {
        Key = key;
        AutoRelease = autoRelease;
        InPressMode = inPressMode;
        Duration = duration ?? TimeSpan.Zero;
        Direction = direction;
    }
    
    private SendAction(IEnumerable<Key> keys, bool autoRelease, bool inPressMode, bool inMultiMode, TimeSpan? duration, InputDirection direction)
    {
        Key = Key.None;
        Keys = keys.ToList();
        AutoRelease = autoRelease;
        InPressMode = inPressMode;
        InMultiMode = inMultiMode;
        Duration = duration ?? TimeSpan.Zero;
        Direction = direction;
    }
    
    public string Build()
    {
        if (InMultiMode)
        {
            var keys = Keys;
            if (keys is null || keys.Count == 0)
            {
                return "";
            }

            var builder = ZString.CreateStringBuilder();
            builder.Append("Send, ");
            foreach (var key in keys)
            {
                if (key.TryGetAhkLabel(out var label))
                {
                    builder.Append($"{label}".Wrap());
                }
            }
            
            builder.AppendLine();
            return builder.ToString();
        }
        
        if (InPressMode)
        {
            return AhkFmt.Send(Key, InputDirection.Both);
        }

        if (Duration != TimeSpan.Zero)
        {
            return AhkFmt.Actions(0,
                Ahk.Snippet(AhkFmt.Send(Key, InputDirection.Down)),
                Ahk.Sleep(Duration),
                Ahk.Snippet(AhkFmt.Send(Key, InputDirection.Up))    
            );
        }

        return AhkFmt.Send(Key, Direction);
    }

    public static SendAction Down(Key key, TimeSpan? duration = null, bool autoRelease = true)
    {
        return new SendAction(key, autoRelease, false, duration, InputDirection.Down);
    }
    
    public static SendAction Up(Key key)
    {
        return new SendAction(key, false, false, null, InputDirection.Up);
    }

    public static SendAction Press(Key key)
    {
        return new SendAction(key, true, true, null, InputDirection.Down);
    }

    public static SendAction Multi(params Key[] keys)
    {
        return new SendAction(keys, false, false, true, null, InputDirection.Both);
    }
    
    public Key Key { get; private set; }
    public List<Key>? Keys { get; private set; }
    public bool IsDown => Direction is InputDirection.Down;
    public bool IsUp => Direction is InputDirection.Up;
    public bool AutoRelease { get; private set; }
    public bool InPressMode { get; private set; }
    public bool InMultiMode { get; private set; }
    public TimeSpan Duration { get; private set; }
    public InputDirection Direction { get; private set; }
}