using System;

namespace Flux.Hotkeys.Actions;

[PublicAPI]
public class SleepAction : IAction
{
    public TimeSpan Duration { get; private set; }

    public SleepAction For(TimeSpan duration)
    {
        Duration = duration;
        return this;
    }

    public SleepAction Minutes(int duration)
    {
        Duration = TimeSpan.FromMinutes(duration);
        return this;
    }

    
    public SleepAction Seconds(int duration)
    {
        Duration = TimeSpan.FromSeconds(duration);
        return this;
    }
    
    public SleepAction Milliseconds(int duration)
    {
        Duration = TimeSpan.FromMilliseconds(duration);
        return this;
    }
    
    public SleepAction Minutes(double duration)
    {
        Duration = TimeSpan.FromMinutes(duration);
        return this;
    }

    
    public SleepAction Seconds(double duration)
    {
        Duration = TimeSpan.FromSeconds(duration);
        return this;
    }
    
    public SleepAction Milliseconds(double duration)
    {
        Duration = TimeSpan.FromMilliseconds(duration);
        return this;
    }
    
    public string Build()
    {
        return $"Sleep, {Duration.TotalMilliseconds}";
        // return Ahk.Sleep(Duration);
    }
}