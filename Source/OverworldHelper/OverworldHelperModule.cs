using MonoMod.ModInterop;

namespace Celeste.Mod.DZ;

public static class OverworldHelperModule {
    public static void Load() {
        OverworldTracker.Initialize();
        typeof(OverworldHelperExports).ModInterop();
    }

    public static void Unload()
    {
        OverworldTracker.Unload();
    }
}