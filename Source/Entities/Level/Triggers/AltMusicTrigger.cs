using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that sets alternative music when entered.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class AltMusicTrigger : CelesteTrigger
{
    public string Track;
    public bool ResetOnLeave;

    public AltMusicTrigger(Vector2 position, int width, int height, string track, bool resetOnLeave = true) : base(position, width, height)
    {
        Track = track;
        ResetOnLeave = resetOnLeave;
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: play alt music: SFX.EventnameByHandle(Track)
    }

    public override void OnLeave(PlayerController player)
    {
        if (!ResetOnLeave)
            return;
        // TODO: stop alt music
    }
}
