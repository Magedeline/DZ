using FMOD.Studio;
using Monocle;

namespace Celeste;

/// <summary>
/// Hooks into the vanilla title screen to replace the first-input sound
/// with the custom DesoloZantas version.
/// </summary>
public static class TitleScreen_ExtHook
{
    private static bool _hooked;

    private const string VANILLA_TITLE_FIRSTINPUT = "event:/ui/main/title_firstinput";
    private const string CUSTOM_TITLE_FIRSTINPUT = "event:/ui/pusheen/game/title_firstinput";

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.Audio.Play_string += OnAudioPlayString;

        Logger.Log(LogLevel.Info, "MaggyHelper", "TitleScreen_ExtHook loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.Audio.Play_string -= OnAudioPlayString;

        Logger.Log(LogLevel.Info, "MaggyHelper", "TitleScreen_ExtHook unloaded");
    }

    private static EventInstance OnAudioPlayString(On.Celeste.Audio.orig_Play_string orig, string path)
    {
        // Use vanilla audio since custom FMOD events may not exist
        return orig(path);
    }
}

