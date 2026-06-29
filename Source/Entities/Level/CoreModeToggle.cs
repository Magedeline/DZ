using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's CoreModeToggle.cs (Chapter 9 — Core).
///
/// An interactive switch that toggles the level between Hot and Cold core
/// modes.  The current mode is stored in the static
/// <see cref="CurrentMode"/> property so any system can read it without
/// requiring a reference to this entity.
///
/// When the player touches the switch's collider (16 × 24 px, centred) and
/// the cooldown has expired, the mode flips and a flash + freeze effect fires.
///
/// <see cref="IceBlock"/> and other mode-sensitive entities subscribe to
/// <see cref="CurrentMode"/> to enable or disable themselves.
///
/// Sprite loading and audio are TODO.
/// </summary>
public class CoreModeToggle : DZ.Nez.Entity
{
    // ── Core mode enum ────────────────────────────────────────────────────────

    public enum CoreMode { Hot, Cold }

    // ── Global state ──────────────────────────────────────────────────────────

    /// <summary>
    /// The current core mode for the active scene.
    /// Defaulting to Hot (lava-world).
    /// Read by <see cref="IceBlock"/> and other mode-sensitive entities.
    /// </summary>
    public static CoreMode CurrentMode = CoreMode.Hot;

    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float Cooldown       = 1f;
    private const float ColliderW      = 16f;
    private const float ColliderH      = 24f;
    private const float ColliderOffX   = -8f;
    private const float ColliderOffY   = -12f;

    // ── Configuration ─────────────────────────────────────────────────────────

    private readonly bool _onlyFire;
    private readonly bool _onlyIce;
    private readonly bool _persistent;

    // ── State ─────────────────────────────────────────────────────────────────

    private float _cooldownTimer;
    private bool  _iceMode;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="CoreModeToggle"/>.
    /// </summary>
    /// <param name="position">World position (centre of the switch).</param>
    /// <param name="onlyFire">Only usable when currently in Hot mode.</param>
    /// <param name="onlyIce">Only usable when currently in Cold mode.</param>
    /// <param name="persistent">Whether the toggled mode persists across room reloads.</param>
    public CoreModeToggle(Vector2 position, bool onlyFire, bool onlyIce, bool persistent)
    {
        Position    = position;
        _onlyFire   = onlyFire;
        _onlyIce    = onlyIce;
        _persistent = persistent;
        Name        = "CoreModeToggle";
        // TODO: load "coreFlipSwitch" sprite bank entry
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        _iceMode = CurrentMode == CoreMode.Cold;
        // TODO: set sprite to appropriate frame
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        float dt = Time.DeltaTime;

        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;

        if (Scene == null) return;

        // Check for player touching the switch.
        var switchRect = new Microsoft.Xna.Framework.Rectangle(
            (int)(Position.X + ColliderOffX),
            (int)(Position.Y + ColliderOffY),
            (int)ColliderW,
            (int)ColliderH);

        for (int _cti = 0; _cti < Scene.Entities.Count; _cti++)
        {
            var e = Scene.Entities[_cti];
            if (e is not MadelinePlayer player) continue;

            var pb = new Microsoft.Xna.Framework.Rectangle(
                (int)player.Position.X, (int)player.Position.Y,
                (int)player.Width,      (int)player.Height);

            if (switchRect.Intersects(pb))
            {
                OnPlayer(player);
                break;
            }
        }
    }

    // ── Player interaction ────────────────────────────────────────────────────

    private void OnPlayer(MadelinePlayer player)
    {
        if (!IsUsable || _cooldownTimer > 0f) return;

        // Toggle mode.
        CurrentMode = CurrentMode == CoreMode.Cold ? CoreMode.Hot : CoreMode.Cold;
        _iceMode    = CurrentMode == CoreMode.Cold;
        _cooldownTimer = Cooldown;

        // TODO: if _persistent, save to session
        // TODO: Screen flash Color.White * 0.15f
        // TODO: Freeze 0.05 s
        // TODO: Rumble (Medium, Medium)
        // TODO: Play appropriate audio
        // TODO: Animate sprite to new state
    }

    private void OnChangeMode(CoreMode mode)
    {
        _iceMode = mode == CoreMode.Cold;
        // TODO: update sprite
    }

    // ── Usability ─────────────────────────────────────────────────────────────

    private bool IsUsable
    {
        get
        {
            if (_onlyFire && !_iceMode) return false;
            if (_onlyIce  &&  _iceMode) return false;
            return true;
        }
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public void Render()
    {
        // TODO: render sprite
        Color c = _iceMode ? Color.CornflowerBlue : Color.OrangeRed;
        Graphics.Instance.Batcher.DrawRect(
            Position.X + ColliderOffX,
            Position.Y + ColliderOffY,
            ColliderW, ColliderH,
            c * 0.6f);
    }
}
