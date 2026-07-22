#pragma warning disable CS8632 // this file uses '?' on a few types without opting the whole file into nullable-reference-type analysis

using DZ.Nez;
using Entity = DZ.Nez.Entity;

namespace DZ.Entities.Level;

/// <summary>
/// Marker component attached to any entity that should interact with
/// <see cref="Water"/> (enter/exit ripples, audio, drip timers).
///
/// Attach to the player or any other swimming entity.
/// </summary>
public class WaterInteraction : DZ.Nez.Component
{
    /// <summary>
    /// When positive the entity recently left the water and is dripping.
    /// Counted down each frame.
    /// </summary>
    public float DrippingTimer;

    /// <summary>
    /// Optional callback to determine whether the owning entity is currently
    /// dashing (affects audio choice).
    /// </summary>
    public System.Func<bool>? IsDashing;

    public override void Update()
    {
        if (DrippingTimer > 0f)
            DrippingTimer -= Time.DeltaTime;
    }
}
