using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that pulls the camera towards a target position.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class CameraTargetTrigger : CelesteTrigger
{
    public Vector2 Target;
    public float LerpStrength;
    public PositionModes PositionMode;
    public bool XOnly;
    public bool YOnly;
    public string? DeleteFlag;

    public CameraTargetTrigger(Vector2 position, int width, int height, Vector2 target, float lerpStrength, 
        PositionModes positionMode, bool xOnly = false, bool yOnly = false, string? deleteFlag = null) : base(position, width, height)
    {
        Target = target;
        LerpStrength = lerpStrength;
        PositionMode = positionMode;
        XOnly = xOnly;
        YOnly = yOnly;
        DeleteFlag = deleteFlag;
    }

    public override void OnStay(PlayerController player)
    {
        // TODO: Check if DeleteFlag is set
        // if (!string.IsNullOrEmpty(DeleteFlag) && Session.GetFlag(DeleteFlag))
        //     return;
        
        // TODO: Set camera anchor
        // player.CameraAnchor = Target;
        // player.CameraAnchorLerp = Vector2.One * MathHelper.Clamp(LerpStrength * GetPositionLerp(player, PositionMode), 0f, 1f);
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
        
        // Check if inside any CameraAdvanceTargetTrigger
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
