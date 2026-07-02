namespace DZ;

using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

/// <summary>
/// Lightweight overworld entity that renders a portal marker linking the DZ
/// mountain to the floating island in space (Chapter 19 / "19_Space").
///
/// Design constraints:
/// - The overworld UI (Oui) is NEVER hidden or manipulated by this entity.
///   It only draws an additive marker on top of the scene so the player can
///   always see the chapter select icons and the rest of the UI.
/// - Kept intentionally simple: no cameras, no coroutines, no state machines.
///   It just draws a pulsing portal ring at the Space chapter's icon position
///   and, when enabled, a small DZ marker indicating DZ appears on the
///   DZ mountain after the Chapter 10 (Ruins) unlock animation.
/// </summary>
public class PortalOverworldConnector : Entity
{
    /// <summary>SID of the floating-island-in-space chapter this portal links to.</summary>
    private const string SpaceChapterSid = "19_Space";

    /// <summary>Radius of the portal ring drawn in overworld space.</summary>
    private const float PortalRadius = 22f;

    /// <summary>Offset of the DZ marker relative to the portal center.</summary>
    private static readonly Vector2 DZOffset = new(0f, -46f);

    private float _pulse;
    private float _DZAlpha;
    private bool _DZEnabled;

    public PortalOverworldConnector()
    {
        // Draw above most overworld scenery but below the Oui overlays/UI.
        Depth = -10000;
        Tag = Tags.Global;
    }

    public override void Update()
    {
        base.Update();

        _pulse += Engine.DeltaTime * 2f;

        if (_DZEnabled)
            _DZAlpha = Math.Min(_DZAlpha + Engine.DeltaTime * 3f, 1f);
    }

    public override void Render()
    {
        Vector2? pos = ResolvePortalPosition();
        if (!pos.HasValue)
            return;

        Vector2 center = pos.Value + Position;

        // Pulsing portal ring - purely visual, does not affect the Oui.
        float pulseScale = 1f + (float)Math.Sin(_pulse) * 0.08f;
        Draw.Circle(center, PortalRadius, Color.Cyan * 0.55f, 32);
        Draw.Circle(center, PortalRadius * pulseScale, Color.White * 0.35f, 32);

        if (_DZAlpha > 0f)
        {
            // DZ marker: a small gold dot above the portal indicating
            // DZ appears on the DZ mountain.
            Draw.Circle(center + DZOffset, 6f, Color.Gold * _DZAlpha, 16);
        }
    }

    /// <summary>
    /// Shows the DZ marker on the DZ mountain. Called by ChapterProgressionManager
    /// after the Chapter 10 (Ruins) unlock animation completes.
    /// This does NOT hide the overworld UI - the Oui remains fully visible.
    /// </summary>
    public void EnableDZMarker()
    {
        _DZEnabled = true;
        Logger.Log(LogLevel.Info, "DZ",
            "[PortalOverworldConnector] DZ marker enabled on the DZ mountain.");
    }

    /// <summary>
    /// Resolves the overworld position of the Space chapter icon so the portal
    /// marker tracks the correct spot on the map. Falls back to a default
    /// position if the icon cannot be located (e.g. chapter select not yet entered).
    /// </summary>
    private Vector2? ResolvePortalPosition()
    {
        Overworld overworld = Scene as Overworld;
        if (overworld == null)
            return null;

        OuiChapterSelect chapterSelect = overworld.Entities.FindFirst<OuiChapterSelect>();
        if (chapterSelect == null)
            return null;

        DynamicData dd = new DynamicData(chapterSelect);
        var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");
        if (icons == null)
            return null;

        foreach (var icon in icons)
        {
            if (icon == null)
                continue;

            try
            {
                DynamicData iconData = new DynamicData(icon);
                object areaObj = iconData.Get<object>("area");
                if (areaObj == null)
                    continue;

                int area = Convert.ToInt32(areaObj);
                if (area < 0 || area >= AreaData.Areas.Count)
                    continue;

                var areaData = AreaData.Areas[area];
                if (areaData?.SID == null)
                    continue;

                if (areaData.SID.EndsWith(SpaceChapterSid, StringComparison.OrdinalIgnoreCase))
                    return icon.Position;
            }
            catch
            {
                continue;
            }
        }

        return null;
    }
}
