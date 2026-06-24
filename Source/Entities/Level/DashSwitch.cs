using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's DashSwitch.cs.
///
/// A switch that is activated by dashing into it from a specific direction.
/// Can be persistent and can open TempleGates.
/// </summary>
public class DashSwitch : CelesteSolid
{
    // -------------------------------------------------------------------------
    // Enum
    // -------------------------------------------------------------------------

    public enum Sides
    {
        Up,
        Down,
        Left,
        Right
    }

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>The side the switch faces (direction to dash from).</summary>
    public Sides Side { get; private set; }

    /// <summary>Whether this switch persists between sessions.</summary>
    public bool Persistent { get; private set; }

    /// <summary>Whether this switch opens all gates in the level.</summary>
    public bool AllGates { get; private set; }

    /// <summary>Unique identifier for this switch.</summary>
    public string Id { get; private set; }

    /// <summary>Whether the switch has been pressed.</summary>
    public bool Pressed { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _pressedTarget;
    private Vector2 _pressDirection;
    private string _spriteName;
    private float _speedY;
    private float _startY;
    private bool _playerWasOn;
    private bool _mirrorMode;
    private float _colliderWidth;
    private float _colliderHeight;
    private BoxCollider? _collider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public DashSwitch(Vector2 position, Sides side, bool persistent, bool allGates, string id, string spriteName)
        : base(position, 0f, 0f, safe: true)
    {
        Side = side;
        Persistent = persistent;
        AllGates = allGates;
        Id = id;
        _spriteName = spriteName;
        _mirrorMode = spriteName != "default";

        // Collider will be set up in OnAddedToEntity based on side
        _colliderWidth = (side == Sides.Up || side == Sides.Down) ? 16f : 8f;
        _colliderHeight = (side == Sides.Up || side == Sides.Down) ? 8f : 16f;

        // Calculate press direction and target
        switch (side)
        {
            case Sides.Up:
                _pressDirection = -Vector2.UnitY;
                _pressedTarget = Position + Vector2.UnitY * -8f;
                break;
            case Sides.Down:
                _pressDirection = Vector2.UnitY;
                _pressedTarget = Position + Vector2.UnitY * 8f;
                _startY = Position.Y;
                break;
            case Sides.Left:
                _pressDirection = -Vector2.UnitX;
                _pressedTarget = Position + Vector2.UnitX * -8f;
                break;
            case Sides.Right:
                _pressDirection = Vector2.UnitX;
                _pressedTarget = Position + Vector2.UnitX * 8f;
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Replace the default collider with our sized one
        var existingCollider = GetComponent<BoxCollider>();
        if (existingCollider != null)
            RemoveComponent(existingCollider);

        _collider = new BoxCollider(_colliderWidth, _colliderHeight);
        _collider.IsTrigger = true;
        AddComponent(_collider);

        // TODO: load sprite "dashSwitch_{_spriteName}"
        // TODO: position and rotate sprite based on side
        // TODO: play idle animation

        // Check if already pressed
        if (Persistent && CheckFlag())
        {
            Pressed = true;
            // TODO: sprite.Play("pushed");
            Position = _pressedTarget - _pressDirection * 2f;
            Collidable = false;

            if (AllGates)
            {
                OpenAllGates();
            }
            else
            {
                GetNearestGate()?.StartOpen();
            }
        }
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        if (Pressed || Side != Sides.Down) return;

        // Down-facing switches have springy button behavior
        var player = GetPlayerOnTop();
        if (player != null)
        {
            // Player on top - depress button
            if (_speedY < 0f)
                _speedY = 0f;

            _speedY = Calc.Approach(_speedY, 70f, 200f * Time.DeltaTime);
            MoveTowardsY(_startY + 2f, _speedY * Time.DeltaTime);

            if (!_playerWasOn)
            {
                // TODO: play sound: event:/game/05_mirror_temple/button_depress
            }
        }
        else
        {
            // No player - return button to up position
            if (_speedY > 0f)
                _speedY = 0f;

            _speedY = Calc.Approach(_speedY, -150f, 200f * Time.DeltaTime);
            MoveTowardsY(_startY, -_speedY * Time.DeltaTime);

            if (_playerWasOn)
            {
                // TODO: play sound: event:/game/05_mirror_temple/button_return
            }
        }

        _playerWasOn = player != null;
    }

    // -------------------------------------------------------------------------
    // Activation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when dashed into by the player.
    /// </summary>
    public DashCollisionResult OnDashed(MadelinePlayer player, Vector2 direction)
    {
        if (Pressed) return DashCollisionResult.NormalCollision;

        // Check if dash direction matches press direction
        if (Vector2.Dot(direction, _pressDirection) < 0.9f)
            return DashCollisionResult.NormalCollision;

        // Press the switch
        Press(player);
        return DashCollisionResult.NormalCollision;
    }

    private void Press(MadelinePlayer player)
    {
        if (Pressed) return;

        Pressed = true;

        // TODO: rumble
        // TODO: play sound: event:/game/05_mirror_temple/button_activate
        // TODO: sprite.Play("push");

        MoveTo(_pressedTarget);
        Collidable = false;
        Position -= _pressDirection * 2f;

        // Emit particles
        // TODO: emit press particles

        // Open gates
        if (AllGates)
        {
            OpenAllGates();
        }
        else
        {
            GetNearestGate()?.SwitchOpen();
        }

        // Set flag if persistent
        if (Persistent)
        {
            SetFlag();
        }
    }

    private void OpenAllGates()
    {
        foreach (var gate in Scene.FindEntitiesWithTag(0).OfType<TempleGate>())
        {
            if (gate.GateType == TempleGate.Types.NearestSwitch)
            {
                gate.StartOpen();
            }
        }
    }

    private TempleGate? GetNearestGate()
    {
        TempleGate? nearest = null;
        float nearestDistSq = float.MaxValue;

        foreach (var gate in Scene.FindEntitiesWithTag(0).OfType<TempleGate>())
        {
            if (gate.GateType != TempleGate.Types.NearestSwitch) continue;
            if (gate.ClaimedByASwitch) continue;

            float distSq = Vector2.DistanceSquared(Position, gate.Position);
            if (distSq < nearestDistSq)
            {
                nearest = gate;
                nearestDistSq = distSq;
            }
        }

        if (nearest != null)
        {
            nearest.ClaimedByASwitch = true;
        }

        return nearest;
    }

    private MadelinePlayer? GetPlayerOnTop()
    {
        // Check for player colliding with the switch
        var colliderWidth = _collider?.Bounds.Width ?? Width;
        var rect = new RectangleF(
            Position.X,
            Position.Y - 4f,
            colliderWidth,
            4f);

        // TODO: Physics.OverlapRectangleAll not available - using stub
        // var hits = Physics.OverlapRectangleAll(rect, null);
        // foreach (var hit in hits)
        // {
        //     var player = hit.Entity.GetComponent<MadelinePlayer>();
        //     if (player != null) return player;
        // }

        // Check using scene entities
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player != null)
        {
            var playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null && rect.Intersects(playerCollider.Bounds))
                return player;
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Flag helpers
    // -------------------------------------------------------------------------

    private string FlagName => $"dashSwitch_{Id}";

    private bool CheckFlag()
    {
        // TODO: return GameState.Instance.GetFlag(FlagName)
        return false;
    }

    private void SetFlag()
    {
        // TODO: GameState.Instance.SetFlag(FlagName, true)
    }

    private void MoveTowardsY(float targetY, float maxMove)
    {
        if (MathF.Abs(Position.Y - targetY) <= maxMove)
        {
            Position = new Vector2(Position.X, targetY);
        }
        else if (Position.Y > targetY)
        {
            Position = new Vector2(Position.X, Position.Y - maxMove);
        }
        else
        {
            Position = new Vector2(Position.X, Position.Y + maxMove);
        }
    }
}

/// <summary>
/// Result of a dash collision.
/// </summary>
public enum DashCollisionResult
{
    NormalCollision,
    Ignore
}
