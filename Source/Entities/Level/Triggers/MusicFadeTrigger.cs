using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that fades music volume based on player position.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class MusicFadeTrigger : CelesteTrigger
{
    public bool LeftToRight;
    public float FadeA;
    public float FadeB;
    public string? Parameter;

    public MusicFadeTrigger(Vector2 position, int width, int height, bool leftToRight, float fadeA, float fadeB, string? parameter = null) : base(position, width, height)
    {
        LeftToRight = leftToRight;
        FadeA = fadeA;
        FadeB = fadeB;
        Parameter = parameter;
    }

    public override void OnStay(PlayerController player)
    {
        float value;
        if (!LeftToRight)
        {
            // Map Y position to fade value
            value = MathHelper.Clamp((player.Entity.Position.Y - Position.Y) / Height, 0f, 1f);
            value = FadeA + (FadeB - FadeA) * value;
        }
        else
        {
            // Map X position to fade value
            value = MathHelper.Clamp((player.Entity.Position.X - Position.X) / Width, 0f, 1f);
            value = FadeA + (FadeB - FadeA) * value;
        }
        
        if (string.IsNullOrEmpty(Parameter))
        {
            // TODO: set music param: fade = value
        }
        else
        {
            // TODO: set music param: Parameter = value
        }
    }
}
