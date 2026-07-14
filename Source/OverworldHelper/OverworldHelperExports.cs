using System;
using Celeste.Mod.Meta;
using MonoMod.ModInterop;

namespace Celeste.Mod.DZ;

[ModExportName("OverworldHelper")]
public static class OverworldHelperExports
{
    // AreaChanged
    public static void SubscribeToAreaChanged(Action<AreaKey> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.AreaChanged += callback;
    }
    public static void UnsubscribeFromAreaChanged(Action<AreaKey> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.AreaChanged -= callback;
    }

    // AreaChangedID
    public static void SubscribeToAreaChangedID(Action<int> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.AreaChangedID += callback;
    }
    public static void UnsubscribeFromAreaChangedID(Action<int> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.AreaChangedID -= callback;
    }

    // OverworldCreated
    public static void SubscribeToOverworldCreated(Action<Overworld> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.OverworldCreated += callback;
    }
    public static void UnsubscribeFromOverworldCreated(Action<Overworld> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.OverworldCreated -= callback;
    }

    // VanillaOverworldCreated
    public static void SubscribeToVanillaOverworldCreated(Action<Overworld> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.VanillaOverworldCreated += callback;
    }
    public static void UnsubscribeFromVanillaOverworldCreated(Action<Overworld> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.VanillaOverworldCreated -= callback;
    }

    // CustomOverworldCreated
    public static void SubscribeToCustomOverworldCreated(Action<Overworld> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.CustomOverworldCreated += callback;
    }
    public static void UnsubscribeFromCustomOverworldCreated(Action<Overworld> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.CustomOverworldCreated -= callback;
    }

    // TitleScreenEntry
    public static void SubscribeToTitleScreenEntry(Action<OuiTitleScreen> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.TitleScreenEntry += callback;
    }
    public static void UnsubscribeFromTitleScreenEntry(Action<OuiTitleScreen> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.TitleScreenEntry -= callback;
    }

    // TitleScreenExit
    public static void SubscribeToTitleScreenExit(Action<OuiTitleScreen> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.TitleScreenExit += callback;
    }
    public static void UnsubscribeFromTitleScreenExit(Action<OuiTitleScreen> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        OverworldTracker.TitleScreenExit -= callback;
    }

    // Status helpers
    public static Overworld GetOverworld() => OverworldTracker.CurrentOverworld;
    public static bool GetOverworldIsVanilla() => OverworldTracker.OverworldIsVanilla;

    // Area lookup helpers
    public static AreaKey GetAreaKeyFromID(int id) => GetAreaDataFromID(id).ToKey();
    public static AreaData GetAreaDataFromID(int id) => AreaData.Areas[id];
    public static AreaKey? FindAreaKeyFromString(string area) => FindAreaDataFromString(area)?.ToKey();
    public static AreaData FindAreaDataFromString(string area) => AreaData.Areas.Find(a => a.SID == area);

    // Config getters by area ID
    public static MapMeta GetConfigFromAreaID(int areaID, Type type) => CustomConfig.GetConfig(AreaData.Areas[areaID].ToKey(), type);
    public static MapMeta GetConfigFromAreaKey(AreaKey area, Type type) => CustomConfig.GetConfig(area, type);
    public static MapMeta GetConfigFromAreaData(AreaData area, Type type) => CustomConfig.GetConfig(area.ToKey(), type);
    public static MapMeta FindConfigFromString(string area, Type type) => CustomConfig.GetConfig(area, type);

    // backwards compatibility
    public static MapMeta GetConfigFromArea(AreaKey area, Type type) => GetConfigFromAreaKey(area, type);
    public static MapMeta GetConfig(AreaKey area, Type type) => GetConfigFromAreaKey(area, type);
    public static MapMeta GetConfigFromString(string area, Type type) => FindConfigFromString(area, type);
}