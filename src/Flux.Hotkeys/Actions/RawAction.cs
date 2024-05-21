using JetBrains.Annotations;

namespace Flux.Hotkeys.Actions;

[PublicAPI]
public class RawAction : IAction
{
    public string Code { get; private init; } = "";

    public static RawAction Snippet(string code)
    {
        return new RawAction
        {
            Code = code,
        };
    }
    
    public string Build()
    {
        return Code;
    }
    
    public static explicit operator RawAction(string s)
    {
        return Snippet(s);
    }
}