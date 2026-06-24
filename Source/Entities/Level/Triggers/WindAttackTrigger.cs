using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that spawns a wind attack (snowball) when entered.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class WindAttackTrigger : CelesteTrigger
{
    public WindAttackTrigger(Vector2 position, int width, int height) : base(position, width, height)
    {
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: Check if snowball already exists
        // if (Scene.FindComponentOfType<Snowball>() == null)
        //     Scene.Add(new Snowball());

        Destroy();
    }
}
