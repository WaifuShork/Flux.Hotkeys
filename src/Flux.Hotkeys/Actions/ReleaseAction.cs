using System.Collections.Generic;
using System.Linq;

namespace Flux.Hotkeys.Actions;

[PublicAPI]
public class ReleaseAction : IAction
{
    public List<Key> Keys { get; private set; } = [];
    public bool ReleaseAll { get; private set; }

    public static ReleaseAction Release(IEnumerable<Key> keys, bool releaseAll = true)
    {
        return new ReleaseAction
        {
            Keys = keys.Distinct().ToList(),
            ReleaseAll = releaseAll,
        };
    }
    
    public string Build()
    {
        return "";
    }
}