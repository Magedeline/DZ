using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that changes music when entered.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class MusicTrigger : CelesteTrigger
{
    public string Track;
    public bool SetInSession;
    public bool ResetOnLeave;
    public int Progress;
    private string? oldTrack;

    public MusicTrigger(Vector2 position, int width, int height, string track, bool resetOnLeave = true, int progress = 0, bool setInSession = false) : base(position, width, height)
    {
        Track = track;
        ResetOnLeave = resetOnLeave;
        Progress = progress;
        SetInSession = setInSession;
    }

    public override void OnEnter(PlayerController player)
    {
        if (ResetOnLeave)
        {
            // oldTrack = Audio.CurrentMusic;
        }
        
        // TODO: Set music
        // Session.Audio.Music.Event = SFX.EventnameByHandle(Track);
        // if (Progress != 0)
        //     Session.Audio.Music.Progress = Progress;
        // Session.Audio.Apply();
    }

    public override void OnLeave(PlayerController player)
    {
        if (!ResetOnLeave)
            return;
        
        // TODO: Restore music
        // Session.Audio.Music.Event = oldTrack;
        // Session.Audio.Apply();
    }
}
