using global::Celeste.Mod.Meta;
using global::Celeste.Mod.MaggyHelper;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Per-instance state attached to a vanilla AreaComplete scene via DynamicData.
/// Holds the mod-specific subtitle and button-timer data injected by AreaCompleteHooks.
/// </summary>
public class AreaCompleteExtData
{
    public float ButtonTimerDelay = 2.2f;
    public float ButtonTimerEase;

    public string SubtitleText;
    public float SubtitleEase;
    public float SubtitleDelay = 1.5f;
    public float SubtitleWaveTime;

    public static string GetCustomCompleteScreenTitle(Session session)
    {
        MapMetaCompleteScreenTitle meta = AreaData.Get(session.Area)?.Meta?.CompleteScreen?.Title;
        if (meta == null)
            return null;

        string text = session.Area.Mode switch
        {
            AreaMode.Normal => !session.FullClear ? meta.ASide : meta.FullClear,
            AreaMode.BSide  => meta.BSide,
            AreaMode.CSide  => meta.CSide,
            _               => null,
        };

        return text == null ? null : Dialog.Clean(text);
    }

    public static string GetDefaultCompleteScreenTitle(Session session)
    {
        string modeName = AreaModeExtender.GetModeName((int)session.Area.Mode);
        string fullClearSuffix = session.FullClear ? "_fullclear" : string.Empty;
        return Dialog.Clean($"areacomplete_{modeName}{fullClearSuffix}");
    }

    public static string GetSubtitleDialogKey(Session session)
    {
        if (!AreaModeExtender.TryParseMainSideSID(session.Area.SID, out string baseKey, out _))
            return null;

        string key = $"COMPLETE_SUBTITLE_{baseKey}";
        return Dialog.Has(key) ? key : null;
    }

    public void RenderSubtitle()
    {
        if (SubtitleText == null || !(SubtitleEase > 0f))
            return;

        float alpha = Ease.CubeOut(SubtitleEase);
        Vector2 pos = new Vector2(960f, 280f);
        ProphecyFontRenderer font = MaggyHelperModule.ProphecyFont;
        if (font != null)
        {
            font.DrawWavyOutline(
                SubtitleText, pos, new Vector2(0.5f, 0f),
                1.2f, Color.Gold * alpha, 2f, Color.Black * alpha * 0.8f,
                SubtitleWaveTime, 2f, 3f);
        }
        else
        {
            ActiveFont.DrawOutline(
                SubtitleText, pos, new Vector2(0.5f, 0f),
                Vector2.One * 0.7f, Color.Gold * alpha, 2f, Color.Black * alpha * 0.8f);
        }
    }

    public void RenderButtonHint()
    {
        if (!(ButtonTimerEase > 0f) || Settings.Instance.SpeedrunClock != SpeedrunType.Off)
            return;

        MTexture tex = Input.GuiButton(Input.MenuConfirm, Input.PrefixMode.Latest, null);
        if (tex == null) return;

        Vector2 pos = new Vector2(1860f - tex.Width, 1020f - tex.Height);
        float e = ButtonTimerEase * ButtonTimerEase;
        float scale = 0.9f + ButtonTimerEase * 0.1f;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 && j != 0)
                    tex.DrawCentered(pos + new Vector2(i, j), Color.Black * e * e * e * e, Vector2.One * scale);
            }
        }
        tex.DrawCentered(pos, Color.White * e, Vector2.One * scale);
    }
}

