using System;
using Celeste.HotReload;
using Monocle;

namespace Celeste.Mod.MaggyHelper.HotReload
{
    /// <summary>
    /// Test class to verify hot reload in the mod's DLL.
    /// </summary>
    [HotReloadable]
    public static class ModHotReloadTest
    {
        public static void OnReload()
        {
            Logger.Log(LogLevel.Info, "MaggyHelper", "[HotReload] Mod assembly reloaded successfully!");
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
            Logger.Log(LogLevel.Info, "MaggyHelper", "[HotReload] Celeste.dll (simulated/modded) reloaded successfully!");
        }
    }
}
