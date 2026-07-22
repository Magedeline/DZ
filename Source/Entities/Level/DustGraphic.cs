using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using System;
using System.Collections.Generic;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Animated dust creature / dust-hazard visual component.
/// Ported from Celeste's DustGraphic.cs.
///
/// Manages four corner nodes that expand into the surrounding space when not
/// blocked by solids.  Has optional eyes that track the player.  The component
/// registers a <see cref="DustEdge"/> callback so the dust renderer can collect
/// and sort all dust visuals.
/// </summary>
public class DustGraphic : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public
    // -------------------------------------------------------------------------

    /// <summary>Local offset from the entity's world position.</summary>
    public Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>Uniform scale applied to the center/node sprites.</summary>
    public float Scale { get; set; } = 1f;

    /// <summary>Action invoked when the owning entity is established (first in-view setup).</summary>
    public Action OnEstablish { get; set; }

    // Eye-tracking
    public Vector2 EyeTargetDirection { get; set; } = Vector2.UnitX;
    public Vector2 EyeDirection       { get; set; } = Vector2.UnitX;
    public int     EyeFlip            { get; set; } = 1;

    // Exposed node lists for external manipulation
    public List<Node> LeftNodes   { get; } = new();
    public List<Node> RightNodes  { get; } = new();
    public List<Node> TopNodes    { get; } = new();
    public List<Node> BottomNodes { get; } = new();

    /// <summary>True once the node expansion has run at least once.</summary>
    public bool Established { get; private set; }

    /// <summary>World-space render position (with shake applied).</summary>
    public Vector2 RenderPosition =>
        Entity != null ? Entity.Position + Position + _shakeValue : Position + _shakeValue;

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    private readonly bool _ignoreSolids;
    private readonly bool _autoControlEyes;
    private readonly bool _autoExpandDust;
    private readonly bool _eyesExist;
    private readonly bool _eyesFollowPlayer;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly List<Node> _nodes        = new();
    private float               _timer;
    private readonly float      _offset;
    private float               _shakeTimer;
    private Vector2             _shakeValue;
    private bool                _eyesMoveByRotation;

    // Blink state
    private bool  _leftEyeVisible  = true;
    private bool  _rightEyeVisible = true;
    private float _blinkTimer;
    private float _blinkInterval;

    // Dead-eyes flag (after hitting player)
    private bool _deadEyes;

    private MadelinePlayer _player;

    // -------------------------------------------------------------------------
    // Nested types
    // -------------------------------------------------------------------------

    public class Node
    {
        public Vector2 Direction;
        public bool    Enabled;
        public float   Rotation;
        // TODO: MTexture[] Textures – loaded from "danger/dustcreature/node"
    }

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="ignoreSolids">
    /// When true, corner nodes always expand regardless of solid tiles.
    /// </param>
    /// <param name="autoControlEyes">
    /// When true, whether eyes appear and whether they follow the player are
    /// randomised on construction.
    /// </param>
    /// <param name="autoExpandDust">
    /// When true, nodes automatically expand on first draw.
    /// </param>
    public DustGraphic(bool ignoreSolids,
                       bool autoControlEyes = false,
                       bool autoExpandDust  = false)
    {
        _ignoreSolids    = ignoreSolids;
        _autoControlEyes = autoControlEyes;
        _autoExpandDust  = autoExpandDust;

        _offset = DZ.Nez.Random.NextFloat() * 4f;
        _timer  = DZ.Nez.Random.NextFloat();

        EyeTargetDirection = EyeDirection =
            new Vector2(MathF.Cos(DZ.Nez.Random.NextFloat() * MathF.PI * 2f),
                        MathF.Sin(DZ.Nez.Random.NextFloat() * MathF.PI * 2f));

        if (autoControlEyes)
        {
            _eyesExist      = DZ.Nez.Random.Chance(0.5f);
            _eyesFollowPlayer = DZ.Nez.Random.Chance(0.3f);
        }
        else
        {
            _eyesExist = true;
        }

        // Blink timing
        _blinkInterval = DZ.Nez.Random.Range(2f, 5f);
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Register the render callback so the dust-edge renderer picks us up
        Entity.AddComponent(new DustEdge(Render));

        // TODO: load "danger/dustcreature/center" atlas sub-textures (random choice)
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        _timer += dt * 0.6f;

        // Shake
        if (_shakeTimer > 0f)
        {
            _shakeTimer -= dt;
            if (_shakeTimer <= 0f)
                _shakeValue = Vector2.Zero;
            else if (Entity?.Scene != null && (int)(Time.TotalTime / 0.05f) !=
                                             (int)((Time.TotalTime - dt) / 0.05f))
                _shakeValue = new Vector2(DZ.Nez.Random.Range(-1f, 1f),
                                          DZ.Nez.Random.Range(-1f, 1f));
        }

        // Eyes
        if (_eyesExist && !_deadEyes)
        {
            if (EyeDirection != EyeTargetDirection)
            {
                if (!_eyesMoveByRotation)
                {
                    EyeDirection = MoveToward(EyeDirection, EyeTargetDirection, 12f * dt);
                }
                else
                {
                    float cur    = MathF.Atan2(EyeDirection.Y, EyeDirection.X);
                    float tgt    = MathF.Atan2(EyeTargetDirection.Y, EyeTargetDirection.X);
                    float delta  = WrapAngle(tgt - cur);
                    float maxD   = 8f * dt;
                    float newAng = cur + Math.Clamp(delta, -maxD, maxD);
                    EyeDirection = new Vector2(MathF.Cos(newAng), MathF.Sin(newAng));
                }
            }

            if (_eyesFollowPlayer && _player != null)
            {
                Vector2 toPlayer = (_player.Position - Entity.Position);
                float   len      = toPlayer.Length();
                if (len > 0f) EyeTargetDirection = toPlayer / len;
            }

            // Blink timer
            _blinkTimer += dt;
            if (_blinkTimer >= _blinkInterval)
            {
                _blinkTimer    = 0f;
                _blinkInterval = DZ.Nez.Random.Range(2f, 5f);
                _leftEyeVisible  = false;
                _rightEyeVisible = false;
                // TODO: schedule re-show after ~0.1 s using a coroutine or timer
            }
        }

        // First-time node expansion
        if (_nodes.Count == 0 && Entity.Scene != null && !Established)
            ExpandNodes();

        foreach (var node in _nodes)
            node.Rotation += dt * 0.5f;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Called when the dust creature hits the player.</summary>
    public void OnHitPlayer()
    {
        _shakeTimer      = 0.6f;
        _deadEyes        = true;
        _leftEyeVisible  = true;
        _rightEyeVisible = true;
        // TODO: swap eye texture to "danger/dustcreature/deadEyes"
    }

    // -------------------------------------------------------------------------
    // Rendering callback (registered with DustEdge)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by the dust-edge renderer each frame.
    /// </summary>
    public override void Render()
    {
        Vector2 rp = RenderPosition;

        // Draw corner nodes
        foreach (var node in _nodes)
        {
            if (!node.Enabled) continue;
            // TODO: draw node texture at rp + node.Direction * 4 * Scale,
            //   rotated by node.Rotation, scaled by Scale,
            //   with dust style color
        }

        // Draw center texture
        // TODO: draw center texture at rp, scaled by Scale,
        //   with slight sine offset using _timer + _offset

        // Draw eyes
        if (_eyesExist && !_deadEyes)
        {
            if (_leftEyeVisible)
            {
                // TODO: draw left eye at rp + EyeDirection * 1.5f - (1.5f,0)
            }
            if (_rightEyeVisible)
            {
                // TODO: draw right eye at rp + EyeDirection * 1.5f + (1.5f,0)
            }
        }
        else if (_deadEyes)
        {
            // TODO: draw "deadEyes" texture at rp
        }
    }

    // -------------------------------------------------------------------------
    // Node expansion
    // -------------------------------------------------------------------------

    private void ExpandNodes()
    {
        Established = true;
        OnEstablish?.Invoke();

        int x = (int)Entity.Position.X;
        int y = (int)Entity.Position.Y;

        bool nw = _ignoreSolids; // TODO: || !scene.CollideCheck<Solid>(x-8,y-8,8,8)
        bool ne = _ignoreSolids;
        bool sw = _ignoreSolids;
        bool se = _ignoreSolids;

        AddNode(new Vector2(-1f, -1f), nw);
        AddNode(new Vector2(+1f, -1f), ne);
        AddNode(new Vector2(-1f, +1f), sw);
        AddNode(new Vector2(+1f, +1f), se);

        // Adjust Position based on which nodes are enabled
        if (nw || sw) Position -= Vector2.UnitX;
        if (ne || se) Position += Vector2.UnitX;
        if (nw || ne) Position -= Vector2.UnitY;
        if (sw || se) Position += Vector2.UnitY;

        int enabledCount = 0;
        foreach (var n in _nodes) if (n.Enabled) enabledCount++;
        _eyesMoveByRotation = enabledCount < 4;
    }

    private void AddNode(Vector2 direction, bool enabled)
    {
        var node = new Node { Direction = direction, Enabled = enabled };
        _nodes.Add(node);

        if (direction.X < 0) LeftNodes.Add(node);
        else                  RightNodes.Add(node);
        if (direction.Y < 0) TopNodes.Add(node);
        else                  BottomNodes.Add(node);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Vector2 MoveToward(Vector2 current, Vector2 target, float maxDelta)
    {
        Vector2 diff = target - current;
        float   len  = diff.Length();
        return len <= maxDelta ? target : current + diff / len * maxDelta;
    }

    private static float WrapAngle(float angle)
    {
        while (angle >  MathF.PI) angle -= MathF.PI * 2f;
        while (angle < -MathF.PI) angle += MathF.PI * 2f;
        return angle;
    }
}
