using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using System;

namespace DZ.Entities.Level;

/// <summary>
/// A simple, invisible world-space marker that designates a valid player spawn location.
///
/// <para>
/// <see cref="SpawnPoint"/> carries no visual representation, no collider, and performs
/// no logic each frame. It exists purely as a named anchor so that scene-loading and
/// respawn code can query <c>Scene.FindEntitiesWithTag&lt;SpawnPoint&gt;()</c> (or a
/// similar search) and pick the right start position by <see cref="SpawnName"/>.
/// </para>
///
/// <para>
/// The <see cref="SpawnPosition"/> property is intentionally kept as an alias for
/// <c>Entity.Position</c> so callers never need to know the internal representation.
/// </para>
///
/// Usage example:
/// <code>
/// var spawn = new SpawnPoint(new Vector2(160, 90), "room_start");
/// scene.AddEntity(spawn);
///
/// // Later, when respawning:
/// var target = scene.FindEntitiesWithTag("SpawnPoint")
///                   .OfType&lt;SpawnPoint&gt;()
///                   .FirstOrDefault(s =&gt; s.SpawnName == "room_start");
/// player.Entity.Position = target?.SpawnPosition ?? Vector2.Zero;
/// </code>
/// </summary>
public class SpawnPoint : Entity
{
    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Human-readable name used to look up this spawn point by ID.
    /// Defaults to <c>"default"</c>.
    /// </summary>
    public string SpawnName { get; set; }

    // ── Position ─────────────────────────────────────────────────────────────

    /// <summary>
    /// World-space position where a player entity should be placed on spawn.
    /// Always equal to this entity's <c>Position</c>.
    /// </summary>
    public Vector2 SpawnPosition => Position;

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="SpawnPoint"/> marker.
    /// </summary>
    /// <param name="position">World-space position of the spawn point.</param>
    /// <param name="name">
    ///   Optional identifier string used when selecting between multiple spawn points.
    ///   Defaults to <c>"default"</c>.
    /// </param>
    public SpawnPoint(Vector2 position, string name = "default")
    {
        Position  = position;
        SpawnName = name;

        // Entity.Name doubles as a scene-search key.
        Name = $"SpawnPoint_{name}";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        // No collider, no renderer – purely a marker.
        // Add a debug gizmo here if desired during development.
    }
}
