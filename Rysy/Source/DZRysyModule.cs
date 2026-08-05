using Rysy.Mods;
using Rysy.Shared;

namespace DZ.RysyPlugin;

/// <summary>
/// Entry point for DZ's Rysy-side plugin.
///
/// This class must exist even though it does nothing. ModRegistry.LoadModule bails out
/// early when an assembly contains no ModModule subclass, and it only assigns
/// ModMeta.PluginAssembly *after* that check. ScriptRegistry scans PluginAssembly for
/// Script subclasses, so without a ModModule here the scripts would silently never load.
/// </summary>
public sealed class DZRysyModule : ModModule {
    public override void Load() {
        base.Load();
        Logger.Log(LogLevel.Info, "DZ Rysy plugin loaded.");
    }
}
