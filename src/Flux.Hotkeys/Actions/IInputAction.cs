namespace Flux.Hotkeys.Actions;

[PublicAPI]
public interface IInputAction : IAction
{
    public Key Key { get; }
    public bool IsDown { get; }
    public bool IsUp { get; }
    public bool AutoRelease { get; }
    public bool InPressMode { get; }
    public bool InMultiMode { get; }
}