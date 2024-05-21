using System;
using Cysharp.Text;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Collections.Generic;
using Flux.Hotkeys.Util;
using Flux.Hotkeys.Actions;

namespace Flux.Hotkeys;

[PublicAPI]
public static class AhkFmt
{
    public static string Expression(string expression)
    {
        return expression.Wrap("%", "%");
    }
    
    public static string Set(string name, string value)
    {
        return $"{name} := {value}";
    }
    
    public static string FuncCall(string name, params string[] arguments)
    {
        return $"{name}({string.Join(',', arguments)})";
    }
    
    public static string SendText(string message)
    {
        return $"Send, {message}";
    }

    public static string Actions(int indent, params string[] actions)
    {
        return Actions(indent, actions.ToActions().ToArray());
    }
    
    public static string Actions(params string[] actions)
    {
        return Actions(0, actions);
    }
    
    public static string Actions(params IAction[] actions)
    {
        return Actions(0, actions);
    }
    
    public static string Actions(int indent, params IAction[] actions)
    {
        var builder = ZString.CreateStringBuilder();

        var currentlyHeldKeys = new Dictionary<Key, bool>();

        foreach (var action in actions)
        {
            if (action is ReleaseAction releaseAction)
            {
                var releaseKeys = releaseAction.Keys;
                builder.AppendLine(releaseAction.Build().Indent(indent));

                if (releaseAction.ReleaseAll)
                {
                    currentlyHeldKeys.Clear();
                }
                else
                {
                    foreach (var k in releaseKeys.Where(k => currentlyHeldKeys.ContainsKey(k)))
                    {
                        currentlyHeldKeys.Remove(k);
                    }
                }
            }
            else if (action is IInputAction inputAction)
            {
                builder.AppendLine(inputAction.Build().Indent(indent));
                if (!inputAction.InMultiMode)
                {
                    if (inputAction.AutoRelease)
                    {
                        currentlyHeldKeys[inputAction.Key] = true;
                    }

                    if (!inputAction.InPressMode)
                    {
                        if (inputAction.IsDown)
                        {
                            currentlyHeldKeys[inputAction.Key] = true;
                        }
                        else if (inputAction.IsUp)
                        {
                            if (currentlyHeldKeys.ContainsKey(inputAction.Key))
                            {
                                currentlyHeldKeys.Remove(inputAction.Key);
                            }
                        }
                    }
                }
            }
            else
            {
                builder.AppendLine(action.Build().Indent(indent));
            }
        }

        foreach (var (key, release) in currentlyHeldKeys)
        {
            if (release)
            {
                builder.AppendLine(Send(key, InputDirection.Up).Indent(indent));
            }
        }

        return builder.ToString();
    }
    
    public static string MsgBox(string text, bool isExpression = false)
    {
        try
        {
            var fullName = Assembly.GetCallingAssembly().FullName ?? "";
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var split = fullName.Split(",");
                if (split.Length > 0)
                {
                    fullName = split[0];
                }
            }

            return $"MsgBox, 4, {fullName}, {(isExpression ? text.Wrap("%", "%") : text)}";
        }
        catch
        {
            return "";
        }
    }
    
    public static string MsgBox(string title, string text, int options = 4, int? timeout = null, bool isExpression = false)
    {
        var t = timeout?.ToString() ?? "";
        return $"MsgBox, {options}, {title}, {(isExpression ? "% " : "")}{text}{(!string.IsNullOrWhiteSpace(t) ? $", {t}" : "")}";
    }

    public static string SendMode(SendMode sendMode)
    {
        return $"SendMode {sendMode:G}";
    }

    public static string Send(Key key, InputDirection direction)
    {
        if (!key.TryGetAhkLabel(out var keyLabel))
        {
            return "";
        }

        return "Send, " + $"{keyLabel}{(direction.IsValid() ? $" {direction.ToAhkLabel()}" : "")}".Wrap();
    }
    
    public static string SleepStr(TimeSpan duration)
    {
        return $"Sleep, {duration.TotalMilliseconds}";
    }

    public static string Click(Key? key = null, int amount = 1)
    {
        if (!key.HasValue)
        {
            return $"Click{(amount > 1 ? $", {amount}" : "")}"; 
        }

        return key.Value.TryGetMouseClickLabel(out var l) ? $"Click, , {l}, {(amount > 1 ? amount : "")}" : "";
    }

    public static string Click(Key key, InputDirection direction, Vector2? coordinates = null)
    {
        if (!key.IsMouse())
        {
            return "";
        }

        if (!key.TryGetMouseClickLabel(out var keyLabel))
        {
            return "";
        }

        var point = !coordinates.HasValue ? "" : $"{coordinates.Value.X} {coordinates.Value.Y} ";
        return "Click, {" + $"{point}{keyLabel}{(direction.IsValid() ? $" {direction.ToAhkLabel()}" : "")}" + "}";
    }
    
}