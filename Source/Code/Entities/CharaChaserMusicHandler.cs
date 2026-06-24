using System.Runtime.CompilerServices;
using Monocle;

namespace Celeste.Entities;

[Tracked(true)]
public class CharaChaserMusicHandler : Entity
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public CharaChaserMusicHandler()
    {
        base.Tag = (int)Tags.TransitionUpdate | (int)Tags.Global;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        int boundsLeft = 1150;
        int boundsRight = 2832;
        Player player = base.Scene.Tracker.GetEntity<Player>();
        if (player != null && Audio.CurrentMusic == "event:/music/pusheen/lvl2/chase")
        {
            float value = (player.X - (float)boundsLeft) / (float)(boundsRight - boundsLeft);
            Audio.SetMusicParam("escape", value);
        }
    }
}
