using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using DZ.Entities.Player;
using DZ.Entities.Core;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's TouchSwitch.cs.
///
/// Pressure plate switch that activates when touched by the player, holdables,
/// or seekers. Multiple switches must all be activated to complete a puzzle.
/// </summary>
public class TouchSwitch : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private static readonly Color InactiveColor = new Color(0x5F, 0xCD, 0xE4);
    private static readonly Color ActiveColor = Color.White;
    private static readonly Color FinishColor = new Color(0xF1, 0x41, 0xDF);

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>The Switch component managing activation state.</summary>
    public Switch Switch { get; private set; }

    /// <summary>Whether the switch is currently activated.</summary>
    public bool Activated => Switch?.Activated ?? false;

    /// <summary>Whether all switches in the level are finished.</summary>
    public bool Finished => Switch?.Finished ?? false;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _ease;
    private float _timer;
    private float _pulse = 1f;
    private BoxCollider? _collider;
    private BoxCollider? _featherCollider;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public TouchSwitch(Vector2 position)
    {
        // Will be set when added to entity
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.UpdateOrder = 2000;

        // Create the Switch component
        Switch = Entity.AddComponent(new Switch(groundReset: false));

        // Set up callbacks
        Switch.OnActivate = () =>
        {
            // TODO: play sound
            // TODO: emit particles
            _pulse = 1.5f;
            _timer = 0f;
        };

        Switch.OnFinish = () =>
        {
            _ease = 0f;
        };

        Switch.OnStartFinished = () =>
        {
            _pulse = 1f;
            _timer = 0f;
            _ease = 1f;
        };

        // Main collider
        _collider = Entity.AddComponent(new BoxCollider(-8f, -8f, 16f, 16f));
        _collider.IsTrigger = true;

        // Feather mode collider (larger)
        _featherCollider = Entity.AddComponent(new BoxCollider(-15f, -15f, 30f, 30f));
        _featherCollider.IsTrigger = true;
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        _timer += dt * 8f;

        // Ease towards activated state
        float targetEase = Finished || Activated ? 1f : 0f;
        _ease = Calc.Approach(_ease, targetEase, dt * 2f);

        // Calculate icon color
        Color targetColor = Finished ? FinishColor : (Activated ? ActiveColor : InactiveColor);
        Color iconColor = Color.Lerp(InactiveColor, targetColor, _ease);

        // Pulse effect
        float pulseMult = 0.5f + (MathF.Sin(_timer) + 1f) / 2f * (1f - _ease) * 0.5f + 0.5f * _ease;
        iconColor = new Color(
            (byte)(iconColor.R * pulseMult),
            (byte)(iconColor.G * pulseMult),
            (byte)(iconColor.B * pulseMult),
            iconColor.A);

        // TODO: set sprite color

        // Decay pulse
        _pulse = Calc.Approach(_pulse, 1f, dt * 2f);

        // Check for activation
        if (!Activated && !Finished)
        {
            CheckActivation();
        }

        // Finished state effects
        if (Finished)
        {
            // Slow down animation
            // TODO: if (icon.Rate > 0.1f) icon.Rate -= 2f * dt;

            // Emit fire particles
            if (Entity.Scene != null && (int)(Time.TotalTime * 33) % 3 == 0)
            {
                // TODO: emit fire particles
            }
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void CheckActivation()
    {
        if (_collider == null) return;

        // Check for player
        var player = FindPlayerInCollider(_featherCollider ?? _collider);
        if (player != null)
        {
            TurnOn();
            return;
        }

        // Check for holdables
        // TODO: check for TheoCrystal, Glider in collider

        // Check for seekers
        // TODO: check for Seeker in collider
    }

    private static readonly Collider[] _overlapResults = new Collider[8];

    private MadelinePlayer? FindPlayerInCollider(BoxCollider collider)
    {
        var rect = new RectangleF(
            Entity.Position.X + collider.LocalOffset.X,
            Entity.Position.Y + collider.LocalOffset.Y,
            collider.Width,
            collider.Height);

        int count = DZ.Nez.Physics.OverlapRectangleAll(ref rect, _overlapResults);
        for (int i = 0; i < count; i++)
        {
            var hit = _overlapResults[i];
            if (hit.Entity == Entity) continue;

            var player = hit.Entity.GetComponent<MadelinePlayer>();
            if (player != null) return player;
        }

        return null;
    }

    /// <summary>
    /// Activates this switch.
    /// </summary>
    public void TurnOn()
    {
        if (Activated || Finished) return;

        // TODO: play sound: event:/game/general/touchswitch_any

        if (Switch.Activate())
        {
            // This was the last switch - all are now finished
            // TODO: play sound: event:/game/general/touchswitch_last_oneshot
            // TODO: add cutoff sound source
        }
    }

    // TODO: Render() - draw border and icon via a RenderableComponent
}
