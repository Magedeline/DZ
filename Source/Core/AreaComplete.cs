using global::Celeste.Mod.Meta;
using global::Celeste.Mod.MaggyHelper;

namespace Celeste;

/// <summary>
/// MonoMod patch for AreaComplete: extends GetCustomCompleteScreenTitle with DSide support.
/// </summary>
internal class patch_AreaComplete : AreaComplete
{
    private Session Session;

    private patch_AreaComplete(Session session, System.Xml.XmlElement xml, Monocle.Atlas atlas, HiresSnow snow, MapMetaCompleteScreen meta)
        : base(session, xml, atlas, snow, meta) { }

    private string GetCustomCompleteScreenTitle()
    {
        MapMetaCompleteScreenTitle titleMeta = AreaData.Get(Session.Area)?.Meta?.CompleteScreen?.Title;
        if (titleMeta == null)
            return null;

        int modeIndex = (int)Session.Area.Mode;
        switch (modeIndex)
        {
            case AreaModeExtender.MODE_NORMAL:
            {
                string text = Session.FullClear ? titleMeta.FullClear : titleMeta.ASide;
                return text == null ? null : Dialog.Clean(text);
            }
            case AreaModeExtender.MODE_BSIDE:
                return titleMeta.BSide == null ? null : Dialog.Clean(titleMeta.BSide);
            case AreaModeExtender.MODE_CSIDE:
                return titleMeta.CSide == null ? null : Dialog.Clean(titleMeta.CSide);
            case AreaModeExtender.MODE_DSIDE:
                return AreaCompleteExtData.GetCustomCompleteScreenTitle(Session);
            default:
                return null;
        }
    }
}
