using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that sets a fixed camera offset.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class CameraOffsetTrigger : CelesteTrigger
{
    public Vector2 CameraOffset;

    public CameraOffsetTrigger(Vector2 position, int width, int height, Vector2 cameraOffset) : base(position, width, height)
    {
        CameraOffset = cameraOffset;
        // In Celeste, the offset is multiplied by 48f (X) and 32f (Y)
        CameraOffset.X *= 48f;
        CameraOffset.Y *= 32f;
    }

    public override void OnEnter(PlayerController player)
    {
        // TODO: Set camera offset: CameraOffset
    }
}
