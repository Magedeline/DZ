using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that controls the wind pattern.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class WindTrigger : CelesteTrigger
{
    public enum Patterns
    {
        None,
        Left,
        Right,
        LeftStrong,
        RightStrong,
        LeftCrazy,
        RightCrazy,
        Alternating,
        LeftGemsOnly,
        RightGemsOnly
    }

    public Patterns Pattern;

    public WindTrigger(Vector2 position, int width, int height, Patterns pattern) : base(position, width, height)
    {
        Pattern = pattern;
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: Find or create WindController
        // var controller = Scene.FindComponentOfType<WindController>();
        // if (controller == null)
        //     Scene.Add(new WindController(Pattern));
        // else
        //     controller.SetPattern(Pattern);
    }
}
