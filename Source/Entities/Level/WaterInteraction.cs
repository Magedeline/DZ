using Nez;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Marker component attached to any entity that should interact with
/// <see cref="Water"/> (enter/exit ripples, audio, drip timers).
///
/// Attach to the player or any other swimming entity.
/// </summary>
public class WaterInteraction : Nez.Component
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

    public void Update()
    {
        if (DrippingTimer > 0f)
            DrippingTimer -= Time.DeltaTime;
    }
}
