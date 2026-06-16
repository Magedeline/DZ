using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;
using Celeste;
using Celeste.Mod.DZ;

namespace DZ;

/// <summary>
/// Audio hook system to replace vanilla Celeste audio with custom Kirby audio.
/// Intercepts Audio.Play calls and redirects them to custom audio events.
/// </summary>
public static class KirbyAudioHook
{
    private static bool _loaded;
    private static Dictionary<string, string> _audioReplacements = new();
    private static ILHook _audioPlayILHook;

    public static void Load()
    {
        if (_loaded)
            return;

        // Only load if Kirby theme is selected
        var settings = global::Celeste.Mod.DZ.DZModule.Settings;
        if (settings == null || settings.AudioThemeMode != global::Celeste.Mod.DZ.AudioThemeMode.Kirby)
        {
            Logger.Log(LogLevel.Info, "DZ", "KirbyAudioHook skipped - Kirby theme not selected");
            return;
        }

        _loaded = true;

        // Initialize audio replacement mappings
        InitializeAudioMappings();

        // Hook Audio.Play using IL manipulation
        InstallAudioPlayILHook();

        Logger.Log(LogLevel.Info, "DZ", "KirbyAudioHook loaded - vanilla audio will be replaced with custom Kirby audio");
    }

    public static void Unload()
    {
        if (!_loaded)
            return;

        _loaded = false;

        // Dispose IL hook
        _audioPlayILHook?.Dispose();
        _audioPlayILHook = null;

        _audioReplacements.Clear();

        Logger.Log(LogLevel.Info, "DZ", "KirbyAudioHook unloaded");
    }

    private static void InitializeAudioMappings()
    {
        _audioReplacements.Clear();

        // TODO: Add Kirby audio replacement mappings here
        // Example format:
        // _audioReplacements["event:/char/madeline/jump"] = "event:/kirby/char/madeline/jump";

        Logger.Log(LogLevel.Info, "DZ", $"KirbyAudioHook: Loaded {_audioReplacements.Count} audio replacement mappings");
    }

    private static void InstallAudioPlayILHook()
    {
        if (_audioPlayILHook != null)
            return;

        try
        {
            MethodInfo target = typeof(Audio).GetMethod(
                "Play",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null);

            if (target == null)
            {
                Logger.Log(LogLevel.Warn, "DZ", "Failed to find Audio.Play method for IL hook.");
                return;
            }

            _audioPlayILHook = new ILHook(target, IL_Audio_Play);
            Logger.Log(LogLevel.Debug, "DZ", "Audio.Play IL hook installed");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ", $"Failed to install Audio.Play IL hook: {ex.Message}");
        }
    }

    private static void IL_Audio_Play(ILContext il)
    {
        try
        {
            ILCursor cursor = new ILCursor(il);
            
            // Move to the beginning of the method
            cursor.Goto(0);

            // Load the path argument
            cursor.Emit(OpCodes.Ldarg_0);

            // Emit our replacement check
            cursor.EmitDelegate<Func<string, string>>(ReplaceAudioPath);

            // Store the result back to the argument
            cursor.Emit(OpCodes.Starg_S, (byte)0);

            Logger.Log(LogLevel.Debug, "DZ", "Audio.Play IL hook patched successfully");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ", $"Error in IL_Audio_Play: {ex.Message}");
        }
    }

    private static string ReplaceAudioPath(string originalPath)
    {
        if (_audioReplacements.TryGetValue(originalPath, out string replacementPath))
        {
            Logger.Log(LogLevel.Debug, "DZ/KirbyAudioHook",
                $"Replacing audio: {originalPath} -> {replacementPath}");
            return replacementPath;
        }
        return originalPath;
    }

    /// <summary>
    /// Add or update an audio replacement mapping at runtime.
    /// </summary>
    public static void AddReplacement(string vanillaPath, string kirbyPath)
    {
        if (!string.IsNullOrWhiteSpace(vanillaPath) && !string.IsNullOrWhiteSpace(kirbyPath))
        {
            _audioReplacements[vanillaPath] = kirbyPath;
            Logger.Log(LogLevel.Debug, "DZ/KirbyAudioHook",
                $"Added audio replacement: {vanillaPath} -> {kirbyPath}");
        }
    }

    /// <summary>
    /// Remove an audio replacement mapping.
    /// </summary>
    public static void RemoveReplacement(string vanillaPath)
    {
        if (_audioReplacements.Remove(vanillaPath))
        {
            Logger.Log(LogLevel.Debug, "DZ/KirbyAudioHook",
                $"Removed audio replacement: {vanillaPath}");
        }
    }

    /// <summary>
    /// Check if a vanilla audio path has a replacement.
    /// </summary>
    public static bool HasReplacement(string vanillaPath)
    {
        return _audioReplacements.ContainsKey(vanillaPath);
    }

    /// <summary>
    /// Get the replacement path for a vanilla audio path, or null if no replacement exists.
    /// </summary>
    public static string GetReplacement(string vanillaPath)
    {
        return _audioReplacements.TryGetValue(vanillaPath, out string replacement) ? replacement : null;
    }

    /// <summary>
    /// Get all current audio replacement mappings.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetAllReplacements()
    {
        return new Dictionary<string, string>(_audioReplacements);
    }
}
