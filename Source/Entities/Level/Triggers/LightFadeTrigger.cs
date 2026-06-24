using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that fades lighting alpha based on player position.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class LightFadeTrigger : CelesteTrigger
{
    public float LightAddFrom;
    public float LightAddTo;
    public PositionModes PositionMode;

    public LightFadeTrigger(Vector2 position, int width, int height, float lightAddFrom, float lightAddTo, PositionModes positionMode) : base(position, width, height)
    {
        LightAddFrom = lightAddFrom;
        LightAddTo = lightAddTo;
        PositionMode = positionMode;
    }

    public override void OnStay(PlayerController player)
    {
        float lerp = GetPositionLerp(player, PositionMode);
        float value = LightAddFrom + (LightAddTo - LightAddFrom) * MathHelper.Clamp(lerp, 0f, 1f);
        // TODO: set lighting alpha: value
        // Session.LightingAlphaAdd = value;
    }
}
