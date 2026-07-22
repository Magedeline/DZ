namespace DZ;

/// <summary>
/// Central registry for all D-Side hook systems.
/// Coordinates loading, unloading, and management of:
/// - D-Side hooks (On.hook, IL.hook)
/// - Music hooks (On.hook, IL.hook)
/// - Title screen hooks (On.hook, IL.hook)
/// - Additional extension hooks
///
/// This provides a single point of control for the entire hook system
/// and makes it easy to enable/disable hooks globally or individually.
/// </summary>
public static class DSideHookRegistry
{
    private static bool _initialized = false;
    private static Dictionary<string, bool> _hookStates = new();

    public enum HookModule
    {
        DSide,
        Music,
        TitleScreen,
        All
    }

    public static void InitializeAll()
    {
        if (_initialized)
            return;

        _initialized = true;

        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            "Initializing D-Side Hook Registry");

        LoadHookModule(HookModule.DSide);
        LoadHookModule(HookModule.Music);
        LoadHookModule(HookModule.TitleScreen);

        LogHookStatus();
    }

    public static void UninitializeAll()
    {
        if (!_initialized)
            return;

        _initialized = false;

        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            "Uninitializing D-Side Hook Registry");

        UnloadHookModule(HookModule.DSide);
        UnloadHookModule(HookModule.Music);
        UnloadHookModule(HookModule.TitleScreen);

        _hookStates.Clear();

        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            "All hooks unloaded");
    }

    public static void LoadHookModule(HookModule module)
    {
        string moduleName = module.ToString();

        if (_hookStates.ContainsKey(moduleName) && _hookStates[moduleName])
        {
            Logger.Log(LogLevel.Debug, "DZ/HookRegistry",
                $"Hook module '{moduleName}' is already loaded");
            return;
        }

        try
        {
            switch (module)
            {
                case HookModule.DSide:
                    Celeste2Hooks.Load();
                    DSideHookImplementation.Initialize();
                    _hookStates[moduleName] = true;
                    Logger.Log(LogLevel.Info, "DZ/HookRegistry",
                        "✓ D-Side hooks loaded (On.hook + IL.hook)");
                    break;

                case HookModule.Music:
                    CelesteMusicHooks.Load();
                    _hookStates[moduleName] = true;
                    Logger.Log(LogLevel.Info, "DZ/HookRegistry",
                        "✓ Music hooks loaded (On.hook + IL.hook)");
                    break;

                case HookModule.TitleScreen:
                    TitleScreen_ExtHook.Load();
                    _hookStates[moduleName] = true;
                    Logger.Log(LogLevel.Info, "DZ/HookRegistry",
                        "✓ Title screen hooks loaded (On.hook + IL.hook)");
                    break;

                case HookModule.All:
                    LoadHookModule(HookModule.DSide);
                    LoadHookModule(HookModule.Music);
                    LoadHookModule(HookModule.TitleScreen);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ/HookRegistry",
                $"Failed to load hook module '{moduleName}': {ex.Message}\n{ex.StackTrace}");
            _hookStates[moduleName] = false;
        }
    }

    public static void UnloadHookModule(HookModule module)
    {
        string moduleName = module.ToString();

        if (_hookStates.ContainsKey(moduleName) && !_hookStates[moduleName])
        {
            Logger.Log(LogLevel.Debug, "DZ/HookRegistry",
                $"Hook module '{moduleName}' is already unloaded");
            return;
        }

        try
        {
            switch (module)
            {
                case HookModule.DSide:
                    DSideHookImplementation.Shutdown();
                    Celeste2Hooks.Unload();
                    _hookStates[moduleName] = false;
                    Logger.Log(LogLevel.Info, "DZ/HookRegistry",
                        "✓ D-Side hooks unloaded");
                    break;

                case HookModule.Music:
                    CelesteMusicHooks.Unload();
                    _hookStates[moduleName] = false;
                    Logger.Log(LogLevel.Info, "DZ/HookRegistry",
                        "✓ Music hooks unloaded");
                    break;

                case HookModule.TitleScreen:
                    TitleScreen_ExtHook.Unload();
                    _hookStates[moduleName] = false;
                    Logger.Log(LogLevel.Info, "DZ/HookRegistry",
                        "✓ Title screen hooks unloaded");
                    break;

                case HookModule.All:
                    UnloadHookModule(HookModule.DSide);
                    UnloadHookModule(HookModule.Music);
                    UnloadHookModule(HookModule.TitleScreen);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ/HookRegistry",
                $"Failed to unload hook module '{moduleName}': {ex.Message}");
        }
    }

    public static bool IsHookModuleLoaded(HookModule module)
    {
        string moduleName = module.ToString();
        return _hookStates.ContainsKey(moduleName) && _hookStates[moduleName];
    }

    public static bool IsInitialized => _initialized;

    private static void LogHookStatus()
    {
        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            "D-Side Hook Registry Status:");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            $"  2 hooks:       {(IsHookModuleLoaded(HookModule.DSide) ? "✓ LOADED" : "✗ UNLOADED")}");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            $"  Music hooks:       {(IsHookModuleLoaded(HookModule.Music) ? "✓ LOADED" : "✗ UNLOADED")}");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            $"  TitleScreen hooks: {(IsHookModuleLoaded(HookModule.TitleScreen) ? "✓ LOADED" : "✗ UNLOADED")}");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry",
            "Supported hook types: On.hook (delegates), IL.hook (IL manipulation)");
    }

    public static Dictionary<string, bool> GetHookStatus()
    {
        return new Dictionary<string, bool>(_hookStates);
    }

    public static void PrintHookInfo()
    {
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "=== D-SIDE HOOK SYSTEM INFO ===");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "HOOK MODULES:");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "  1. Celeste2Hooks");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - On.hook: HeartGem.Collect, Level.LoadLevel, Overworld.Begin");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - IL.hook: HeartGem.Collect, Level.LoadLevel (IL manipulation)");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - Purpose: D-Side tracking, crystal hearts, level initialization");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "  2. CelesteMusicHooks");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - On.hook: Audio.Play (multiple variants), Audio.SetMusicParam, Audio.Stop");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - IL.hook: Audio.Play, Audio.Stop (IL manipulation)");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - Purpose: Music tracking, parameter management, audio bus routing");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "  3. TitleScreen_ExtHook");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - On.hook: Audio.Play, Oui.Begin, Oui.End");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - IL.hook: OuiTitleScreen.Render, OuiTitleScreen.Update");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "     - Purpose: Title screen customization, D-Side unlock display");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "HOOK TYPES:");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "  • On.hook:  Delegate-based hooks for method interception");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "  • IL.hook:  IL manipulation for low-level patching");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "");
        Logger.Log(LogLevel.Info, "DZ/HookRegistry", "=== STATUS ===");
        LogHookStatus();
    }
}
