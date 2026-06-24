using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that sets whether the player can refill.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class NoRefillTrigger : CelesteTrigger
{
    public bool State;

    public NoRefillTrigger(Vector2 position, int width, int height, bool state) : base(position, width, height)
    {
        State = state;
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: Set no refills state
        // Session.Inventory.NoRefills = State;
    }
}
