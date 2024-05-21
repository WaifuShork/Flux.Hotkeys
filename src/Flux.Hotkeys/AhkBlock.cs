using System;
using System.Collections.Generic;
using Flux.Hotkeys.Actions;
using Flux.Hotkeys.Util;

namespace Flux.Hotkeys;

public class AhkBlock
{
    public string InitialSnippet { get; private set; }
    public List<IAction> Actions { get; private set; } = [];

    private AhkBlock(string initialSnippet)
    {
        InitialSnippet = initialSnippet;
    }

    public static AhkBlock Create(string initialSnippet = "")
    {
        return new AhkBlock(initialSnippet);
    }

    public AhkBlock Send(params Key[] keys)
    {
        Actions.Add(Keyboard.Send(keys));
        return this;
    }

    public AhkBlock Down(Key key, TimeSpan? duration = null, bool autoRelease = true)
    {
        Actions.Add(Keyboard.Down(key, duration, autoRelease));
        return this;
    }

    public AhkBlock Up(Key key)
    {
        Actions.Add(Keyboard.Up(key));
        return this;
    }
    
    public AhkBlock Press(Key key)
    {
        Actions.Add(Keyboard.Press(key));
        return this;
    }

    public AhkBlock Click(Key key, int amount = 1)
    {
        if (!key.IsMouse())
        {
            return this;
        }

        Actions.Add(Ahk.Snippet(AhkFmt.Click(key, amount)));
        return this;
    }
    
    public AhkBlock LClick()
    {
        Actions.Add(Mouse.Press(Key.LeftButton));
        return this;
    }
    
    public AhkBlock RClick()
    {
        Actions.Add(Mouse.Press(Key.RightButton));
        return this;
    }
    
    public AhkBlock Snippet(string code)
    {
        Actions.Add(Ahk.Snippet(code));
        return this;
    }

    public AhkBlock Def(IDefAction defAction)
    {
        Actions.Add(defAction);
        return this;
    }
    
    public AhkBlock Action(IAction action)
    {
        Actions.Add(action);
        return this;
    }
    
    public AhkBlock Action(string action)
    {
        Actions.Add(Ahk.Snippet(action));
        return this;
    }

    // Immediately executes the code
    public bool Execute()
    {
        return Execute(out _);
    }
    
    public bool Execute(out string code)
    {
        code = GetText();
        return Ahk.Execute(code);
    }

    // Completes/adds the code to the running script
    public bool Complete(ExecuteOption option = ExecuteOption.Run)
    {
        return Ahk.LoadScript(GetText(), option);
    }
    
    public bool Complete(out string code, ExecuteOption option = ExecuteOption.Run)
    {
        code = GetText();
        return Ahk.LoadScript(code, option);
    }
    
    private string GetText()
    {
        return AhkFmt.Actions(0, Actions.ToArray());
    }
}