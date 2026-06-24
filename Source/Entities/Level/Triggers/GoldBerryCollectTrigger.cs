using Microsoft.Xna.Framework;
using Nez;
using System;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Trigger that marks an area for gold berry collection.
/// Ported from Celeste (BloodLantern/Celeste)
/// </summary>
public class GoldBerryCollectTrigger : CelesteTrigger
{
    public GoldBerryCollectTrigger(Vector2 position, int width, int height) : base(position, width, height)
    {
    }
    
    // This trigger acts as a marker - no additional logic needed
    // Other systems check for this trigger's presence
}
