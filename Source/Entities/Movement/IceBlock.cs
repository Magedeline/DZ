using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Core;
using KirbyCelesteStandalone.Entities.Level;
using LavaRect = KirbyCelesteStandalone.Entities.Level.LavaRect;

namespace KirbyCelesteStandalone.Entities.Movement;

/// <summary>
/// Port of Celeste's IceBlock.cs.
///
/// An ice-themed block that is only active in Cold core mode
/// (see <see cref="CoreModeToggle.CurrentMode"/>).
///
/// It contains an inner <see cref="CelesteSolid"/> slightly inset from the
/// ice-block visual bounds, plus a <see cref="LavaRect"/> visual effect
/// configured with ice colours.
///
/// When core mode switches away from Cold the block (and its inner solid)
/// become non-collidable and emit deactivation particles.
/// </summary>
public class IceBlock : Nez.Entity
{
    // ── Ice colours ───────────────────────────────────────────────────────────

    private static readonly Color SurfaceColor = HexToColor("a6fff4");
    private static readonly Color EdgeColor    = HexToColor("6cd6eb");
    private static readonly Color CenterColor  = HexToColor("4ca8d6");

    // ── Children ──────────────────────────────────────────────────────────────

    private readonly LavaRect    _lava;
    private CelesteSolid?        _solid;

    // ── Dimensions ────────────────────────────────────────────────────────────

    private readonly float _width;
    private readonly float _height;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary><c>true</c> when the block is currently active (Cold core mode).</summary>
    public bool Collidable { get; private set; } = true;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>Creates a new <see cref="IceBlock"/>.</summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public IceBlock(Vector2 position, float width, float height)
        : base()
    {
        Position = position;
        _width   = width;
        _height  = height;

        // LavaRect visual — configured with ice palette.
        _lava = new LavaRect(width, height, step: 2)
        {
            UpdateMultiplier  = 0f,
            SurfaceColor      = SurfaceColor,
            EdgeColor         = EdgeColor,
            CenterColor       = CenterColor,
            SmallWaveAmplitude = 1f,
            BigWaveAmplitude   = 1f,
            CurveAmplitude     = 1f,
            Spikey             = 3f,
        };
        Name = "IceBlock";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Inner solid slightly inset.
        _solid = new CelesteSolid(
            Position + new Vector2(2f, 3f),
            _width - 4f,
            _height - 5f,
            safe: false);

        Scene.AddEntity(_solid);

        // Sync collidable state to current core mode.
        bool cold = CoreModeToggle.CurrentMode == CoreModeToggle.CoreMode.Cold;
        SetCollidable(cold);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();
        _lava.Update();

        // React to core mode changes.
        bool cold = CoreModeToggle.CurrentMode == CoreModeToggle.CoreMode.Cold;
        if (cold != Collidable)
            OnChangeMode(cold);

        // Kill player on contact when active.
        if (Collidable && Scene != null)
        {
            for (int _ii = 0; _ii < Scene.Entities.Count; _ii++)
            {
                var e = Scene.Entities[_ii];
                if (e is KirbyCelesteStandalone.Entities.Player.MadelinePlayer player)
                {
                    var bounds = new Microsoft.Xna.Framework.Rectangle(
                        (int)Position.X, (int)Position.Y, (int)_width, (int)_height);
                    var pb = new Microsoft.Xna.Framework.Rectangle(
                        (int)player.Position.X, (int)player.Position.Y,
                        (int)player.Width, (int)player.Height);
                    if (bounds.Intersects(pb))
                    {
                        // TODO: call player.Die(...)
                    }
                }
            }
        }
    }

    // ── Core mode change ──────────────────────────────────────────────────────

    private void OnChangeMode(bool cold)
    {
        SetCollidable(cold);

        if (!cold)
        {
            // TODO: emit P_Deactivate particles around the block
        }
    }

    private void SetCollidable(bool value)
    {
        Collidable = value;
        if (_solid != null)
            _solid.Collidable = value;
    }

    // ── Render ────────────────────────────────────────────────────────────────

    // TODO: Render via RenderableComponent
    // (_lava.Render() should be called from a custom renderer when Collidable is true)

    // ── Colour helper ─────────────────────────────────────────────────────────

    private static Color HexToColor(string hex)
    {
        if (hex.Length == 6)
        {
            int r = System.Convert.ToInt32(hex[0..2], 16);
            int g = System.Convert.ToInt32(hex[2..4], 16);
            int b = System.Convert.ToInt32(hex[4..6], 16);
            return new Color(r, g, b);
        }
        return Color.White;
    }
}
