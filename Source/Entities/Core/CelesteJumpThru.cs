using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using KirbyCelesteStandalone.Core;

namespace KirbyCelesteStandalone.Entities.Core;

// =============================================================================
//  CelesteJumpThru
// =============================================================================

/// <summary>
/// Port of Celeste's JumpThru.cs (the abstract base) combined with the logic
/// from JumpThruPlatform.cs (the standard tile-based implementation).
///
/// A JumpThru is a one-way platform: actors can jump up through it from below,
/// but land on it from above.  It extends <see cref="CelestePlatform"/> and
/// therefore inherits subpixel-accurate movement, LiftSpeed, and shake support.
///
/// Collision contract:
/// <list type="bullet">
///   <item>The platform is 5 pixels tall (standard Celeste hitbox height).</item>
///   <item>An actor "rides" the jump-thru when its bottom edge is at or within
///         1 pixel of the platform's top edge and its horizontal span overlaps.</item>
///   <item><see cref="CelesteActor.MoveVExact"/> checks
///         <see cref="CelesteActor.CollideJumpThru"/> only when moving downward
///         and only when the actor's bottom is above the platform top before the
///         step — thus implementing one-way behaviour without any special flags.</item>
/// </list>
///
/// Moving-platform behaviour:
/// <list type="bullet">
///   <item><see cref="MoveHExact"/> / <see cref="MoveVExact"/> carry riding actors.</item>
///   <item><see cref="Speed"/> (inherited from <see cref="CelestePlatform"/> via
///         <see cref="CelesteJumpThru.Speed"/>) is integrated each frame.</item>
/// </list>
///
/// Rendering:
/// <list type="bullet">
///   <item>Tile-based rendering is left as a TODO; see <see cref="JumpThruPlatform"/>
///         for the subclass that would normally drive it.</item>
/// </list>
/// </summary>
public class CelesteJumpThru : CelestePlatform
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// Standard hitbox height for jump-thru platforms in Celeste (5 px).
    /// </summary>
    public const float JumpThruHitboxHeight = 5f;

    // -------------------------------------------------------------------------
    // Velocity (for moving jump-thru platforms)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Current velocity of this jump-thru in pixels per second.
    /// Set to zero for static platforms.
    /// </summary>
    public Vector2 Speed;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new jump-thru platform.
    /// </summary>
    /// <param name="position">
    ///   Top-left world position. The hitbox will be
    ///   <paramref name="width"/> × <see cref="JumpThruHitboxHeight"/> pixels.
    /// </param>
    /// <param name="width">Width in pixels.</param>
    public CelesteJumpThru(Vector2 position, float width)
        : base(position, width, JumpThruHitboxHeight)
    {
        Collidable = true;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Register a trigger collider so Nez's broadphase is aware of this platform.
        // It is marked as a trigger so Nez does not resolve collision responses on its
        // own — all one-way logic is handled manually in CelesteActor.MoveVExact.
        var collider = AddComponent(new BoxCollider(0f, 0f, Width, Height));
        collider.PhysicsLayer = PhysicsLayers.JumpThru;
        collider.IsTrigger    = true;
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    /// <summary>
    /// Integrates <see cref="Speed"/> each frame if this is a moving platform.
    /// </summary>
    public override void Update()
    {
        base.Update(); // shake + LiftSpeed timer

        float dt = Time.DeltaTime;

        // Propagate velocity to LiftSpeed so riders pick it up.
        if (Speed != Vector2.Zero)
            SetLiftSpeed(Speed);

        if (Speed.X != 0f) MoveH(Speed.X * dt);
        if (Speed.Y != 0f) MoveV(Speed.Y * dt);
    }

    // -------------------------------------------------------------------------
    // CelestePlatform — exact movement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Moves the jump-thru platform exactly <paramref name="move"/> pixels
    /// horizontally, carrying any riding actors.
    /// </summary>
    public override void MoveHExact(int move)
    {
        if (Scene == null) return;

        var riders = GetRiders();

        // Disable collision so actors can pass through us during teleport.
        Collidable = false;
        Position += new Vector2(move, 0f);
        UpdateBounds();
        Collidable = true;

        int sign = Math.Sign(move);

        // Carry riders.
        foreach (var actor in riders)
        {
            actor.SetLiftSpeed(LiftSpeed);
            actor.MoveH(move);
        }

        // Push non-riding actors that now overlap (actors can't be pushed up or down
        // by a jump-thru, only horizontally if they are already on top of it).
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            if (Scene.Entities[i] is not CelesteActor actor) continue;
            if (!actor.Collidable) continue;
            if (riders.Contains(actor)) continue;

            // Only push if the actor is overlapping us horizontally AND its feet
            // are already resting on the platform (IsRiding).
            if (actor.IsRiding(this) && Bounds.Intersects(actor.Bounds) && actor.AllowPushing)
            {
                float pushDist = sign > 0
                    ? Bounds.Right - actor.Bounds.Left
                    : actor.Bounds.Right - Bounds.Left;

                actor.MoveH((int)(pushDist + 0.5f) * sign);
            }
        }
    }

    /// <summary>
    /// Moves the jump-thru platform exactly <paramref name="move"/> pixels
    /// vertically, carrying riding actors.
    ///
    /// Note: Jump-thrus do NOT squish actors — if there is no room the actor
    /// is simply left behind (the platform passes through it from below).
    /// </summary>
    public override void MoveVExact(int move)
    {
        if (Scene == null) return;

        var riders = GetRiders();

        Collidable = false;
        Position += new Vector2(0f, move);
        UpdateBounds();
        Collidable = true;

        int sign = Math.Sign(move);

        // Carry riders upward (moving up: lift the actor with us).
        // When moving downward the actor lands later naturally.
        if (sign < 0)
        {
            foreach (var actor in riders)
            {
                actor.SetLiftSpeed(LiftSpeed);
                actor.MoveV(move);
            }
        }
        else
        {
            // Moving downward — only carry if the actor would otherwise float.
            foreach (var actor in riders)
            {
                // If the actor is still above the platform top after our move,
                // don't carry it (it will land on its own).
                if (actor.Bounds.Bottom > Bounds.Top + Height)
                {
                    actor.SetLiftSpeed(LiftSpeed);
                    actor.MoveV(move);
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Rider queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="HashSet{T}"/> of every <see cref="CelesteActor"/>
    /// currently riding (standing on top of) this jump-thru.
    /// </summary>
    public HashSet<CelesteActor> GetRiders()
    {
        var riders = new HashSet<CelesteActor>();
        if (Scene == null) return riders;

        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            if (Scene.Entities[i] is CelesteActor actor && actor.IsRiding(this))
                riders.Add(actor);
        }

        return riders;
    }

    /// <summary>
    /// Returns true if at least one actor is riding this platform.
    /// </summary>
    public bool HasRider()
    {
        if (Scene == null) return false;
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            if (Scene.Entities[i] is CelesteActor actor && actor.IsRiding(this))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if an entity carrying a <see cref="PlayerController"/> component
    /// is riding this platform. The entity must also be a <see cref="CelesteActor"/>.
    /// </summary>
    public bool HasPlayerRider()
    {
        if (Scene == null) return false;
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity is CelesteActor actor
                && actor.GetComponent<PlayerController>() != null
                && actor.IsRiding(this))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the <see cref="PlayerController"/> component of the first entity riding
    /// this platform, or <c>null</c> if none.
    /// </summary>
    public PlayerController? GetPlayerRider()
    {
        if (Scene == null) return null;
        for (int i = 0; i < Scene.Entities.Count; i++)
        {
            var entity = Scene.Entities[i];
            if (entity is CelesteActor actor && actor.IsRiding(this))
            {
                var pc = actor.GetComponent<PlayerController>();
                if (pc != null) return pc;
            }
        }
        return null;
    }
}

// =============================================================================
//  JumpThruPlatform
// =============================================================================

/// <summary>
/// Port of Celeste's JumpThruPlatform — the standard tile-column–based
/// jump-through platform entity used throughout Celeste's levels.
///
/// Extends <see cref="CelesteJumpThru"/> with:
/// <list type="bullet">
///   <item>A <see cref="TextureName"/> property that identifies which tileset to
///         render (the actual sprite rendering is a TODO until the asset pipeline
///         is set up).</item>
///   <item>Column count computed from width ÷ 8 (standard Celeste tile size).</item>
///   <item>Edge vs. middle tile variant selection placeholder.</item>
/// </list>
///
/// Rendering:
/// This class creates the entity and computes column metadata. Replace the
/// TODO comments below with actual sprite/texture loading once the asset
/// pipeline is integrated.
/// </summary>
public class JumpThruPlatform : CelesteJumpThru
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Standard Celeste tile size in pixels.</summary>
    public const int TileSize = 8;

    // -------------------------------------------------------------------------
    // Rendering metadata
    // -------------------------------------------------------------------------

    /// <summary>
    /// Atlas texture name for this platform (e.g. <c>"wood"</c>, <c>"cliff"</c>).
    /// Used to look up sprites in the GFX atlas — currently a placeholder.
    /// </summary>
    public string TextureName { get; private set; }

    /// <summary>
    /// Number of 8-pixel tile columns this platform spans.
    /// Computed from <see cref="CelestePlatform.Width"/> in the constructor.
    /// </summary>
    public int Columns { get; private set; }

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new tiled jump-thru platform.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels (should be a multiple of 8).</param>
    /// <param name="textureName">
    ///   Texture/tileset name for rendering (e.g. <c>"wood"</c>).
    ///   Ignored until the GFX pipeline is implemented.
    /// </param>
    public JumpThruPlatform(Vector2 position, float width, string textureName = "wood")
        : base(position, width)
    {
        TextureName = textureName;
        Columns     = (int)(width / TileSize);
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        SetupSprites();
    }

    // -------------------------------------------------------------------------
    // Rendering setup
    // -------------------------------------------------------------------------

    /// <summary>
    /// Placeholder for tile sprite setup.
    ///
    /// In the original Celeste this method picks left-edge, middle, and right-edge
    /// tile textures from the GFX atlas and adds them as child SpriteRenderers at
    /// the appropriate column offsets.
    ///
    /// Replace this body once the asset pipeline is in place:
    /// <code>
    /// // Example (not compiled):
    /// for (int col = 0; col &lt; Columns; col++)
    /// {
    ///     string variant = col == 0 ? "left" : (col == Columns - 1 ? "right" : "middle");
    ///     var sprite = GFX.Game[$"objects/jumpthru/{TextureName}"][variant];
    ///     var renderer = AddComponent(new SpriteRenderer(sprite));
    ///     renderer.LocalOffset = new Vector2(col * TileSize + TileSize / 2f, Height / 2f);
    /// }
    /// </code>
    /// </summary>
    private void SetupSprites()
    {
        // TODO: load sprite — GFX pipeline not yet integrated.
        // When ready:
        //   for (int col = 0; col < Columns; col++)
        //   {
        //       string variant = col == 0 ? "l" : (col == Columns - 1 ? "r" : "m");
        //       var tex  = /* load from atlas: objects/jumpthru/{TextureName}/{variant} */;
        //       var sr   = AddComponent(new SpriteRenderer(tex));
        //       sr.LocalOffset = new Vector2(col * TileSize + TileSize / 2f, Height / 2f);
        //   }
    }

    // -------------------------------------------------------------------------
    // Helpers — column metadata
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the tile-column variant for the given column index:
    /// <c>"l"</c> (left edge), <c>"r"</c> (right edge), or <c>"m"</c> (middle).
    /// </summary>
    public string GetColumnVariant(int column)
    {
        if (column == 0)              return "l";
        if (column == Columns - 1)   return "r";
        return "m";
    }

    /// <summary>
    /// World-space X position of the left edge of column <paramref name="column"/>.
    /// </summary>
    public float GetColumnX(int column)
        => Position.X + column * TileSize;
}
