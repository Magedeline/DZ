using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's StaticMover.cs.
///
/// A non-active, non-visible <see cref="DZ.Nez.Component"/> that makes its owning
/// entity "ride" a <see cref="CelesteSolid"/> or <see cref="CelesteJumpThru"/>
/// platform.
///
/// When the linked platform moves the component's <see cref="OnMove"/> callback
/// is invoked.  If the platform shakes, <see cref="OnShake"/> fires.
/// If the platform is destroyed, <see cref="OnDestroy"/> fires (default: RemoveSelf).
///
/// Usage:
/// <code>
///   var sm = new StaticMover();
///   sm.SolidChecker    = solid => solid == myTargetSolid;
///   sm.OnMove          = delta => Entity.Position += delta;
///   myEntity.AddComponent(sm);
/// </code>
///
/// Actual platform-scanning and attachment are TODO (needs Tracker equivalent).
/// </summary>
public class StaticMover : DZ.Nez.Component
{
    // ── Callbacks ─────────────────────────────────────────────────────────────

    /// <summary>Returns <c>true</c> if this mover should ride the given solid.</summary>
    public Func<CelesteSolid, bool>      SolidChecker;

    /// <summary>Returns <c>true</c> if this mover should ride the given jump-thru.</summary>
    public Func<CelesteJumpThru, bool>   JumpThruChecker;

    /// <summary>Called each frame the linked platform moves, with the delta vector.</summary>
    public Action<Vector2> OnMove;

    /// <summary>Called when the linked platform shakes.</summary>
    public Action<Vector2> OnShake;

    /// <summary>Called when the linked platform is attached (first linked).</summary>
    public Action<DZ.Nez.Entity> OnAttach;

    /// <summary>Called when the linked platform is destroyed. Default: remove self.</summary>
    public Action OnDestroy;

    /// <summary>Called when the linked platform is disabled (collidable = false).</summary>
    public Action OnDisable;

    /// <summary>Called when the linked platform is re-enabled.</summary>
    public Action OnEnable;

    // ── Linked platform ───────────────────────────────────────────────────────

    /// <summary>The platform this mover is currently riding. Null if not attached.</summary>
    public DZ.Nez.Entity Platform { get; set; }

    // ── Constructor ───────────────────────────────────────────────────────────

    public StaticMover() { }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>Destroys this entity (or calls <see cref="OnDestroy"/> if set).</summary>
    public new void Destroy()
    {
        if (OnDestroy != null)
            OnDestroy();
        else
            Entity?.Destroy();
    }

    /// <summary>Propagates a shake amount to the owning entity.</summary>
    public void Shake(Vector2 amount) => OnShake?.Invoke(amount);

    /// <summary>
    /// Moves the owning entity by <paramref name="amount"/>.
    /// If <see cref="OnMove"/> is set it is called instead.
    /// </summary>
    public void Move(Vector2 amount)
    {
        if (OnMove != null)
            OnMove(amount);
        else if (Entity != null)
            Entity.Position += amount;
    }

    /// <summary>
    /// Returns whether this mover should ride the given <paramref name="solid"/>.
    /// </summary>
    public bool IsRiding(CelesteSolid solid) =>
        SolidChecker != null && SolidChecker(solid);

    /// <summary>
    /// Returns whether this mover should ride the given <paramref name="jumpThru"/>.
    /// </summary>
    public bool IsRiding(CelesteJumpThru jumpThru) =>
        JumpThruChecker != null && JumpThruChecker(jumpThru);

    /// <summary>
    /// Triggers the linked platform's OnStaticMoverTrigger callback (if any).
    /// </summary>
    public void TriggerPlatform()
    {
        // TODO: call Platform.OnStaticMoverTrigger(this) when CelestePlatform exposes it
    }

    /// <summary>
    /// Disables the owning entity, or calls <see cref="OnDisable"/> if set.
    /// </summary>
    public void Disable()
    {
        if (OnDisable != null)
            OnDisable();
        else if (Entity != null)
            Entity.Enabled = false;
    }

    /// <summary>
    /// Re-enables the owning entity, or calls <see cref="OnEnable"/> if set.
    /// </summary>
    public void Enable()
    {
        if (OnEnable != null)
            OnEnable();
        else if (Entity != null)
            Entity.Enabled = true;
    }
}
