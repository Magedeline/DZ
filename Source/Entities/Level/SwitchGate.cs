using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using DZ.Entities.Core;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's SwitchGate.cs.
///
/// A gate that opens when all TouchSwitches in the level are activated.
/// The gate moves to a target position when the switches are completed.
/// </summary>
public class SwitchGate : CelesteSolid
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private static readonly Color InactiveColor = new Color(0x5F, 0xCD, 0xE4);
    private static readonly Color ActiveColor = Color.White;
    private static readonly Color FinishColor = new Color(0xF1, 0x41, 0xDF);

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _targetNode;
    private bool _persistent;
    private string _spriteName;
    private float _iconRate;
    private Color _iconColor;
    private float _wiggleValue;
    private bool _moving;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public SwitchGate(Vector2 position, float width, float height, Vector2 targetNode, bool persistent, string spriteName)
        : base(position, width, height, safe: false)
    {
        _targetNode = targetNode;
        _persistent = persistent;
        _spriteName = spriteName;
        _iconColor = InactiveColor;
        _iconRate = 0f;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // TODO: load 9-slice texture from "objects/switchgate/{_spriteName}"
        // TODO: create icon sprite at center
        // TODO: set up light occlude

        // Check if already finished
        if (Switch.Check(Scene) || (_persistent && CheckLevelFlag()))
        {
            // Already completed - move to target immediately
            Position = _targetNode;
            _iconRate = 0f;
            _iconColor = FinishColor;
        }
        else
        {
            // Start sequence to wait for switches
            // TODO: Add(new Coroutine(Sequence()));
        }
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator Sequence()
    {
        Vector2 start = Position;

        // Wait for all switches to be activated
        while (!Switch.Check(Scene))
            yield return null;

        if (_persistent)
        {
            SetLevelFlag();
        }

        yield return 0.1f;

        // TODO: play sound: event:/game/general/touchswitch_gate_open
        // TODO: start shaking

        // Spin up icon animation
        while (_iconRate < 1f)
        {
            _iconColor = Color.Lerp(InactiveColor, ActiveColor, _iconRate);
            _iconRate += Time.DeltaTime * 2f;
            yield return null;
        }

        yield return 0.1f;

        // Move to target position
        _moving = true;
        int particleAt = 0;

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.DeltaTime;
            float t = Ease.CubeOut(elapsed / duration);
            Position = Vector2.Lerp(start, _targetNode, t);

            // Emit particles periodically
            if ((int)(Time.TotalTime * 10) % 2 == particleAt)
            {
                // TODO: emit dust particles behind the gate
            }

            particleAt = (particleAt + 1) % 2;
            yield return null;
        }

        Position = _targetNode;

        // Disable collision temporarily for dust effect
        bool wasCollidable = Collidable;
        Collidable = false;

        // Emit dust at edges
        EmitEdgeDust(start, _targetNode);

        Collidable = wasCollidable;

        // TODO: play finish sound
        // TODO: shake

        // Slow down icon animation
        while (_iconRate > 0f)
        {
            _iconRate -= Time.DeltaTime * 2f;
            if (_iconRate <= 0.1f)
            {
                _iconRate = 0.1f;
                // TODO: wiggler start
                // TODO: emit displacement burst
                break;
            }
            yield return null;
        }

        _iconColor = FinishColor;
        _moving = false;
    }

    private void EmitEdgeDust(Vector2 from, Vector2 to)
    {
        // Emit dust particles based on movement direction
        if (to.X <= from.X)
        {
            // Moving left - emit dust on left edge
            for (int i = 0; i < Height / 8f; i++)
            {
                Vector2 point = new Vector2(Left - 1f, Top + 4f + i * 8);
                // TODO: emit dust particles
            }
        }

        if (to.X >= from.X)
        {
            // Moving right - emit dust on right edge
            for (int i = 0; i < Height / 8f; i++)
            {
                Vector2 point = new Vector2(Right + 1f, Top + 4f + i * 8);
                // TODO: emit dust particles
            }
        }

        if (to.Y <= from.Y)
        {
            // Moving up - emit dust on top edge
            for (int i = 0; i < Width / 8f; i++)
            {
                Vector2 point = new Vector2(Left + 4f + i * 8, Top - 1f);
                // TODO: emit dust particles
            }
        }

        if (to.Y >= from.Y)
        {
            // Moving down - emit dust on bottom edge
            for (int i = 0; i < Width / 8f; i++)
            {
                Vector2 point = new Vector2(Left + 4f + i * 8, Bottom + 1f);
                // TODO: emit dust particles
            }
        }
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    // TODO: Render() - draw 9-slice texture and icon at center with shake offset
    // (Add a RenderableComponent or override OnRender in a renderer class)

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private bool CheckLevelFlag()
    {
        // TODO: check GameState.Instance.GetFlag("switches_" + levelName)
        return false;
    }

    private void SetLevelFlag()
    {
        // TODO: GameState.Instance.SetFlag("switches_" + levelName, true)
    }

    // -------------------------------------------------------------------------
    // Easing
    // -------------------------------------------------------------------------

    private static class Ease
    {
        public static float CubeOut(float t) => 1f - MathF.Pow(1f - t, 3f);
    }
}
