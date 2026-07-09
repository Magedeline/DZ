#nullable enable
using Celeste;

namespace DZ;

/// <summary>
/// Makes dialog portrait expressions whose name contains "glitch"/"glitching"/"glitchy"
/// (e.g. <c>[ASRIEL_KID right glitch]</c>, <c>[ASRIEL_ZERO left glitching]</c>,
/// <c>[DIGITAL_MERCHANT left glitchy]</c>) render with Celeste's native portrait
/// glitchy noise overlay.
///
/// Celeste's <see cref="Celeste.FancyText.Portrait"/> only sets its <c>Glitchy</c> flag
/// from the sprite XML's <c>glitchy</c> attribute — never from the dialog expression
/// name. That means dialog lines that ask for a "glitch" expression get no glitch
/// overlay at all. This hook bridges that gap so the dialog.txt references are
/// compatible without requiring per-expression glitch art.
///
/// When a glitch expression has no dedicated portrait animation defined, the
/// expression falls back to "normal" so a portrait still shows underneath the
/// glitch overlay (forward-compatible: once base "normal" art is added for a
/// character, the glitch dialog references light up automatically).
/// </summary>
public static class GlitchPortraitHooks
{
    private static bool _hooked;

    // ── Public API ────────────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.FancyText.Parse_string_int_int_float_Nullable1_Language += OnFancyTextParse;
        Logger.Log(LogLevel.Info, "DZ", "GlitchPortraitHooks loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.FancyText.Parse_string_int_int_float_Nullable1_Language -= OnFancyTextParse;
        Logger.Log(LogLevel.Info, "DZ", "GlitchPortraitHooks unloaded");
    }

    // ── Hook ──────────────────────────────────────────────────────────────

    private static Celeste.FancyText.Text OnFancyTextParse(
        On.Celeste.FancyText.orig_Parse_string_int_int_float_Nullable1_Language orig,
        string text, int maxLineWidth, int linesPerPage,
        float startFade, Color? defaultColor, Language language)
    {
        Celeste.FancyText.Text result = orig(text, maxLineWidth, linesPerPage, startFade, defaultColor, language);
        if (result?.Nodes == null)
            return result!;

        foreach (Celeste.FancyText.Node node in result.Nodes)
        {
            if (node is Celeste.FancyText.Portrait portrait)
                PatchGlitchPortrait(portrait);
        }

        return result;
    }

    // ── Glitch expression handling ────────────────────────────────────────

    private static void PatchGlitchPortrait(Celeste.FancyText.Portrait portrait)
    {
        string animation = portrait.Animation;
        if (string.IsNullOrEmpty(animation))
            return;

        // Only touch expressions that look like glitch variants.
        if (animation.IndexOf("glitch", StringComparison.OrdinalIgnoreCase) < 0)
            return;

        // Enable Celeste's native portrait glitchy noise overlay. The Textbox
        // renders this as a scrolling noise texture over the portrait sprite.
        portrait.Glitchy = true;

        // If the portrait sprite has a dedicated animation for this glitch
        // expression (e.g. Chara's idle_glitchycreepy), keep it as-is so the
        // authored frames play. Otherwise fall back to "normal" so a portrait
        // still renders under the overlay instead of stalling on the previous
        // animation.
        string spriteId = portrait.SpriteId;
        var bank = GFX.PortraitsSpriteBank;
        if (bank.Has(spriteId))
        {
            string idleAnim = portrait.IdleAnimation; // "idle_" + animation
            var template = bank.SpriteData[spriteId].Sprite;
            if (template.Has(idleAnim))
                return; // dedicated glitch animation exists — keep it
        }

        portrait.Animation = "normal";
    }
}
