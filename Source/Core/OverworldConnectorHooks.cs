namespace DZ;

using Celeste;
using Monocle;

/// <summary>
/// Installs the <see cref="PortalOverworldConnector"/> into the Overworld scene.
/// Registered/unregistered by <c>DZModule</c> alongside the other hook systems.
///
/// The connector is a lightweight, purely-visual entity that renders a portal
/// marker for the floating-island-in-space chapter. It never hides or manipulates
/// the overworld UI (Oui) - it only draws on top of the scene.
/// </summary>
public static class OverworldConnectorHooks
{
    private static bool _hooked;

    public static void Load()
    {
        if (_hooked)
            return;

        _hooked = true;
        On.Celeste.Overworld.Begin += OnOverworldBegin;
        Logger.Log(LogLevel.Info, "DZ", "[OverworldConnectorHooks] Loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;
        On.Celeste.Overworld.Begin -= OnOverworldBegin;
        Logger.Log(LogLevel.Info, "DZ", "[OverworldConnectorHooks] Unloaded");
    }

    private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
    {
        orig(self);

        // Add the portal connector once per Overworld instance. FindFirst guards
        // against duplicates if Begin fires more than once.
        if (self.Entities.FindFirst<PortalOverworldConnector>() == null)
            self.Add(new PortalOverworldConnector());
    }
}
