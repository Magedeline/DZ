using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that blocks the player from getting a checkpoint.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class CheckpointBlockerTrigger : CelesteTrigger
{
    public CheckpointBlockerTrigger(Vector2 position, int width, int height) : base(position, width, height)
    {
    }
    
    // This trigger acts as a marker/blocker - no additional logic needed
    // Other systems check for this trigger's presence
}
