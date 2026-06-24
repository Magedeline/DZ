using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Entity that marks a target position for respawning.
/// Note: This is not a trigger, it extends Entity directly.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class RespawnTargetTrigger : Entity
{
    public Vector2 Target;
    public float Width;
    public float Height;

    public RespawnTargetTrigger(Vector2 position, float width, float height, Vector2 target) : base("RespawnTargetTrigger")
    {
        Position = position;
        Width = width;
        Height = height;
        Target = target;
        
        // Invisible and inactive in normal gameplay
    }
}
