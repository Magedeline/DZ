using System;
using Monocle;

namespace DZ;

[Tracked(false)]
public class PostUpdateHookCH19 : Component
{
    public Action OnPostUpdate;

    public PostUpdateHookCH19(Action onPostUpdate)
        : base(active: false, visible: false)
    {
        OnPostUpdate = onPostUpdate;
    }
}
