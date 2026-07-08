#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/AltMusicTrigger")]
public class AltMusicTrigger : Trigger {
    private string track;
    private bool resetOnLeave;
    private string? oldTrack;

    public AltMusicTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        track = data.Attr("track", "");
        resetOnLeave = data.Bool("resetOnLeave", true);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        Level level = SceneAs<Level>();
        if (resetOnLeave)
            oldTrack = level.Session.Audio.Music.Event;
        level.Session.Audio.Music.Event = SFX.EventnameByHandle(track);
        level.Session.Audio.Apply();
    }

    public override void OnLeave(Player player) {
        base.OnLeave(player);
        if (!resetOnLeave) return;
        Level level = SceneAs<Level>();
        level.Session.Audio.Music.Event = oldTrack;
        level.Session.Audio.Apply();
    }
}
