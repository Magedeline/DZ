#nullable enable
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ.Triggers;

[CustomEntity("DZ/MusicTrigger")]
public class MusicTrigger : Trigger {
    private string track;
    private bool setInSession;
    private bool resetOnLeave;
    private int progress;
    private string? oldTrack;

    public MusicTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        track = data.Attr("track", "");
        resetOnLeave = data.Bool("resetOnLeave", true);
        progress = data.Int("progress", 0);
        setInSession = data.Bool("setInSession", false);
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        Level level = SceneAs<Level>();
        if (resetOnLeave)
            oldTrack = level.Session.Audio.Music.Event;
        level.Session.Audio.Music.Event = SFX.EventnameByHandle(track);
        if (progress != 0)
            level.Session.Audio.Music.Progress = progress;
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
