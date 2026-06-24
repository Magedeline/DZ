using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Entity that sets the facing direction when spawning.
/// Note: This is not a trigger, it extends Entity directly.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class SpawnFacingTrigger : Entity
{
    public enum Facings
    {
        Left = -1,
        Right = 1
    }

    public Facings Facing;
    public float Width;
    public float Height;

    public SpawnFacingTrigger(Vector2 position, float width, float height, Facings facing) : base("SpawnFacingTrigger")
    {
        Position = position;
        Width = width;
        Height = height;
        Facing = facing;
        
        // Invisible in normal gameplay
    }
}
