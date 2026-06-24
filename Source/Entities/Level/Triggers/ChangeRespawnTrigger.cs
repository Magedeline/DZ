using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that changes the player's respawn point.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class ChangeRespawnTrigger : CelesteTrigger
{
    public Vector2 Target;

    public ChangeRespawnTrigger(Vector2 position, int width, int height, Vector2 target) : base(position, width, height)
    {
        Target = target;
    }

    public override void OnEnter(PlayerController player)
    {
        if (!SolidCheck())
            return;
        
        // TODO: Set respawn point
        // Session.HitCheckpoint = true;
        // Session.RespawnPoint = Target;
        // Session.UpdateLevelStartDashes();
    }

    private bool SolidCheck()
    {
        Vector2 point = Target + new Vector2(0f, -4f);
        // TODO: Check for solid collision at point
        // return !Scene.CollideCheck<Solid>(point) || Scene.CollideCheck<FloatySpaceBlock>(point);
        return true;
    }
}
