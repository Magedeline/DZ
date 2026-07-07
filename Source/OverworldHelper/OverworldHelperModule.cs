using System;
using System.Collections.Generic;
using MonoMod.ModInterop;

namespace Celeste.Mod.DZ;

public static class OverworldHelperModule {
    public static bool Enabled => DZModule.Settings.OverworldHelperEnabled;
    
    public static OverworldTracker Tracker;

    public static void Load() {
        if (Enabled) Tracker = new OverworldTracker();
        typeof(OverworldHelperExports).ModInterop();
    }

    public static void Unload()
    {
        Tracker?.Unload();
        Tracker = null;
    }
}