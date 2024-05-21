using System;
using System.Linq;
using Cysharp.Text;
using System.Collections.Generic;
using Flux.Hotkeys.Util;
using Flux.Hotkeys.Util.Exceptions;

namespace Flux.Hotkeys.Actions;

public delegate string HotkeyCallback(string hotkey);

public enum CallbackLocation
{
    Start,
    End,
}

[PublicAPI]
public class HotkeyAction : IDefAction
{
    public List<IAction> AhkActions { get; private set; } = [];
    public List<Key> Modifiers { get; private set; } = [];
    public (Key Left, Key Right)? KeyPair { get; private set; }
    public SendMode SendMode { get; set; }

    public Key Key { get; private set; }
    public bool IsBlocking { get; private set; }
    public InputDirection Direction { get; private set; } = InputDirection.Down;
    public HotkeyCallback? Callback { get; private set; }
    public CallbackLocation CallbackLocation { get; private set; } = CallbackLocation.Start;
    public string? PipeVariableName { get; private set; }
    
    public HotkeyAction Combine((Key left, Key right) keyPair, IEnumerable<Key>? modifiers = default, bool isBlocking = false, SendMode sendMode = SendMode.Input)
    {
        KeyPair = keyPair;
        Modifiers = modifiers?.ToList() ?? [];
        IsBlocking = isBlocking;
        SendMode = sendMode;
        Key = Key.None;
        return this;
    }
    
    public HotkeyAction Normal(Key key, IEnumerable<Key>? modifiers = default, bool isBlocking = false, SendMode sendMode = SendMode.Input)
    {
        Key = key;
        Modifiers = modifiers?.ToList() ?? [];
        IsBlocking = isBlocking;
        SendMode = sendMode;
        KeyPair = null;
        return this;
    }

    public HotkeyAction Block(bool block = true)
    {
        IsBlocking = block;
        return this;
    }

    public HotkeyAction OnDown(HotkeyCallback? callback = null, CallbackLocation location = CallbackLocation.Start)
    {
        Direction = InputDirection.Down;
        Callback = callback;
        CallbackLocation = location;
        return this;
    }
    
    public HotkeyAction OnUp(HotkeyCallback? callback = null, CallbackLocation location = CallbackLocation.Start)
    {
        Direction = InputDirection.Up;
        Callback = callback;
        CallbackLocation = location;
        return this;
    }
    
    public HotkeyAction OnUp(string pipeVariableName, HotkeyCallback? callback = null, CallbackLocation location = CallbackLocation.Start)
    {
        if (string.IsNullOrWhiteSpace(pipeVariableName))
        {
            throw new AhkException("Pipe variable name cannot be null or whitespace");
        }

        PipeVariableName = pipeVariableName;
        Direction = InputDirection.Up;
        Callback = callback;
        CallbackLocation = location;
        return this;
    }

    
    public HotkeyAction Action(IAction action)
    {
        AhkActions.Add(action);
        return this;
    }
    
    public HotkeyAction Action(string actions)
    {
        AhkActions.Add(Ahk.Snippet(actions));
        return this;
    }
    
    public HotkeyAction Actions(params IAction[] actions)
    {
        AhkActions.AddRange(actions);
        return this;
    }
    
    public HotkeyAction Action(params string[] actions)
    {
        foreach (var action in actions)
        {
            AhkActions.Add(Ahk.Snippet(action));
        }
        return this;
    }
    
    public string Build()
    {
        var buffer = ZString.CreateStringBuilder();
        if (!IsBlocking)
        {
            buffer.Append("~");
        }

        var modifiers = Modifiers.Distinct().Where(m => m.IsModifier());
        var keyPair = KeyPair;
        var hotkeyName = "";
        if (keyPair is not null)
        {
            var left = keyPair.Value.Left;
            var right = keyPair.Value.Right;

            
            modifiers = modifiers.Where(m => m != left && m != right);
            foreach (var modifier in modifiers)
            {
                if (modifier.TryGetModifierSymbol(out var l))
                {
                    buffer.Append(l);
                }
            }

            if (!left.TryGetAhkLabel(out var leftLabel) || !right.TryGetAhkLabel(out var rightLabel))
            {
                return "";
            }

            hotkeyName = $"{leftLabel} & {rightLabel}{(Direction is InputDirection.Up ? " UP" : "")}";

            buffer.AppendLine($"{hotkeyName}::");
        }
        else
        {
            foreach (var modifier in modifiers)
            {
                if (modifier.TryGetModifierSymbol(out var l))
                {
                    buffer.Append(l);
                }
            }

            if (Key != Key.None)
            {
                if (Key.TryGetAhkLabel(out var keyLabel))
                {
                    hotkeyName = $"{keyLabel}{(Direction is InputDirection.Up ? " UP" : "")}";
                    buffer.AppendLine($"{hotkeyName}::");
                }
            }
        }
        
        buffer.AppendLine("{");
        buffer.AppendLine($"{AhkFmt.SendMode(SendMode)}".Indent(4));

        buffer.AppendLine(RegisterCallback(hotkeyName, true));
        buffer.AppendLine(AhkFmt.Actions(4, AhkActions.ToArray()));
        buffer.AppendLine(RegisterCallback(hotkeyName, false));
        
        buffer.AppendLine("return".Indent(4));
        buffer.AppendLine("}");
        
        return buffer.ToString();
    }

    private string RegisterCallback(string hotkeyName, bool isStart)
    {
        if (Callback is not null)
        {
            _ = Ahk.HotkeyCallbacks.TryAdd(hotkeyName, Callback);

            var funcCall = AhkFmt.FuncCall("SendPipeMessage", hotkeyName.Quote());
            var assign = !string.IsNullOrWhiteSpace(PipeVariableName) ? AhkFmt.Set(PipeVariableName, funcCall).Indent(4) : (funcCall).Indent(4);

            switch (isStart)
            {
                case true when CallbackLocation is CallbackLocation.Start:
                    return assign;
                case false when CallbackLocation is CallbackLocation.End:
                    return assign;
            }
        }

        return "";
    }
}