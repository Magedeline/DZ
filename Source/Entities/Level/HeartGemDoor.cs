using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;
using Component = Nez.Component;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's HeartGemDoor.cs.
///
/// A massive door that requires a certain number of heart gems to open.
/// Shows heart gems filling in as the player approaches.
/// </summary>
public class HeartGemDoor : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const string OpenedFlag = "opened_heartgem_door_";

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>The number of heart gems required to open the door.</summary>
    public int Requires { get; private set; }

    /// <summary>The width of the door.</summary>
    public int Size { get; private set; }

    /// <summary>Whether the door is fully opened.</summary>
    public bool Opened { get; private set; }

    /// <summary>Current heart gem counter display.</summary>
    public float Counter { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _spawnPosition;
    private float _openDistance;
    private float _openPercent;
    private KirbyCelesteStandalone.Entities.Core.CelesteSolid? _topSolid;
    private KirbyCelesteStandalone.Entities.Core.CelesteSolid? _botSolid;
    private float _offset;
    private Vector2 _mist;
    private bool _startHidden;
    private float _heartAlpha = 1f;
    private int _heartGems;

    // Particle system
    private DoorParticle[] _particles = new DoorParticle[50];

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public HeartGemDoor(Vector2 position, int requires, int size, float? openDistance = null, bool startHidden = false)
    {
        _spawnPosition = position;
        Requires = requires;
        Size = size;
        _openDistance = openDistance ?? 32f;
        _startHidden = startHidden;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // Initialize particles
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i] = new DoorParticle
            {
                Position = new Vector2(Nez.Random.NextFloat() * Size, Nez.Random.NextFloat() * 1000f),
                Speed = Nez.Random.Range(4, 12),
                Color = Color.White * Nez.Random.Range(0.2f, 0.6f)
            };
        }

        // Create solid colliders (CelesteSolid is an Entity, added directly to scene)
        if (Entity.Scene != null)
        {
            // Top solid (above door)
            _topSolid = new KirbyCelesteStandalone.Entities.Core.CelesteSolid(new Vector2(Entity.Position.X, Entity.Position.Y - 1000f), Size, 1000f, safe: true);
            Entity.Scene?.AddEntity(_topSolid);

            // Bottom solid (below door)
            _botSolid = new KirbyCelesteStandalone.Entities.Core.CelesteSolid(Entity.Position, Size, 1000f, safe: true);
            Entity.Scene?.AddEntity(_botSolid);
        }

        // TODO: set up custom bloom rendering

        // Check if already opened
        if (CheckOpenedFlag())
        {
            Opened = true;
            // Entity.Visible = true; // TODO: not supported in Nez
            _openPercent = 1f;
            Counter = Requires;
            if (_topSolid != null) _topSolid.Position = new Vector2(_topSolid.Position.X, _topSolid.Position.Y - _openDistance);
            if (_botSolid != null) _botSolid.Position = new Vector2(_botSolid.Position.X, _botSolid.Position.Y + _openDistance);
        }
        else
        {
            // Start the sequence
            // TODO: Add(new Coroutine(Routine()));
        }
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator Routine()
    {
        var player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();

        if (_startHidden)
        {
            // Wait for player to approach
            while (player == null || MathF.Abs(player.Position.X - Entity.Position.X - Size / 2f) >= 100f)
            {
                player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
                yield return null;
            }

            // TODO: play sound: event:/new_content/game/10_farewell/heart_door
            // Entity.Visible = true; // TODO: not supported in Nez
            _heartAlpha = 0f;

            // Animate door appearing
            float topTo = _topSolid?.Position.Y ?? 0f;
            float botTo = _botSolid?.Position.Y ?? 0f;
            float topFrom = topTo - 240f;
            float botFrom = botTo - 240f;

            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.DeltaTime;
                float t = Ease.CubeIn(elapsed / duration);

                if (_topSolid != null)
                    _topSolid.Position = new Vector2(_topSolid.Position.X, topFrom + (topTo - topFrom) * t);
                if (_botSolid != null)
                    _botSolid.Position = new Vector2(_botSolid.Position.X, botFrom + (botTo - botFrom) * t);

                // TODO: check for dash blocks and break them
                yield return null;
            }

            // Finalize position
            if (_topSolid != null) _topSolid.Position = new Vector2(_topSolid.Position.X, topTo);
            if (_botSolid != null) _botSolid.Position = new Vector2(_botSolid.Position.X, botTo);

            // Fade in heart icon
            while (_heartAlpha < 1f)
            {
                _heartAlpha = Calc.Approach(_heartAlpha, 1f, Time.DeltaTime * 2f);
                yield return null;
            }

            yield return 0.6f;
        }

        // Main loop - fill hearts as player approaches
        while (!Opened && Counter < Requires)
        {
            player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
            if (player == null)
            {
                yield return null;
                continue;
            }

            // Check if player is near and on the left side
            float distToCenter = MathF.Abs(player.Position.X - (Entity.Position.X + Size / 2f));
            bool playerOnLeft = player.Position.X < Entity.Position.X;

            if (distToCenter < 80f && playerOnLeft)
            {
                // First heart fill sound
                if (Counter == 0f && _heartGems > 0)
                {
                    // TODO: play sound: event:/game/09_core/frontdoor_heartfill
                }

                // Not enough hearts flag
                if (_heartGems < Requires)
                {
                    // TODO: SetFlag("granny_door");
                }

                int prevCounter = (int)Counter;
                int target = Math.Min(_heartGems, Requires);
                Counter = Calc.Approach(Counter, target, Time.DeltaTime * Requires * 0.8f);
                int newCounter = (int)Counter;

                // Play sound when counter ticks up
                if (prevCounter != newCounter)
                {
                    yield return 0.1f;
                    if (Counter < target)
                    {
                        // TODO: play sound: event:/game/09_core/frontdoor_heartfill
                    }
                }
            }
            else
            {
                // Player moved away - decrease counter
                Counter = Calc.Approach(Counter, 0f, Time.DeltaTime * Requires * 0.8f);
            }

            yield return null;
        }

        // Door is opening!
        if (Counter >= Requires)
        {
            Opened = true;
            SetOpenedFlag();

            // TODO: play sound: event:/game/09_core/frontdoor_heartfill_finish
            // TODO: rumble

            float shakeDuration = 0.5f;
            float shakeElapsed = 0f;

            while (shakeElapsed < shakeDuration)
            {
                shakeElapsed += Time.DeltaTime;
                // TODO: shake
                yield return null;
            }

            // Animate door opening
            float duration = 2f;
            float elapsed = 0f;
            float topStart = _topSolid?.Position.Y ?? 0f;
            float botStart = _botSolid?.Position.Y ?? 0f;

            while (elapsed < duration)
            {
                elapsed += Time.DeltaTime;
                float t = elapsed / duration;
                t = t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f; // CubeOut easing
                _openPercent = t;

                float openAmount = _openPercent * _openDistance;

                if (_topSolid != null)
                    _topSolid.Position = new Vector2(_topSolid.Position.X, topStart - openAmount);
                if (_botSolid != null)
                    _botSolid.Position = new Vector2(_botSolid.Position.X, botStart + openAmount);

                // TODO: emit shimmer particles
                yield return null;
            }

            // Final position
            if (_topSolid != null)
                _topSolid.Position = new Vector2(_topSolid.Position.X, topStart - _openDistance);
            if (_botSolid != null)
                _botSolid.Position = new Vector2(_botSolid.Position.X, botStart + _openDistance);
        }
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        // Update mist animation
        _offset -= Time.DeltaTime * 4f;
        if (_offset < 0f) _offset += 32f;

        // Update particles
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i].Position.Y += _particles[i].Speed * Time.DeltaTime;
            if (_particles[i].Position.Y > 1000f)
            {
                _particles[i].Position.Y = 0f;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    // TODO: Render via RenderableComponent

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string GetOpenedFlagName() => OpenedFlag + Requires;

    private bool CheckOpenedFlag()
    {
        // TODO: return GameState.Instance.GetFlag(GetOpenedFlagName())
        return false;
    }

    private void SetOpenedFlag()
    {
        // TODO: GameState.Instance.SetFlag(GetOpenedFlagName(), true)
    }

    private int GetHeartGems()
    {
        // TODO: return GameState.Instance.TotalHeartGems
        return 0;
    }

    // -------------------------------------------------------------------------
    // Inner types
    // -------------------------------------------------------------------------

    private struct DoorParticle
    {
        public Vector2 Position;
        public int Speed;
        public Color Color;
    }

    private static class Ease
    {
        public static float CubeIn(float t) => t * t * t;
        public static float CubeOut(float t) => 1f - MathF.Pow(1f - t, 3f);
    }

    private static class Calc
    {
        public static float Approach(float val, float target, float maxMove)
        {
            return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
        }
    }
}


