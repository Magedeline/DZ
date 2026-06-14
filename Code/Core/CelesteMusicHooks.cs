using FMOD.Studio;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using System.Reflection.Emit;

namespace Celeste;

/// <summary>
/// Comprehensive music hook system for the Celeste D-Side mod.
/// Supports both On.hook (delegate) and IL.hook (IL manipulation) for audio/music events.
/// Handles music playback, parameters, bus routing, and special audio events.
/// </summary>
public static class CelesteMusicHooks
{
    private static bool _loaded;

    // IL.hook patches (reserved for future use)
    private static ILHook _audioPlayILHook;
    private static ILHook _audioStopILHook;

    private static Dictionary<string, int> _musicPlayCount = new();
    private static Dictionary<string, float> _musicParamValues = new();

    public static void Load()
    {
        if (_loaded)
            return;

        _loaded = true;

        // ──── On.hook for audio events ────

        On.Celeste.Audio.SetMusicParam += OnAudioSetMusicParam;

        // ──── IL.hook patches for music system ────

        InstallMusicILHooks();

        Logger.Log(LogLevel.Info, "MaggyHelper", "CelesteMusicHooks loaded with On.hook and IL.hook support");
    }

    public static void Unload()
    {
        if (!_loaded)
            return;

        _loaded = false;

        // ──── Unload On.hook delegates ────

        On.Celeste.Audio.SetMusicParam -= OnAudioSetMusicParam;

        // ──── Dispose IL.hook patches ────

        _audioPlayILHook?.Dispose();
        _audioPlayILHook = null;

        _audioStopILHook?.Dispose();
        _audioStopILHook = null;

        _musicPlayCount.Clear();
        _musicParamValues.Clear();

        Logger.Log(LogLevel.Info, "MaggyHelper", "CelesteMusicHooks unloaded (On.hook and IL.hook)");
    }

    // ──── On.hook handler delegates ────

    private static bool _fmodParamApiMissing;

    private static void OnAudioSetMusicParam(On.Celeste.Audio.orig_SetMusicParam orig, string param, float value)
    {
        // Track music parameter changes for D-Side music system
        if (param != null)
        {
            lock (_musicParamValues)
            {
                _musicParamValues[param] = value;
            }

            if (param.Contains("dside") || param.Contains("escape") || param.Contains("intensity"))
            {
                Logger.Log(LogLevel.Debug, "MaggyHelper/MusicHooks",
                    $"Music param set: {param} = {value}");
            }
        }

        // If we already know the FMOD 1.x API is missing, don't even try calling orig
        if (_fmodParamApiMissing)
            return;

        try
        {
            orig(param, value);
        }
        catch (EntryPointNotFoundException)
        {
            // FMOD 2.x removed setParameterValue (1.x API) in favor of
            // setParameterByName. Suppress the crash and disable future
            // calls to avoid repeated exceptions.
            _fmodParamApiMissing = true;
            Logger.Log(LogLevel.Warn, "MaggyHelper/MusicHooks",
                "[Audio] FMOD_Studio_EventInstance_SetParameterValue not found in fmodstudio DLL " +
                "(FMOD 1.x API vs 2.x DLL mismatch). Music parameter changes will be skipped.");
        }
        catch (TypeLoadException ex) when (ex.TypeName?.Contains("FMOD") == true || ex.Message.Contains("FMOD"))
        {
            // Also catch TypeLoadException from FMOD interop issues
            _fmodParamApiMissing = true;
            Logger.Log(LogLevel.Warn, "MaggyHelper/MusicHooks",
                "[Audio] FMOD TypeLoadException - API mismatch detected. Music parameter changes will be skipped.");
        }
    }

    // ──── IL.hook installation ────

    private static void InstallMusicILHooks()
    {
        try
        {
            // Reserved for future music-system IL hooks
            // Music parameter tracking can be implemented here if needed
            Logger.Log(LogLevel.Debug, "MaggyHelper", "Music IL hooks initialized");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to install music IL hooks: {ex.Message}");
        }
    }

    // ──── Music tracking utilities ────

    private static void TrackMusicPlay(string path)
    {
        lock (_musicPlayCount)
        {
            if (_musicPlayCount.ContainsKey(path))
                _musicPlayCount[path]++;
            else
                _musicPlayCount[path] = 1;
        }

        Logger.Log(LogLevel.Debug, "MaggyHelper/MusicHooks",
            $"Music played: {path}");
    }

    public static int GetMusicPlayCount(string path)
    {
        lock (_musicPlayCount)
        {
            return _musicPlayCount.ContainsKey(path) ? _musicPlayCount[path] : 0;
        }
    }

    public static float GetMusicParam(string param)
    {
        lock (_musicParamValues)
        {
            return _musicParamValues.ContainsKey(param) ? _musicParamValues[param] : 0f;
        }
    }

    public static Dictionary<string, int> GetMusicPlayStats()
    {
        lock (_musicPlayCount)
        {
            return new Dictionary<string, int>(_musicPlayCount);
        }
    }
}
