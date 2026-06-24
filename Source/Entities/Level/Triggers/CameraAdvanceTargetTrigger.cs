using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Advanced camera target trigger with separate X/Y lerp strengths and position modes.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class CameraAdvanceTargetTrigger : CelesteTrigger
{
    public Vector2 Target;
    public Vector2 LerpStrength;
    public PositionModes PositionModeX;
    public PositionModes PositionModeY;
    public bool XOnly;
    public bool YOnly;

    public CameraAdvanceTargetTrigger(Vector2 position, int width, int height, Vector2 target, Vector2 lerpStrength, 
        PositionModes positionModeX, PositionModes positionModeY, bool xOnly = false, bool yOnly = false) : base(position, width, height)
    {
        Target = target;
        LerpStrength = lerpStrength;
        PositionModeX = positionModeX;
        PositionModeY = positionModeY;
        XOnly = xOnly;
        YOnly = yOnly;
    }

    public override void OnStay(PlayerController player)
    {
        // TODO: Set camera anchor to Target
        // player.CameraAnchor = Target;
        // player.CameraAnchorLerp.X = MathHelper.Clamp(LerpStrength.X * GetPositionLerp(player, PositionModeX), 0f, 1f);
        // player.CameraAnchorLerp.Y = MathHelper.Clamp(LerpStrength.Y * GetPositionLerp(player, PositionModeY), 0f, 1f);
        // player.CameraAnchorIgnoreX = YOnly;
        // player.CameraAnchorIgnoreY = XOnly;
    }

    public override void OnLeave(PlayerController player)
    {
        bool insideOther = false;
        
        // Check if inside any other CameraTargetTrigger
        // foreach (var trigger in Scene.FindComponentsOfType<CameraTargetTrigger>())
        // {
        //     if (trigger.PlayerIsInside)
        //     {
        //         insideOther = true;
        //         break;
        //     }
        // }
        
        // Check if inside any other CameraAdvanceTargetTrigger
        if (!insideOther)
        {
            // foreach (var trigger in Scene.FindComponentsOfType<CameraAdvanceTargetTrigger>())
            // {
            //     if (trigger.PlayerIsInside)
            //     {
            //         insideOther = true;
            //         break;
            //     }
            // }
        }
        
        if (!insideOther)
        {
            // player.CameraAnchorLerp = Vector2.Zero;
        }
    }
}
