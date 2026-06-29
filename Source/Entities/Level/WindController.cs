using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's WindController.cs.
///
/// Controls the wind speed in the current room by driving a target speed that
/// is smoothly approached via <see cref="Accel"/> px/s².  Supports a set of
/// named wind patterns (constant, on/off cycling, alternating, etc.).
///
/// Each frame the controller:
/// <list type="number">
///   <item>Updates the current pattern (cycling patterns update <see cref="Wind"/>
///         via a simple coroutine-like timer state-machine).</item>
///   <item>Smoothly approaches <see cref="Wind"/> toward the current target speed.</item>
///   <item>Calls <see cref="WindMover.Move"/> on every <see cref="WindMover"/>
///         component in the scene.</item>
/// </list>
///
/// The global <see cref="WindDirection"/> property is read by other systems
/// (e.g., particle emitters) that need to know the current wind vector.
/// </summary>
public class WindController : DZ.Nez.Entity
{
    // ── Constants ─────────────────────────────────────────────────────────────

    private const float Weak   =  400f;
    private const float Strong =  800f;
    private const float Crazy  = 1200f;
    private const float Accel  = 1000f;
    private const float Down   =  300f;
    private const float Up     = -400f;
    private const float Space  = -600f;

    // ── Patterns enum ─────────────────────────────────────────────────────────

    public enum Patterns
    {
        None,
        Left,
        Right,
        LeftStrong,
        RightStrong,
        LeftOnOff,
        RightOnOff,
        LeftOnOffFast,
        RightOnOffFast,
        Alternating,
        LeftGemsOnly,
        RightCrazy,
        Down,
        Up,
        Space,
    }

    // ── Global state (read by game systems) ───────────────────────────────────

    /// <summary>
    /// The current wind velocity in pixels/second (scene-global).
    /// Updated each frame by <see cref="WindController"/>.
    /// </summary>
    public static Vector2 WindDirection = Vector2.Zero;

    // ── Instance state ────────────────────────────────────────────────────────

    public Vector2 Wind { get; private set; }

    private readonly Patterns _startPattern;
    private Patterns          _pattern;
    private Vector2           _targetSpeed;
    private bool              _everSetPattern;

    // On/off cycling state.
    private float _cycleTimer;
    private bool  _cycleOn;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="WindController"/>.
    /// </summary>
    /// <param name="pattern">Initial wind pattern.</param>
    public WindController(Patterns pattern)
    {
        _startPattern = pattern;
        Name          = "WindController";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        SetStartPattern();
    }

    // ── API ───────────────────────────────────────────────────────────────────

    public void SetStartPattern()
    {
        if (!_everSetPattern)
            SetPattern(_startPattern);
    }

    public void SetPattern(Patterns pattern)
    {
        if (_pattern == pattern && _everSetPattern) return;

        _everSetPattern = true;
        _pattern        = pattern;
        _cycleTimer     = 0f;
        _cycleOn        = true;

        switch (pattern)
        {
            case Patterns.None:         _targetSpeed = Vector2.Zero;                        break;
            case Patterns.Left:         _targetSpeed = new Vector2(-Weak, 0f);              break;
            case Patterns.Right:        _targetSpeed = new Vector2( Weak, 0f);              break;
            case Patterns.LeftStrong:   _targetSpeed = new Vector2(-Strong, 0f);            break;
            case Patterns.RightStrong:  _targetSpeed = new Vector2( Strong, 0f);            break;
            case Patterns.RightCrazy:   _targetSpeed = new Vector2( Crazy, 0f);             break;
            case Patterns.Down:         _targetSpeed = new Vector2(0f,  Down);              break;
            case Patterns.Up:           _targetSpeed = new Vector2(0f,  Up);                break;
            case Patterns.Space:        _targetSpeed = new Vector2(0f,  Space);             break;
            case Patterns.LeftOnOff:
            case Patterns.LeftOnOffFast:
            case Patterns.RightOnOff:
            case Patterns.RightOnOffFast:
            case Patterns.Alternating:
            case Patterns.LeftGemsOnly:
                _targetSpeed = Vector2.Zero;
                break;
        }
    }

    /// <summary>Immediately snaps the wind to its target (skips the smooth approach).</summary>
    public void SnapWind()
    {
        Wind          = _targetSpeed;
        WindDirection = Wind;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();
        float dt = Time.DeltaTime;

        // Update cycling patterns.
        UpdateCyclingPattern(dt);

        // Smooth-approach wind toward target.
        Wind = Approach(Wind, _targetSpeed, Accel * dt);

        // Publish global wind direction.
        WindDirection = Wind;

        if (Wind == Vector2.Zero || Scene == null) return;

        // Push all WindMover components.
        for (int _wci = 0; _wci < Scene.Entities.Count; _wci++)
        {
            var wm = Scene.Entities[_wci].GetComponent<WindMover>();
            if (wm != null)
                wm.Move(Wind * 0.1f * dt);
        }
    }

    // ── Cycling pattern state machine ─────────────────────────────────────────

    private void UpdateCyclingPattern(float dt)
    {
        float period     = IsFastCycle(_pattern) ? 2f : 3f;
        bool  isOnOff    = IsOnOffPattern(_pattern);
        bool  isAlt      = _pattern == Patterns.Alternating;
        bool  isGemsOnly = _pattern == Patterns.LeftGemsOnly;

        if (isGemsOnly)
        {
            // TODO: check if any gem/seed is collected → enable wind
            // For now treat as None.
            _targetSpeed = Vector2.Zero;
            return;
        }

        if (!isOnOff && !isAlt) return;

        _cycleTimer += dt;
        if (_cycleTimer >= period)
        {
            _cycleTimer -= period;
            _cycleOn = !_cycleOn;
        }

        if (isAlt)
        {
            // Alternating: Left then Right, 3 s each with 2 s rest.
            // Simplified: toggle every `period`.
            _targetSpeed = _cycleOn
                ? new Vector2(-Strong, 0f)
                : new Vector2( Strong, 0f);
        }
        else
        {
            float dir = (_pattern == Patterns.LeftOnOff || _pattern == Patterns.LeftOnOffFast)
                ? -Strong : Strong;
            _targetSpeed = _cycleOn
                ? new Vector2(dir, 0f)
                : Vector2.Zero;
        }
    }

    private static bool IsOnOffPattern(Patterns p) =>
        p == Patterns.LeftOnOff    || p == Patterns.RightOnOff
     || p == Patterns.LeftOnOffFast || p == Patterns.RightOnOffFast;

    private static bool IsFastCycle(Patterns p) =>
        p == Patterns.LeftOnOffFast || p == Patterns.RightOnOffFast;

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Vector2 Approach(Vector2 current, Vector2 target, float maxMove)
    {
        Vector2 diff = target - current;
        float   len  = diff.Length();
        if (len <= maxMove) return target;
        return current + diff / len * maxMove;
    }
}
