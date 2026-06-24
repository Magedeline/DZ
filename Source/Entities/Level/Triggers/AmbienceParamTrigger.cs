using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that fades an ambience parameter based on player position.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class AmbienceParamTrigger : CelesteTrigger
{
    public string Parameter;
    public float From;
    public float To;
    public PositionModes PositionMode;

    public AmbienceParamTrigger(Vector2 position, int width, int height, string parameter, float from, float to, PositionModes positionMode) : base(position, width, height)
    {
        Parameter = parameter;
        From = from;
        To = to;
        PositionMode = positionMode;
    }

    public override void OnStay(PlayerController player)
    {
        float lerp = GetPositionLerp(player, PositionMode);
        float value = MathHelper.Clamp(lerp, 0f, 1f);
        float paramValue = From + (To - From) * value;
        // TODO: set ambience param: Parameter = paramValue
    }
}
