using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that fades bloom intensity based on player position.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class BloomFadeTrigger : CelesteTrigger
{
    public float BloomAddFrom;
    public float BloomAddTo;
    public PositionModes PositionMode;

    public BloomFadeTrigger(Vector2 position, int width, int height, float bloomAddFrom, float bloomAddTo, PositionModes positionMode) : base(position, width, height)
    {
        BloomAddFrom = bloomAddFrom;
        BloomAddTo = bloomAddTo;
        PositionMode = positionMode;
    }

    public override void OnStay(PlayerController player)
    {
        float lerp = GetPositionLerp(player, PositionMode);
        float value = BloomAddFrom + (BloomAddTo - BloomAddFrom) * MathHelper.Clamp(lerp, 0f, 1f);
        // TODO: set bloom intensity: value
    }
}
