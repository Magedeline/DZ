using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's WindMover.cs.
///
/// A non-active, non-visible <see cref="DZ.Nez.Component"/> that allows its
/// owning entity to be pushed by wind controlled by a
/// <see cref="WindController"/> in the scene.
///
/// The <see cref="WindController"/> calls <see cref="Move"/> on all
/// <see cref="WindMover"/>s in the scene each frame.
///
/// Usage:
/// <code>
///   entity.AddComponent(new WindMover(windDelta =>
///   {
///       Entity.Position += windDelta;
///   }));
/// </code>
/// </summary>
public class WindMover : DZ.Nez.Component
{
    // ── Callback ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by <see cref="WindController"/> each frame with the wind displacement.
    /// The parameter is <c>windSpeed × 0.1 × deltaTime</c> (already scaled).
    /// </summary>
    public Action<Vector2> Move { get; }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="WindMover"/>.
    /// </summary>
    /// <param name="move">
    ///   Callback invoked each frame the wind is non-zero, receiving the
    ///   pre-scaled displacement vector.
    /// </param>
    public WindMover(Action<Vector2> move)
    {
        Move = move ?? throw new ArgumentNullException(nameof(move));
    }
}
