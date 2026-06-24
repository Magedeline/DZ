using System;
using System.Collections.Generic;
using System.Reflection;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;

namespace Celeste;

/// <summary>
/// Redirects vanilla audio event paths to their custom Kirby variants.
/// Covers: UI, music, ambience/env, game, and character SFX.
/// When the game (or other mods) call Audio.Play, Audio.SetMusic,
/// Audio.SetAmbience, or Audio.CreateInstance with vanilla paths,
/// this hook swaps them for the corresponding kirby/ events.
///
/// This is the Kirby-themed alternative to <see cref="PusheenAudioHooks"/>.
/// </summary>
public static class KirbyAudioHooks
{
    private static bool _loaded;

    // -- Play hooks ----------------------------------------------------------
    private static Hook _playStringHook;
    private static Hook _playVector2Hook;
    private static Hook _playStringFloatHook;
    private static Hook _playVector2StringFloatHook;

    // -- Music / Ambience hooks ----------------------------------------------
    private static Hook _setMusicStringBoolBoolHook;
    private static Hook _setAmbienceStringHook;
    private static Hook _setAmbienceStringBoolHook;

    // -- Instance creation hook ----------------------------------------------
    private static Hook _createInstanceHook;

    public static void Load()
    {
        if (_loaded)
            return;

        _loaded = true;

        try
        {
            InstallPlayHooks();
            InstallMusicHooks();
            InstallAmbienceHooks();
            InstallCreateInstanceHook();
            Logger.Log(LogLevel.Info, "MaggyHelper", "KirbyAudioHooks loaded -- full audio path replacement active (UI, music, env, game, char)");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"[KirbyAudioHooks] Failed to load: {ex.Message}");
        }
    }

    public static void Unload()
    {
        if (!_loaded)
            return;

        _loaded = false;

        _playStringHook?.Dispose(); _playStringHook = null;
        _playVector2Hook?.Dispose(); _playVector2Hook = null;
        _playStringFloatHook?.Dispose(); _playStringFloatHook = null;
        _playVector2StringFloatHook?.Dispose(); _playVector2StringFloatHook = null;

        _setMusicStringBoolBoolHook?.Dispose(); _setMusicStringBoolBoolHook = null;

        _setAmbienceStringHook?.Dispose(); _setAmbienceStringHook = null;
        _setAmbienceStringBoolHook?.Dispose(); _setAmbienceStringBoolHook = null;

        _createInstanceHook?.Dispose(); _createInstanceHook = null;

        Logger.Log(LogLevel.Info, "MaggyHelper", "KirbyAudioHooks unloaded");
    }

    // =======================================================================
    // HOOK INSTALLATION
    // =======================================================================

    private static void InstallPlayHooks()
    {
        Type audioType = typeof(global::Celeste.Audio);
        Type hookType = typeof(KirbyAudioHooks);
        BindingFlags hookFlags = BindingFlags.Static | BindingFlags.NonPublic;

        // Audio.Play(string)
        TryHook(audioType, hookType, hookFlags, "Play",
            new[] { typeof(string) },
            nameof(Hook_Audio_Play_String), ref _playStringHook);

        // Audio.Play(string, Vector2)
        TryHook(audioType, hookType, hookFlags, "Play",
            new[] { typeof(string), typeof(Vector2) },
            nameof(Hook_Audio_Play_Vector2), ref _playVector2Hook);

        // Audio.Play(string, string, float)
        TryHook(audioType, hookType, hookFlags, "Play",
            new[] { typeof(string), typeof(string), typeof(float) },
            nameof(Hook_Audio_Play_String_Float), ref _playStringFloatHook);

        // Audio.Play(string, Vector2, string, float)
        TryHook(audioType, hookType, hookFlags, "Play",
            new[] { typeof(string), typeof(Vector2), typeof(string), typeof(float) },
            nameof(Hook_Audio_Play_Vector2_String_Float), ref _playVector2StringFloatHook);
    }

    private static void InstallMusicHooks()
    {
        Type audioType = typeof(global::Celeste.Audio);
        Type hookType = typeof(KirbyAudioHooks);
        BindingFlags hookFlags = BindingFlags.Static | BindingFlags.NonPublic;

        // Audio.SetMusic(string, bool, bool) — the only overload in Celeste 1.4.0.0
        TryHook(audioType, hookType, hookFlags, "SetMusic",
            new[] { typeof(string), typeof(bool), typeof(bool) },
            nameof(Hook_Audio_SetMusic_String_Bool_Bool), ref _setMusicStringBoolBoolHook);
    }

    private static void InstallAmbienceHooks()
    {
        Type audioType = typeof(global::Celeste.Audio);
        Type hookType = typeof(KirbyAudioHooks);
        BindingFlags hookFlags = BindingFlags.Static | BindingFlags.NonPublic;

        // Audio.SetAmbience(string)
        TryHook(audioType, hookType, hookFlags, "SetAmbience",
            new[] { typeof(string) },
            nameof(Hook_Audio_SetAmbience_String), ref _setAmbienceStringHook);

        // Audio.SetAmbience(string, bool)
        TryHook(audioType, hookType, hookFlags, "SetAmbience",
            new[] { typeof(string), typeof(bool) },
            nameof(Hook_Audio_SetAmbience_String_Bool), ref _setAmbienceStringBoolHook);
    }

    private static void InstallCreateInstanceHook()
    {
        Type audioType = typeof(global::Celeste.Audio);
        Type hookType = typeof(KirbyAudioHooks);
        BindingFlags hookFlags = BindingFlags.Static | BindingFlags.NonPublic;

        // Audio.CreateInstance(string)
        TryHook(audioType, hookType, hookFlags, "CreateInstance",
            new[] { typeof(string) },
            nameof(Hook_Audio_CreateInstance_String), ref _createInstanceHook);
    }

    private static void TryHook(Type targetType, Type hookType, BindingFlags hookFlags,
        string methodName, Type[] paramTypes, string hookMethodName, ref Hook hookField)
    {
        MethodInfo target = targetType.GetMethod(methodName,
            BindingFlags.Static | BindingFlags.Public,
            null, paramTypes, null);
        MethodInfo hook = hookType.GetMethod(hookMethodName, hookFlags);

        if (target != null && hook != null)
        {
            hookField = new Hook(target, hook);
        }
        else
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"[KirbyAudioHooks] Audio.{methodName}({string.Join(", ", Array.ConvertAll(paramTypes, t => t.Name))}) not found -- skipping");
        }
    }

    // =======================================================================
    // PATH REPLACEMENT
    // =======================================================================

    /// <summary>
    /// Replaces vanilla audio event paths with their custom variants.
    /// The FMOD banks only contain events under pusheen/ paths, so both
    /// Kirby and Pusheen themes redirect to pusheen/ until separate
    /// kirby-specific banks are built.
    /// </summary>
    private static string ReplacePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Already redirected -- don't double-redirect
        if (path.Contains("/pusheen/") || path.Contains("/kirby/"))
            return path;

        // -- UI ----------------------------------------------------------------
        if (path.StartsWith("event:/ui/"))
            return "event:/pusheen/ui/" + path.Substring("event:/ui/".Length);
        if (path.StartsWith("event:/new_content/ui/"))
            return "event:/pusheen/ui/new_content/" + path.Substring("event:/new_content/ui/".Length);

        // -- Music -------------------------------------------------------------
        if (path.StartsWith("event:/music/"))
            return "event:/pusheen/music/" + path.Substring("event:/music/".Length);
        if (path.StartsWith("event:/new_content/music/"))
            return "event:/pusheen/music/new_content/" + path.Substring("event:/new_content/music/".Length);

        // -- Environment / Ambience --------------------------------------------
        if (path.StartsWith("event:/env/"))
            return "event:/pusheen/env/" + path.Substring("event:/env/".Length);
        if (path.StartsWith("event:/new_content/env/"))
            return "event:/pusheen/env/new_content/" + path.Substring("event:/new_content/env/".Length);

        // -- Game SFX ----------------------------------------------------------
        if (path.StartsWith("event:/game/"))
            return "event:/pusheen/game/" + path.Substring("event:/game/".Length);
        if (path.StartsWith("event:/new_content/game/"))
            return "event:/pusheen/game/new_content/" + path.Substring("event:/new_content/game/".Length);

        // -- Character SFX (character-specific mapping) ------------------------
        // Vanilla badeline -> pusheen/char/chara (Chara is the custom Badeline)
        if (path.StartsWith("event:/char/badeline/"))
            return "event:/pusheen/char/chara/" + path.Substring("event:/char/badeline/".Length);
        if (path.StartsWith("event:/new_content/char/badeline/"))
            return "event:/pusheen/char/chara/new_content/" + path.Substring("event:/new_content/char/badeline/".Length);

        // Vanilla madeline -> pusheen/char/kirby (Kirby replaces Madeline)
        if (path.StartsWith("event:/char/madeline/"))
            return "event:/pusheen/char/kirby/" + path.Substring("event:/char/madeline/".Length);
        if (path.StartsWith("event:/new_content/char/madeline/"))
            return "event:/pusheen/char/kirby/new_content/" + path.Substring("event:/new_content/char/madeline/".Length);

        // Vanilla granny -> pusheen/char/granny
        if (path.StartsWith("event:/char/granny/"))
            return "event:/pusheen/char/granny/" + path.Substring("event:/char/granny/".Length);
        if (path.StartsWith("event:/new_content/char/granny/"))
            return "event:/pusheen/char/granny/new_content/" + path.Substring("event:/new_content/char/granny/".Length);

        // Vanilla oshiro -> pusheen/char/oshiro
        if (path.StartsWith("event:/char/oshiro/"))
            return "event:/pusheen/char/oshiro/" + path.Substring("event:/char/oshiro/".Length);
        if (path.StartsWith("event:/new_content/char/oshiro/"))
            return "event:/pusheen/char/oshiro/new_content/" + path.Substring("event:/new_content/char/oshiro/".Length);

        // Vanilla theo -> pusheen/char/theo
        if (path.StartsWith("event:/char/theo/"))
            return "event:/pusheen/char/theo/" + path.Substring("event:/char/theo/".Length);
        if (path.StartsWith("event:/new_content/char/theo/"))
            return "event:/pusheen/char/theo/new_content/" + path.Substring("event:/new_content/char/theo/".Length);

        return path;
    }

    // =======================================================================
    // HOOK HANDLER DELEGATES
    // =======================================================================

    // -- Play ----------------------------------------------------------------

    private static EventInstance Hook_Audio_Play_String(
        Func<string, EventInstance> orig, string path)
    {
        return orig(ReplacePath(path));
    }

    private static EventInstance Hook_Audio_Play_Vector2(
        Func<string, Vector2, EventInstance> orig, string path, Vector2 position)
    {
        return orig(ReplacePath(path), position);
    }

    private static EventInstance Hook_Audio_Play_String_Float(
        Func<string, string, float, EventInstance> orig, string path, string param, float value)
    {
        return orig(ReplacePath(path), param, value);
    }

    private static EventInstance Hook_Audio_Play_Vector2_String_Float(
        Func<string, Vector2, string, float, EventInstance> orig,
        string path, Vector2 position, string param, float value)
    {
        return orig(ReplacePath(path), position, param, value);
    }

    // -- Music ---------------------------------------------------------------

    private static bool Hook_Audio_SetMusic_String_Bool_Bool(
        Func<string, bool, bool, bool> orig, string path, bool startPlaying, bool allowFadeOut)
    {
        return orig(ReplacePath(path), startPlaying, allowFadeOut);
    }

    // -- Ambience ------------------------------------------------------------

    private static void Hook_Audio_SetAmbience_String(
        Action<string> orig, string path)
    {
        orig(ReplacePath(path));
    }

    private static void Hook_Audio_SetAmbience_String_Bool(
        Action<string, bool> orig, string path, bool allowFadeOut)
    {
        orig(ReplacePath(path), allowFadeOut);
    }

    // -- Instance creation ---------------------------------------------------

    private static EventInstance Hook_Audio_CreateInstance_String(
        Func<string, EventInstance> orig, string path)
    {
        return orig(ReplacePath(path));
    }
}
