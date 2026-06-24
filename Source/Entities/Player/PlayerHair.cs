using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;

namespace KirbyCelesteStandalone.Entities.Player;

/// <summary>
/// Port of Celeste's PlayerHair.cs as a Nez <see cref="Component"/>.
///
/// Simulates the trailing hair segments that follow Madeline (or your player
/// character) with a spring-like, inertia-based motion.
///
/// Each frame <see cref="Update"/> is called first (wave oscillation, timing),
/// then <see cref="AfterUpdate"/> repositions the hair nodes so they trail
/// behind wherever the player head ended up this frame.
///
/// Attach this component to the same <see cref="Nez.Entity"/> as the player
/// character. Use <see cref="Nodes"/>[0] as the "root" (head attachment point)
/// and render segments 0 … <see cref="HairCount"/> from head to tip.
///
/// Colours:
/// <list type="bullet">
///   <item><see cref="HairColor"/> — fill colour (red while dashing, blue normally, etc.)</item>
///   <item><see cref="BorderColor"/> — outline / shadow drawn behind each segment.</item>
/// </list>
/// </summary>
public class PlayerHair : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Number of hair segments (including the root node at index 0).
    /// Default is 4, matching Celeste's normal hair.
    /// </summary>
    public int HairCount { get; set; } = 4;

    /// <summary>
    /// Vertical offset (pixels) applied per segment when stepping down the
    /// hair chain.  Default <c>(0, 2)</c> gives a slight downward droop.
    /// </summary>
    public Vector2 StepPerSegment { get; set; } = new Vector2(0f, 2f);

    /// <summary>
    /// Additional horizontal offset per segment, scaled by <see cref="Facing"/>.
    /// Creates the slight backwards sweep of the hair.  Default 0.5f.
    /// </summary>
    public float StepInFacingPerSegment { get; set; } = 0.5f;

    /// <summary>
    /// Maximum pixels per second each hair node is allowed to move toward its
    /// target each frame (spring strength / approach speed).  Default 64f.
    /// </summary>
    public float StepApproach { get; set; } = 64f;

    // -------------------------------------------------------------------------
    // Visual
    // -------------------------------------------------------------------------

    /// <summary>Fill colour of all hair segments.</summary>
    public Color HairColor { get; set; } = new Color(172, 50, 50); // Celeste red

    /// <summary>Outline / border colour drawn behind each segment.</summary>
    public Color BorderColor { get; set; } = Color.Black;

    /// <summary>Global opacity multiplier (0 = transparent, 1 = fully opaque).</summary>
    public float Alpha { get; set; } = 1f;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    /// <summary>
    /// The world-space positions of each hair segment.
    /// <c>Nodes[0]</c> is the root (attached to the player's head).
    /// Always has exactly <see cref="HairCount"/> entries; call
    /// <see cref="AfterUpdate"/> each frame to keep them up to date.
    /// </summary>
    public List<Vector2> Nodes { get; } = new();

    /// <summary>
    /// Whether to run the spring-simulation in <see cref="AfterUpdate"/>.
    /// Set to <c>false</c> during cutscenes or frozen states to lock the hair
    /// in place.
    /// </summary>
    public bool SimulateMotion { get; set; } = true;

    /// <summary>
    /// Direction the character is facing: <c>1</c> = right, <c>-1</c> = left.
    /// Drives the horizontal lean of the hair chain.
    /// </summary>
    public int Facing { get; set; } = 1;

    /// <summary>
    /// Offset from <see cref="Entity.Position"/> where the hair root is attached
    /// (the head "hair anchor" point).  Override to match your sprite's head
    /// position.
    /// </summary>
    public Vector2 HeadOffset { get; set; } = new Vector2(0f, -4f);

    /// <summary>Internal wave oscillation timer (radians).</summary>
    private float _waveTimer;

    /// <summary>
    /// Wave amplitude applied to each segment's Y position for a gentle float.
    /// </summary>
    private const float WaveAmplitude = 0.5f;

    /// <summary>Wave speed in radians per second.</summary>
    private const float WaveSpeed = 3f;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        InitialiseNodes();
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    /// <summary>
    /// Phase 1 — runs every frame before physics / movement.
    /// Advances the wave oscillation timer.
    /// </summary>
    public void Update()
    {
        _waveTimer += Time.DeltaTime * WaveSpeed;
        // Wrap to avoid floating-point drift over long sessions.
        if (_waveTimer > MathHelper.TwoPi)
            _waveTimer -= MathHelper.TwoPi;
    }

    /// <summary>
    /// Phase 2 — call this <em>after</em> the player entity has moved for the
    /// frame (e.g. from <c>PlayerController.Update</c> at the end, or via
    /// <c>Entity.Scene.AfterUpdate</c> subscription).
    ///
    /// Moves each node toward its "ideal" trailing position using a capped
    /// approach that mimics Celeste's spring physics.
    /// </summary>
    public void AfterUpdate()
    {
        // Ensure node list matches HairCount.
        while (Nodes.Count < HairCount) Nodes.Add(Entity.Position);
        while (Nodes.Count > HairCount) Nodes.RemoveAt(Nodes.Count - 1);

        // Root node snaps directly to the head anchor.
        Vector2 headAnchor = Entity.Position + HeadOffset;
        Nodes[0] = headAnchor;

        if (!SimulateMotion) return;

        float dt = Time.DeltaTime;

        for (int i = 1; i < HairCount; i++)
        {
            // Ideal position: step behind the previous node.
            Vector2 stepBack = new Vector2(-Facing * StepInFacingPerSegment, 0f)
                             + StepPerSegment;

            // Add a tiny sine wave undulation per segment.
            float wave = (float)Math.Sin(_waveTimer + i * 0.8f) * WaveAmplitude;
            stepBack.Y += wave;

            Vector2 target = Nodes[i - 1] + stepBack;

            // Approach: move at most StepApproach px/s toward the target.
            Nodes[i] = Approach(Nodes[i], target, StepApproach * dt);
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shifts all hair nodes by <paramref name="amount"/> pixels.
    /// Use this when teleporting or snapping the entity's position so the hair
    /// doesn't lag behind.
    /// </summary>
    public void MoveHairBy(Vector2 amount)
    {
        for (int i = 0; i < Nodes.Count; i++)
            Nodes[i] += amount;
    }

    /// <summary>
    /// Instantly snaps all nodes to their ideal trailing positions without
    /// any spring simulation (useful after teleporting / respawning).
    /// </summary>
    public void SnapToIdealPositions()
    {
        // Temporarily disable simulation, run AfterUpdate, re-enable.
        bool prev = SimulateMotion;
        SimulateMotion = false;
        AfterUpdate();
        SimulateMotion = prev;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialises the node list so it always contains <see cref="HairCount"/>
    /// entries placed at the entity's current position.
    /// </summary>
    private void InitialiseNodes()
    {
        Nodes.Clear();
        for (int i = 0; i < HairCount; i++)
            Nodes.Add(Entity.Position);
    }

    /// <summary>
    /// Moves <paramref name="current"/> toward <paramref name="target"/> by at
    /// most <paramref name="maxMove"/> units.  Mirrors Celeste's
    /// <c>Calc.Approach(Vector2, Vector2, float)</c>.
    /// </summary>
    private static Vector2 Approach(Vector2 current, Vector2 target, float maxMove)
    {
        Vector2 diff = target - current;
        float len = diff.Length();
        if (len <= maxMove || len == 0f)
            return target;
        return current + diff / len * maxMove;
    }
}
