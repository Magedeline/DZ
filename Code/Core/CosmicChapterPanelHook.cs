using System;
using Celeste.Mod.MaggyHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste;

/// <summary>
/// Two visual effects applied to Maggy chapter panel icons in the overworld:
///
/// 1. <b>Cosmic Kirby tint</b> — crystal heart and mini-heart UI textures drawn
///    inside the chapter panel receive a slowly cycling purple/cyan/pink hue to
///    give a starry, Kirby-inspired look.  This applies even before full mastery.
///
/// 2. <b>Asriel ISO backdrop</b> — when a chapter has achieved
///    <see cref="ChapterMasteryRecord.IsFullMastery"/> the asset at
///    <c>Graphics/Atlases/Gui/MaggyHelper/mastery/asriel_iso_bg.png</c> is
///    rendered behind the chapter panel card, blending in as a cosmic "100%"
///    completion reward.
///
/// <para>
/// Art assets needed (place in <c>Graphics/Atlases/Gui/MaggyHelper/mastery/</c>):
/// <list type="bullet">
///   <item><c>asriel_iso_bg.png</c> — The Asriel isometric backdrop (recommended
///     size 480×272 to match the chapter card bounds).</item>
/// </list>
/// </para>
/// </summary>
public static class CosmicChapterPanelHook
{
    private static bool _hooked;

    // Cosmic cycling speed (radians per second)
    private const float CosmicCycleSpeed = 0.8f;

    // Tint palette anchors (HSV hue positions)
    private static readonly Color[] CosmicPalette =
    {
        Calc.HexToColor("CC88FF"), // lavender
        Calc.HexToColor("66DDFF"), // cyan
        Calc.HexToColor("FF88CC"), // pink
        Calc.HexToColor("AAAAFF"), // periwinkle
    };

    // Mastery bg alpha/scale spring
    private static float _masteryAlpha;
    private static float _masteryAlphaTarget;

    // Per-render timer (accumulated; not tied to a scene clock)
    private static float _cosmicTimer;

    // Atlas path for the cosmic bg sprite
    private const string MasteryBgTexPath = "MaggyHelper/mastery/asriel_iso_bg";

