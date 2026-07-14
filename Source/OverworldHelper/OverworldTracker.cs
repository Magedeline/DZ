using System;
using System.Collections;
using Monocle;

namespace Celeste.Mod.DZ;

public static class OverworldTracker
{
    public static event Action<AreaKey> AreaChanged;
    public static event Action<int> AreaChangedID;
    public static event Action<Overworld> OverworldCreated;
    public static event Action<Overworld> VanillaOverworldCreated;
    public static event Action<Overworld> CustomOverworldCreated;
    public static event Action<OuiTitleScreen> TitleScreenEntry;
    public static event Action<OuiTitleScreen> TitleScreenExit;

    public static Overworld CurrentOverworld;
    public static bool OverworldIsVanilla;

    private static void AttachToNewOverworld(
        On.Celeste.OverworldLoader.orig_LoadThread orig,
        OverworldLoader self
    )
    {
        try { orig(self); } catch (Exception e) { Logger.Log(LogLevel.Warn, "DZ/OverworldTracker", $"orig(self) threw: {e}"); }
        if (self.overworld == null) return;
        CurrentOverworld = self.overworld;
        OverworldCreated?.Invoke(self.overworld);
        OverworldIsVanilla = self.overworld.GetType() == typeof(Overworld);
        (OverworldIsVanilla ? VanillaOverworldCreated : CustomOverworldCreated)?.Invoke(CurrentOverworld);
    }

    private static float AttachToAreaChange(
        On.Celeste.MountainRenderer.orig_EaseCamera_int_MountainCamera_Nullable1_bool_bool orig,
        MountainRenderer self,
        int area,
        MountainCamera transform,
        float? duration = null,
        bool nearTarget = true,
        bool targetRotate = false
    )
    {
        if (area >= 0 && area < AreaData.Areas.Count)
        {
            AreaChanged?.Invoke(AreaData.Areas[area].ToKey());
            AreaChangedID?.Invoke(area);
        }
        return orig(self, area, transform, duration, nearTarget, targetRotate);
    }

    private static IEnumerator AttachToTitleScreenEntry(
        On.Celeste.OuiTitleScreen.orig_Enter orig,
        OuiTitleScreen self,
        Oui from
    )
    {
        TitleScreenEntry?.Invoke(self);
        return orig(self, from);
    }

    private static IEnumerator AttachToTitleScreenExit(
        On.Celeste.OuiTitleScreen.orig_Leave orig,
        OuiTitleScreen self,
        Oui next
    )
    {
        TitleScreenExit?.Invoke(self);
        return orig(self, next);
    }

    public static void Initialize()
    {
        On.Celeste.OverworldLoader.LoadThread += AttachToNewOverworld;
        On.Celeste.MountainRenderer.EaseCamera_int_MountainCamera_Nullable1_bool_bool += AttachToAreaChange;
        On.Celeste.OuiTitleScreen.Enter += AttachToTitleScreenEntry;
        On.Celeste.OuiTitleScreen.Leave += AttachToTitleScreenExit;
    }

    public static void Unload()
    {
        On.Celeste.OuiTitleScreen.Leave -= AttachToTitleScreenExit;
        On.Celeste.OuiTitleScreen.Enter -= AttachToTitleScreenEntry;
        On.Celeste.MountainRenderer.EaseCamera_int_MountainCamera_Nullable1_bool_bool -= AttachToAreaChange;
        On.Celeste.OverworldLoader.LoadThread -= AttachToNewOverworld;
        CurrentOverworld = null;
    }
}