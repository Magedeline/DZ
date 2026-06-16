using System;
using Monocle;

namespace Celeste.Mod.DZ.HotReload
{
    /// <summary>
    /// Test class to verify hot reload in the mod's DLL.
    /// </summary>
    [HotReloadable]
    public static class ModHotReloadTest
    {
        public static void OnReload()
        {
            Logger.Log(LogLevel.Info, "DZ", "[HotReload] Mod assembly reloaded successfully!");
        }
    }
}

namespace Celeste.HotReload
{
    /// <summary>
    /// Test class to verify hot reload in the game's DLL (mimicked).
    /// </summary>
    [HotReloadable]
    public static class GameHotReloadTest
    {
        public static void OnReload()
        {
            Logger.Log(LogLevel.Info, "DZ", "[HotReload] Celeste.dll (simulated/modded) reloaded successfully!");
        }
    }
}
