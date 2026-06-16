using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DZ;

/// <summary>
/// Holds per-scene extended data for the AreaComplete screen:
/// subtitle text, button hint timing, and render helpers.
/// </summary>
public sealed class AreaCompleteExtData
{
    public string TitleText;

    public float TitleEase;
    public float TitleDelay  = 1.8f;
    public float TitleWaveTime;

    public float ButtonTimerDelay = 2.2f;
    public float ButtonTimerEase;

    // ── Static helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the dialog key for the area-complete subtitle, or null if none.
    /// Handles A/B/C/D sides.
    /// </summary>
    public static string GetSubtitleDialogKey(Session session)
    {
        if (session == null) return null;

        if (!AreaModeExtender.TryParseMainSideSID(session.Area.SID, out string baseKey, out _))
            return null;

        int mode = (int)session.Area.Mode;
        string modeSuffix = mode switch
        {
            AreaModeExtender.MODE_NORMAL => "aside",
            AreaModeExtender.MODE_BSIDE  => "bside",
            AreaModeExtender.MODE_CSIDE  => "cside",
            AreaModeExtender.MODE_DSIDE  => "dside",
            _                            => "dside", // Default to DSide for unknown modes
        };

        string normalised = baseKey.Replace(" ", "_").ToLowerInvariant();
        return $"areacomplete_{normalised}_{modeSuffix}";
    }

    /// <summary>
    /// Returns the title string from the map meta (CompleteScreen.Title), including
    /// DSide support via a "DSide" dialog key convention.
    /// Returns null if no custom meta title is set for this mode.
    /// </summary>
    public static string GetCustomCompleteScreenTitle(Session session)
    {
        if (session == null) return null;

        var titleMeta = AreaData.Get(session.Area)?.Meta?.CompleteScreen?.Title;

        int mode = (int)session.Area.Mode;

        string dialogKey = mode switch
        {
            AreaModeExtender.MODE_NORMAL => session.FullClear ? titleMeta?.FullClear : titleMeta?.ASide,
            AreaModeExtender.MODE_BSIDE  => titleMeta?.BSide,
            AreaModeExtender.MODE_CSIDE  => titleMeta?.CSide,
            AreaModeExtender.MODE_DSIDE  => null, // D-Side uses default behavior, no custom title
            _                            => null,
        };

        if (dialogKey == null) return null;

        if (mode == AreaModeExtender.MODE_DSIDE)
            return Dialog.Has(dialogKey) ? Dialog.Clean(dialogKey) : null;

        return Dialog.Clean(dialogKey);
    }

    /// <summary>
    /// Returns the default (vanilla-style) complete screen title for the given session,
    /// including DSide support.
    /// </summary>
    public static string GetDefaultCompleteScreenTitle(Session session)
    {
        if (session == null) return null;

        int mode = (int)session.Area.Mode;

        string key = mode switch
        {
            AreaModeExtender.MODE_NORMAL => $"areacomplete_Normal{(session.FullClear ? "_fullclear" : "")}",
            AreaModeExtender.MODE_BSIDE  => "areacomplete_BSide",
            AreaModeExtender.MODE_CSIDE  => "areacomplete_CSide",
            AreaModeExtender.MODE_DSIDE  => "areacomplete_DSide",
            _                            => $"areacomplete_DSide",
        };

        if (!Dialog.Has(key)) return null;
        return Dialog.Clean(key);
    }

    // ── Render ────────────────────────────────────────────────────────────

    public void RenderTitle()
    {
        if (TitleEase <= 0f || TitleText == null)
            return;

        float ease  = TitleEase;
        float wave  = TitleWaveTime;
        float alpha = ease * ease;

        Vector2 pos = new Vector2(
            960f,
            280f + (float)Math.Sin(wave * 1.2f) * 4f * ease
        );

        float scaleX = 1f + (1f - Ease.CubeOut(ease)) * 0.15f;
        float scaleY = 1f + (1f - Ease.CubeOut(ease)) * 0.1f;
        Vector2 scale = new Vector2(scaleX, scaleY);

        ActiveFont.DrawOutline(
            TitleText,
            pos,
            new Vector2(0.5f, 0.5f),
            scale * 0.7f,
            Color.White * alpha,
            2f,
            Color.Black * alpha
        );
    }

    public void RenderButtonHint()
    {
        if (ButtonTimerEase <= 0f || Settings.Instance.SpeedrunClock != SpeedrunType.Off)
            return;

        MTexture icon   = Input.GuiButton(Input.MenuConfirm, "controls/keyboard/oemquestion");
        Vector2  pos    = new Vector2(1860f - (float)icon.Width, 1020f - (float)icon.Height);
        float    ease   = ButtonTimerEase;
        float    alpha  = ease * ease;
        float    size   = 0.9f + ease * 0.1f;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 && j != 0)
                    icon.DrawCentered(pos + new Vector2(i, j), Color.Black * alpha * alpha * alpha * alpha, Vector2.One * size);
            }
        }
        icon.DrawCentered(pos, Color.White * alpha, Vector2.One * size);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static string GetDSideTitleKey(Session session)
    {
        if (!AreaModeExtender.TryParseMainSideSID(session.Area.SID, out string baseKey, out _))
            return null;

        string normalised = baseKey.Replace(" ", "_").ToLowerInvariant();
        return $"areacomplete_{normalised}_dside";
    }
}