    // ── Lifecycle ───────────────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.OuiChapterPanel.Render += OnChapterPanelRender;
        On.Celeste.OuiChapterPanel.Update += OnChapterPanelUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "CosmicChapterPanelHook loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.OuiChapterPanel.Render -= OnChapterPanelRender;
        On.Celeste.OuiChapterPanel.Update -= OnChapterPanelUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "CosmicChapterPanelHook unloaded");
    }

    // ── Panel update — advance timers ───────────────────────────────────────

    private static void OnChapterPanelUpdate(On.Celeste.OuiChapterPanel.orig_Update orig,
        OuiChapterPanel self)
    {
        orig(self);

        if (!AreaModeExtender.IsOurMap(AreaData.Get(self.Area)))
            return;

        _cosmicTimer += Engine.DeltaTime * CosmicCycleSpeed;

        bool hasMastery = MaggyHelperModule.SaveData?.HasFullMastery(self.Area.SID) ?? false;
        _masteryAlphaTarget = hasMastery ? 1f : 0f;
        _masteryAlpha = Calc.Approach(_masteryAlpha, _masteryAlphaTarget, Engine.DeltaTime * 2.5f);
    }

    // ── Panel render ────────────────────────────────────────────────────────

    private static void OnChapterPanelRender(On.Celeste.OuiChapterPanel.orig_Render orig,
        OuiChapterPanel self)
    {
        bool isOurs = AreaModeExtender.IsOurMap(AreaData.Get(self.Area));

        // Draw mastery backdrop BEFORE the panel so it sits behind the card
        if (isOurs && _masteryAlpha > 0.01f)
            DrawMasteryBackdrop(self);

        // Let vanilla (and CU2) draw the panel normally
        orig(self);

        // Apply cosmic tint overlay on top of the card after vanilla renders
        if (isOurs)
            DrawCosmicOverlay(self);
    }

    // ── Mastery backdrop ────────────────────────────────────────────────────

    private static void DrawMasteryBackdrop(OuiChapterPanel panel)
    {
        // Attempt to load the custom asset; silently skip if not yet present
        MTexture bgTex = null;
        try
        {
            if (GFX.Gui.Has(MasteryBgTexPath))
                bgTex = GFX.Gui[MasteryBgTexPath];
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "CosmicChapterPanelHook", $"Failed to load mastery background texture: {ex.Message}");
        }

        // Card position — the chapter panel sits near screen centre in vanilla.
        // We offset slightly behind/below to create a layered effect.
        Vector2 panelPos = GetPanelPosition(panel);

        if (bgTex != null)
        {
            // Render the Asriel ISO bg centred on the panel with a pulsing alpha
            float pulse = 0.75f + 0.25f * (float)Math.Sin(_cosmicTimer * 1.5f);
            Color tint = Color.White * (_masteryAlpha * pulse);
            bgTex.DrawCentered(panelPos + new Vector2(0f, 4f), tint, 1.05f);
        }
        else
        {
            // Fallback: draw a deep-purple starfield gradient rectangle until the
            // artist supplies the real asset.
            DrawFallbackStarfield(panelPos);
        }

        // Scattered star sparkles around the panel edges
        DrawCosmicStars(panelPos, _masteryAlpha);
    }

    // ── Cosmic overlay (tint on hearts/berries shown in the panel) ──────────

    private static void DrawCosmicOverlay(OuiChapterPanel panel)
    {
        // A soft additive shimmer rectangle over the whole card
        Vector2 panelPos = GetPanelPosition(panel);
        Color shimmer = GetCosmicColor(_cosmicTimer) * 0.12f;

        Draw.Rect(panelPos.X - 240f, panelPos.Y - 136f, 480f, 272f, shimmer);

        // Star particle scatter (lightweight — no particle system needed)
        DrawCosmicStars(panelPos, 0.6f);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static void DrawFallbackStarfield(Vector2 centre)
    {
        // Deep purple base
        Color bg = new Color(20, 0, 40) * _masteryAlpha * 0.85f;
        Draw.Rect(centre.X - 245f, centre.Y - 140f, 490f, 280f, bg);

        // A few static "star" pixels keyed off _cosmicTimer for gentle twinkle
        var rng = new Random(42);
        for (int i = 0; i < 60; i++)
        {
            float sx = centre.X - 240f + rng.Next(480);
            float sy = centre.Y - 136f + rng.Next(272);
            float flicker = 0.4f + 0.6f * (float)Math.Abs(Math.Sin(_cosmicTimer * (0.5f + i * 0.07f)));
            Color sc = Color.Lerp(Color.White, GetCosmicColor(_cosmicTimer + i * 0.3f), 0.4f)
                       * (_masteryAlpha * flicker);
            Draw.Point(new Vector2(sx, sy), sc);
        }
    }

    private static void DrawCosmicStars(Vector2 centre, float alpha)
    {
        // Six drifting sparkle points around the card perimeter
        for (int i = 0; i < 6; i++)
        {
            float angle  = _cosmicTimer * 0.6f + i * (MathF.PI * 2f / 6f);
            float radius = 200f + 20f * (float)Math.Sin(_cosmicTimer + i * 1.3f);
            var pos = centre + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            float size  = 1.5f + (float)Math.Sin(_cosmicTimer * 2f + i) * 0.5f;
            Color color = GetCosmicColor(_cosmicTimer + i * 0.8f) * alpha * 0.9f;
            Draw.Point(pos, color);
            if (size > 1.8f)
                Draw.Point(pos + Vector2.UnitX, color * 0.5f);
        }
    }

    /// <summary>
    /// Smoothly cycles through the <see cref="CosmicPalette"/> using the timer.
    /// </summary>
    private static Color GetCosmicColor(float t)
    {
        float wrapped = t % (MathF.PI * 2f);
        float norm    = wrapped / (MathF.PI * 2f) * CosmicPalette.Length;
        int   idx0    = (int)norm % CosmicPalette.Length;
        int   idx1    = (idx0 + 1) % CosmicPalette.Length;
        float lerp    = norm - (int)norm;
        return Color.Lerp(CosmicPalette[idx0], CosmicPalette[idx1], lerp);
    }

    /// <summary>
    /// Reads the panel's screen position via DynamicData, falling back to
    /// screen centre if the field is unavailable.
    /// </summary>
    private static Vector2 GetPanelPosition(OuiChapterPanel panel)
    {
        var dyn = MonoMod.Utils.DynamicData.For(panel);
        if (dyn.TryGet("position", out object posObj) || dyn.TryGet("Position", out posObj))
        {
            if (posObj is Vector2 v)
                return v;
        }

        return new Vector2(960f, 540f);
    }
}
