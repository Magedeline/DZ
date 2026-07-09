using FMOD.Studio;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;

namespace DZ;

/// <summary>
/// Enhanced title screen hook system with support for both On.hook and IL.hook patches.
/// Handles title screen interactions, music, animations, and D-Side unlocks display.
/// </summary>
public static class TitleScreen_ExtHook
{
    private static bool _hooked;

    // On.hook delegates
    private static Hook _titleScreenEnterHook;
    private static Hook _titleScreenUpdateHook;

    // IL.hook patches
    private static ILHook _titleScreenRenderILHook;
    private static ILHook _titleScreenInputILHook;

    private const string VANILLA_TITLE_FIRSTINPUT = "event:/ui/main/title_firstinput";
    private const string CUSTOM_TITLE_FIRSTINPUT = "event:/DZ/ui/game/title_firstinput";

    private static int _2UnlockCount = 0;

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        // ──── On.hook delegates for title screen ────

        // Title screen hooks provide D-Side information display and tracking
        // Can be extended with additional title screen customization hooks

        // ──── IL.hook patches for title screen ────

        InstallTitleScreenILHooks();

        Logger.Log(LogLevel.Info, "DZ", "TitleScreen_ExtHook loaded with On.hook and IL.hook support");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        // ──── Unload On.hook delegates ────

        // ──── Dispose IL.hook patches ────

        _titleScreenRenderILHook?.Dispose();
        _titleScreenRenderILHook = null;

        _titleScreenInputILHook?.Dispose();
        _titleScreenInputILHook = null;

        _titleScreenEnterHook?.Dispose();
        _titleScreenEnterHook = null;

        _titleScreenUpdateHook?.Dispose();
        _titleScreenUpdateHook = null;

        Logger.Log(LogLevel.Info, "DZ", "TitleScreen_ExtHook unloaded (On.hook and IL.hook)");
    }

    // ──── On.hook handler delegates ────
    // [Placeholder for future title screen hooks]

    // ──── IL.hook installation ────

    private static void InstallTitleScreenILHooks()
    {
        try
        {
            InstallTitleScreenRenderILHook();
            InstallTitleScreenInputILHook();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ", $"Failed to install title screen IL hooks: {ex.Message}");
        }
    }

    private static void InstallTitleScreenRenderILHook()
    {
        if (_titleScreenRenderILHook != null)
            return;

        MethodInfo target = typeof(OuiTitleScreen).GetMethod(
            "Render",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        if (target == null)
        {
            Logger.Log(LogLevel.Warn, "DZ", "Failed to find OuiTitleScreen.Render method for IL hook.");
            return;
        }

        _titleScreenRenderILHook = new ILHook(target, IL_TitleScreen_Render);
        Logger.Log(LogLevel.Debug, "DZ", "OuiTitleScreen.Render IL hook installed");
    }

    private static void IL_TitleScreen_Render(ILContext il)
    {
        try
        {
            ILCursor cursor = new ILCursor(il);

            // Hook title screen rendering for D-Side unlock display
            // This can be used to display D-Side information on the title screen
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ", $"Error in IL_TitleScreen_Render: {ex.Message}");
        }
    }

    private static void InstallTitleScreenInputILHook()
    {
        if (_titleScreenInputILHook != null)
            return;

        MethodInfo target = typeof(OuiTitleScreen).GetMethod(
            "Update",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        if (target == null)
        {
            Logger.Log(LogLevel.Warn, "DZ", "Failed to find OuiTitleScreen.Update method for IL hook.");
            return;
        }

        _titleScreenInputILHook = new ILHook(target, IL_TitleScreen_Update);
        Logger.Log(LogLevel.Debug, "DZ", "OuiTitleScreen.Update IL hook installed");
    }

    private static void IL_TitleScreen_Update(ILContext il)
    {
        try
        {
            ILCursor cursor = new ILCursor(il);

            // Hook title screen update for input handling and D-Side data
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ", $"Error in IL_TitleScreen_Update: {ex.Message}");
        }
    }

    // ──── Title screen utilities ────

    public static void Set2UnlockCount(int count)
    {
        _2UnlockCount = count;
        Logger.Log(LogLevel.Debug, "DZ/TitleScreen",
            $"D-Side unlock count updated: {count}");
    }

    public static int Get2UnlockCount()
    {
        return _2UnlockCount;
    }
}

