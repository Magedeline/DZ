using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;
using KirbyCelesteStandalone.Entities.Collectibles;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's TempleGate.cs.
///
/// A vertical gate that opens and closes based on various triggers:
/// - NearestSwitch: opens when a nearby DashSwitch is hit
/// - CloseBehindPlayer: closes when player passes through
/// - HoldingTheo: opens when TheoCrystal is nearby
/// - TouchSwitches: opens when all TouchSwitches are activated
/// </summary>
public class TempleGate : CelesteSolid
{
    // -------------------------------------------------------------------------
    // Enum
    // -------------------------------------------------------------------------

    public enum Types
    {
        NearestSwitch,
        CloseBehindPlayer,
        CloseBehindPlayerAlways,
        CloseBehindPlayerAndTheo,
        HoldingTheo,
        TouchSwitches
    }

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>The type of trigger that controls this gate.</summary>
    public Types GateType { get; private set; }

    /// <summary>The level ID this gate belongs to.</summary>
    public string LevelId { get; private set; }

    /// <summary>Whether this gate has been claimed by a DashSwitch.</summary>
    public bool ClaimedByASwitch { get; set; }

    /// <summary>Whether the gate is currently open.</summary>
    public bool IsOpen => _open;

    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const int OpenHeight = 0;
    private const float HoldingWaitTime = 0.2f;
    private const float HoldingOpenDistSq = 4096f; // 64^2
    private const float HoldingCloseDistSq = 6400f; // 80^2
    private const int MinDrawHeight = 4;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool _theoGate;
    private int _closedHeight;
    private float _drawHeight;
    private float _drawHeightMoveSpeed;
    private bool _open;
    private float _holdingWaitTimer = HoldingWaitTime;
    private Vector2 _holdingCheckFrom;
    private bool _lockState;
    private string _spriteName;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public TempleGate(Vector2 position, int height, Types type, string spriteName, string levelId)
        : base(position, 8f, Math.Max(height, 64), safe: true)
    {
        GateType = type;
        _closedHeight = height;
        LevelId = levelId;
        _spriteName = spriteName;
        _theoGate = spriteName.Equals("theo", StringComparison.InvariantCultureIgnoreCase);
        _holdingCheckFrom = Position + new Vector2(Width / 2f, height / 2f);

        Depth = -9000;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // TODO: load sprite "templegate_{_spriteName}"
        // TODO: position sprite
        // TODO: play idle animation

        // Set up initial state based on type
        SetupInitialState();

        _drawHeight = MathF.Max(MinDrawHeight, Height);
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    private void SetupInitialState()
    {
        switch (GateType)
        {
            case Types.CloseBehindPlayer:
                var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
                if (player != null && player.Position.X < Right &&
                    player.Position.Y >= Top && player.Position.Y <= Bottom)
                {
                    StartOpen();
                    // TODO: Add(new Coroutine(CloseBehindPlayer()));
                }
                break;

            case Types.CloseBehindPlayerAlways:
                StartOpen();
                // TODO: Add(new Coroutine(CloseBehindPlayer()));
                break;

            case Types.CloseBehindPlayerAndTheo:
                StartOpen();
                // TODO: Add(new Coroutine(CloseBehindPlayerAndTheo()));
                break;

            case Types.HoldingTheo:
                if (TheoIsNearby())
                    StartOpen();
                // Make wider for holding detection
                Collider.Width = 16f;
                break;

            case Types.TouchSwitches:
                // TODO: Add(new Coroutine(CheckTouchSwitches()));
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        if (GateType == Types.HoldingTheo)
        {
            if (_holdingWaitTimer > 0f)
            {
                _holdingWaitTimer -= Time.DeltaTime;
            }
            else if (!_lockState)
            {
                bool theoNearby = TheoIsNearby();

                if (_open && !theoNearby)
                {
                    Close();
                    // Kill player if they're in the way
                    var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
                    if (player != null && Collider.Collides(player.Collider))
                    {
                        // TODO: player.Die(Vector2.Zero);
                    }
                }
                else if (!_open && theoNearby)
                {
                    Open();
                }
            }
        }

        // Smooth height animation
        float targetHeight = MathF.Max(MinDrawHeight, Height);
        if (_drawHeight != targetHeight)
        {
            _lockState = true;
            _drawHeight = Calc.Approach(_drawHeight, targetHeight, _drawHeightMoveSpeed * Time.DeltaTime);
        }
        else
        {
            _lockState = false;
        }
    }

    // -------------------------------------------------------------------------
    // Open/Close
    // -------------------------------------------------------------------------

    /// <summary>
    /// Opens the gate.
    /// </summary>
    public void Open()
    {
        // TODO: play sound: _theoGate ? event:/game/05_mirror_temple/gate_theo_open : event:/game/05_mirror_temple/gate_main_open

        _holdingWaitTimer = HoldingWaitTime;
        _drawHeightMoveSpeed = 200f;
        _drawHeight = Height;
        // TODO: shake

        SetHeight(OpenHeight);
        // TODO: sprite.Play("open");
        _open = true;
    }

    /// <summary>
    /// Closes the gate.
    /// </summary>
    public void Close()
    {
        // TODO: play sound: _theoGate ? event:/game/05_mirror_temple/gate_theo_close : event:/game/05_mirror_temple/gate_main_close

        _holdingWaitTimer = HoldingWaitTime;
        _drawHeightMoveSpeed = 300f;
        _drawHeight = MathF.Max(MinDrawHeight, Height);
        // TODO: shake

        SetHeight(_closedHeight);
        // TODO: sprite.Play("hit");
        _open = false;
    }

    /// <summary>
    /// Starts the gate in the open position.
    /// </summary>
    public void StartOpen()
    {
        SetHeight(OpenHeight);
        _drawHeight = MinDrawHeight;
        _open = true;
    }

    /// <summary>
    /// Switches the gate open (from DashSwitch).
    /// </summary>
    public void SwitchOpen()
    {
        // TODO: sprite.Play("open");
        // TODO: Add(Alarm.Set(() =>
        // {
        //     // TODO: shake
        //     // TODO: Add(Alarm.Set(() => Open(), 0.2f));
        // }, 0.2f));
    }

    private void SetHeight(int height)
    {
        if (height < Collider.Height)
        {
            Collider.Height = height;
        }
        else
        {
            float y = Position.Y;
            int oldHeight = (int)Collider.Height;

            if (Collider.Height < 64f)
            {
                Position = new Vector2(Position.X, Position.Y - (64f - Collider.Height));
                Collider.Height = 64f;
            }

            MoveVExact(height - oldHeight);
            Position = new Vector2(Position.X, y);
            Collider.Height = height;
        }
    }

    // -------------------------------------------------------------------------
    // Detection
    // -------------------------------------------------------------------------

    private bool TheoIsNearby()
    {
        var theo = Scene?.FindEntitiesWithTag(0).OfType<TheoCrystal>().FirstOrDefault();
        if (theo == null) return true; // No Theo, treat as nearby

        if (theo.Position.X > Position.X + 10f)
            return true; // Theo is past the gate

        float distSq = Vector2.DistanceSquared(_holdingCheckFrom, theo.Position);
        return distSq < (_open ? HoldingCloseDistSq : HoldingOpenDistSq);
    }

    public bool CloseBehindPlayerCheck()
    {
        var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        return player != null && player.Position.X < Position.X;
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator CloseBehindPlayer()
    {
        while (true)
        {
            if (_lockState)
            {
                yield return null;
                continue;
            }

            var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
            if (player != null && player.Position.X > Right + 4f)
            {
                Close();
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator CloseBehindPlayerAndTheo()
    {
        while (true)
        {
            var player = Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
            if (player != null && player.Position.X > Right + 4f)
            {
                var theo = Scene?.FindEntitiesWithTag(0).OfType<TheoCrystal>().FirstOrDefault();
                if (theo != null && theo.Position.X > Right + 4f && !_lockState)
                {
                    Close();
                    yield break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator CheckTouchSwitches()
    {
        while (!Switch.Check(Scene))
            yield return null;

        // TODO: sprite.Play("open");
        yield return 0.5f;
        // TODO: shake
        yield return 0.2f;

        while (_lockState)
            yield return null;

        Open();
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    // TODO: Render via RenderableComponent
}
