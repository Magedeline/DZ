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
        Player player = base.Scene.Tracker.GetEntity<Player>();
        if (player == null || !(base.Scene is Level level))
        {
            return;
        }

        string music = Audio.CurrentMusic;
        if (music == "event:/DZ/music/lvl2/chase" || music == "event:/DZ/music/lvl2/evilDZ_CHara")
        {
            int boundsLeft = 1150;
            int boundsRight = 2832;
            float value = (player.X - (float)boundsLeft) / (float)(boundsRight - boundsLeft);
            Audio.SetMusicParam("escape", value);
        }
        else if (music == "event:/DZ/music/lvl4/chara_warning")
        {
            float boundsLeft = level.Bounds.Left;
            float boundsRight = level.Bounds.Right;
            float value = (player.X - boundsLeft) / (boundsRight - boundsLeft);
            Audio.SetMusicParam("escape", value);
        }
    }
}
