using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that changes the blackhole background strength.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class BlackholeStrengthTrigger : CelesteTrigger
{
    // Strength levels for blackhole background
    public enum Strengths
    {
        Low,
        Medium,
        High
    }

    private Strengths strength;

    public BlackholeStrengthTrigger(Vector2 position, int width, int height, Strengths strength) : base(position, width, height)
    {
        this.strength = strength;
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: Find BlackholeBG and set strength
        // var blackholeBg = Scene.FindComponentOfType<BlackholeBG>();
        // blackholeBg?.NextStrength(strength);
    }
}
