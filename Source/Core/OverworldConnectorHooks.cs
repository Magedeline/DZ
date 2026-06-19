using System;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

/// <summary>
/// Spawns the <see cref="OverworldConnector"/> on the overworld scene so the mod's
/// custom mountain visuals (secondary renderer, Maggy3D marker, Void-Moon / Astral
/// Void) actually become live.  Without this hook the <see cref="OverworldConnector"/>
/// is never instantiated and the entire mountain-visual system is dead code.
/// </summary>
public static class OverworldConnectorHooks
{
    private static bool _hooked;

    // World-space points for the primary/secondary mountain junction, derived from
    // the chapter cursor positions in Maps/Maggy/ASide/*.meta.yaml:
    //   - The main mountain's apex is chapter 09_Summit, cursor (0, 10, 15).
    //   - The floating/cosmic tier (chapters 16, 19-21) sits high above at ~y=33.
    // The connection point is the summit (where the two tiers visually meet); the
    // secondary mountain slides in from a lateral offset to the right and slightly
    // above, so the unlock animation has a sensible start position.
    private static readonly Vector3 ConnectionPoint = new Vector3(0f, 10f, 15f);
    private static readonly Vector3 SecondaryStartPoint = new Vector3(8f, 14f, 12f);

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.Overworld.Begin += OnOverworldBegin;
        Logger.Log(LogLevel.Info, "DZ", "[OverworldConnectorHooks] Loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.Overworld.Begin -= OnOverworldBegin;
        Logger.Log(LogLevel.Info, "DZ", "[OverworldConnectorHooks] Unloaded");
    }

    private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
    {
        orig(self);

        // The vanilla OverworldLoader creates the MountainRenderer before Begin runs,
        // so self.Mountain is available here.  Guard against re-adding on re-entry.
        if (self.Mountain == null)
            return;

        if (self.Entities.FindFirst<OverworldConnector>() != null)
            return;

        try
        {
            var connector = new OverworldConnector(self.Mountain, ConnectionPoint, SecondaryStartPoint);
            self.Add(connector);
            Logger.Log(LogLevel.Info, "DZ", "[OverworldConnectorHooks] OverworldConnector added to overworld");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"[OverworldConnectorHooks] Failed to create OverworldConnector: {ex.Message}");
        }
    }
}
