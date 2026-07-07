using System;
using System.Reflection;
using Celeste.Mod.Meta;
using MonoMod.ModInterop;
using MonoMod.Utils;

namespace Celeste.Mod.DZ;

[ModExportName("OverworldHelper")]
public static class OverworldHelperExports
{
    public static void SubscribeToAreaChanged(Action<AreaKey> callback)
    {
        OverworldTracker.AreaChanged += callback;
    }
    public static void UnsubscribeFromAreaChanged(Action<AreaKey> callback)
    {
        OverworldTracker.AreaChanged -= callback;
    }
    public static void SubscribeToOverworldCreated(Action<Overworld> callback)
    {
        OverworldTracker.OverworldCreated += callback;
    }
    public static void UnsubscribeFromOverworldCreated(Action<Overworld> callback)
    {
        OverworldTracker.OverworldCreated -= callback;
    }
    public static Overworld GetOverworld()
    {
        return OverworldHelperModule.Tracker.currentOverworld;
    }
    public static MapMeta GetConfigFromArea(AreaKey area, Type type) => CustomConfig.GetConfig(area, type);

    // backwards compatibility
    public static MapMeta GetConfig(AreaKey area, Type type) => GetConfigFromArea(area, type);

    public static MapMeta GetConfigFromString(string area, Type type) => CustomConfig.GetConfig(area, type);
}