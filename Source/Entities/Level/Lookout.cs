using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KirbyCelesteStandalone.Entities.Player;
using Component = Nez.Component;
using Entity = Nez.Entity;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's Lookout.cs.
///
/// Binoculars that the player can use to look around the level.
/// Locks the player in place and allows camera panning.
/// </summary>
public class Lookout : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Whether this is a summit lookout (special behavior).</summary>
    public bool Summit { get; private set; }

    /// <summary>Whether to only allow Y-axis movement.</summary>
    public bool OnlyY { get; private set; }

    /// <summary>List of nodes to track the camera along.</summary>
    public List<Vector2> Nodes { get; private set; } = new();

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool _interacting;
    private string _animPrefix = "";
    private int _nodeIndex;
    private float _nodePercent;
    private Hud? _hud;
    private Vector2 _interactOffset;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Lookout(Vector2 position, bool summit = false, bool onlyY = false, Vector2[]? nodes = null)
    {
        Entity.Position = position;
        // Depth = -8500; // TODO: Depth not available in Nez.Entity
        Summit = summit;
        OnlyY = onlyY;

        if (nodes != null && nodes.Length > 0)
        {
            Nodes = new List<Vector2>(nodes);
        }

        _interactOffset = new Vector2(-0.5f, -20f);
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // 4x4 hitbox
        var collider = Entity.AddComponent(new BoxCollider(-2f, -4f, 4f, 4f));

        // TODO: add talk component
        // TODO: add vertex light with pulse tween
        // TODO: load sprite "lookout"
        // TODO: set up OnFrameChange callback
    }

    public override void OnRemovedFromEntity()
    {
        base.OnRemovedFromEntity();

        if (_interacting)
        {
            var player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
            if (player != null)
            {
                // TODO: player.StateMachine.State = StNormal;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        if (!_interacting) return;

        var player = Entity.Scene?.FindEntitiesWithTag(0).OfType<MadelinePlayer>().FirstOrDefault();
        if (player == null) return;

        // Update HUD if exists
        _hud?.Update();
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    private void Interact(MadelinePlayer player)
    {
        // Determine animation prefix based on player mode
        // TODO: check if playing as Badeline or no backpack
        _animPrefix = "";

        // Start look routine
        // TODO: Add(new Coroutine(LookRoutine(player)));
        _interacting = true;
    }

    public void StopInteracting()
    {
        _interacting = false;
        // TODO: sprite.Play(_animPrefix + "idle");
    }

    // -------------------------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------------------------

    private IEnumerator LookRoutine(MadelinePlayer player)
    {
        // TODO: if (Entity.Scene.Entities.FindFirst<SandwichLava>() is SandwichLava lava)
        //     lava.Waiting = true;

        // Drop held item
        // TODO: if (player.Holding != null) player.Drop();

        // Put player in dummy state
        // TODO: player.StateMachine.State = StDummy;

        // Walk to position
        // yield return player.DummyWalkToExact((int)Entity.X, cancelOnFall: true);

        // Check if we're still in position
        if (MathF.Abs(Entity.Position.X - player.Position.X) > 4f || player.Dead || !player.OnGround())
        {
            if (!player.Dead)
            {
                // TODO: player.StateMachine.State = StNormal;
            }
            yield break;
        }

        // Start looking
        // TODO: play sound: event:/game/general/lookout_use

        // Play look animation
        if (player.Facing > 0)
        {
            // TODO: sprite.Play(_animPrefix + "lookRight");
        }
        else
        {
            // TODO: sprite.Play(_animPrefix + "lookLeft");
        }

        // Hide player
        // TODO: player.Sprite.Visible = player.Hair.Visible = false;

        yield return 0.2f;

        // Create HUD
        _hud = new Hud();
        // TODO: Scene.Add(_hud);
        _hud.TrackMode = Nodes.Count > 0;
        _hud.OnlyY = OnlyY;

        _nodePercent = 0f;
        _nodeIndex = 0;

        // Fade in
        // TODO: play sound: event:/ui/game/lookout_on
        float fadeElapsed = 0f;
        float fadeDuration = 0.5f;

        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.DeltaTime;
            _hud.Easer = fadeElapsed / fadeDuration;
            // TODO: level.ScreenPadding = (int)(Ease.CubeInOut(_hud.Easer) * 16f);
            yield return null;
        }

        _hud.Easer = 1f;

        // Camera control loop
        Vector2 camStart = Vector2.Zero; // TODO: level.Camera.Position;
        Vector2 camStartCenter = camStart + new Vector2(160f, 90f);
        Vector2 speed = Vector2.Zero;
        Vector2 lastDir = Vector2.Zero;

        const float accel = 800f;
        const float maxSpd = 240f;

        while (_interacting)
        {
            // TODO: get aim input
            Vector2 aim = Vector2.Zero; // Input.Aim.Value;
            if (OnlyY) aim.X = 0f;

            // Play sound on direction change
            if (MathF.Sign(aim.X) != MathF.Sign(lastDir.X) ||
                MathF.Sign(aim.Y) != MathF.Sign(lastDir.Y))
            {
                // TODO: play sound: event:/game/general/lookout_move
            }
            lastDir = aim;

            // Update animations based on aim
            // TODO: UpdateLookAnimation(aim);

            if (Nodes.Count == 0)
            {
                // Free camera mode
                // TODO: implement free camera panning
            }
            else
            {
                // Track mode - follow nodes
                // TODO: implement node tracking
            }

            // Check for exit input
            // TODO: if (Input.MenuCancel.Pressed || Input.MenuConfirm.Pressed ||
            //     Input.Dash.Pressed || Input.Jump.Pressed) break;

            yield return null;
        }

        // Fade out
        fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.DeltaTime;
            _hud.Easer = 1f - fadeElapsed / fadeDuration;
            // TODO: level.ScreenPadding = (int)(Ease.CubeInOut(_hud.Easer) * 16f);
            yield return null;
        }

        // Restore player
        // TODO: player.Sprite.Visible = player.Hair.Visible = true;
        // TODO: player.StateMachine.State = StNormal;

        _hud?.Destroy();
        _hud = null;
        StopInteracting();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static class Ease
    {
        public static float CubeInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
    }
}

/// <summary>
/// HUD for the lookout.
/// </summary>
public class Hud : Entity, IUpdatable
{
    public bool TrackMode { get; set; }
    public bool OnlyY { get; set; }
    public float Easer { get; set; }

    public void Update()
    {
        // Update HUD state
    }

    public void Render()
    {
        // Draw HUD elements
        // TODO: draw vignette, track indicators, etc.
    }
}
