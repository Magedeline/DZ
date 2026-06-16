using System;
using Monocle;

namespace DZ;

[Tracked(false)]
public class PostUpdateHookCH18 : Component
{
    public Action OnPostUpdate;

    public PostUpdateHookCH18(Action onPostUpdate)
        : base(active: false, visible: false)
    {
        OnPostUpdate = onPostUpdate;
    }
}
