using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Reflection;
using Celeste;

namespace Celeste;

/// <summary>
/// Audio hook system to replace vanilla Celeste audio with custom Pusheen audio.
/// Intercepts Audio.Play calls and redirects them to custom audio events.
/// </summary>
public static class PusheenAudioHook
{
    private static bool _loaded;
    private static Dictionary<string, string> _audioReplacements = new();
    private static ILHook _audioPlayILHook;

    public static void Load()
    {
        if (_loaded)
            return;

        _loaded = true;

        // Initialize audio replacement mappings
        InitializeAudioMappings();

        // Hook Audio.Play using IL manipulation
        InstallAudioPlayILHook();

        Logger.Log(LogLevel.Info, "MaggyHelper", "PusheenAudioHook loaded - vanilla audio will be replaced with custom Pusheen audio");
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

        Logger.Log(LogLevel.Info, "MaggyHelper", "PusheenAudioHook unloaded");
    }

    private static void InitializeAudioMappings()
    {
        _audioReplacements.Clear();

        // Character dialogue replacements
        _audioReplacements["event:/char/dialogue/badeline"] = "event:/char/dialogue/pusheen/badeline";
        _audioReplacements["event:/char/dialogue/ex"] = "event:/char/dialogue/pusheen/ex";
        _audioReplacements["event:/char/dialogue/granny"] = "event:/char/dialogue/pusheen/granny";
        _audioReplacements["event:/char/dialogue/madeline"] = "event:/char/dialogue/pusheen/madeline";
        _audioReplacements["event:/char/dialogue/madeline_mirror"] = "event:/char/dialogue/pusheen/madeline_mirror";
        _audioReplacements["event:/char/dialogue/mom"] = "event:/char/dialogue/pusheen/mom";
        _audioReplacements["event:/char/dialogue/oshiro"] = "event:/char/dialogue/pusheen/oshiro";
        _audioReplacements["event:/char/dialogue/secret_character"] = "event:/char/dialogue/pusheen/secret_character";
        _audioReplacements["event:/char/dialogue/theo"] = "event:/char/dialogue/pusheen/theo";
        _audioReplacements["event:/char/dialogue/theo_mirror"] = "event:/char/dialogue/pusheen/theo_mirror";
        _audioReplacements["event:/char/dialogue/theo_webcam"] = "event:/char/dialogue/pusheen/theo_webcam";

        // Phone static SFX support replacements
        _audioReplacements["event:/char/dialogue/sfx_support/phone_static_ex"] = "event:/char/dialogue/pusheen/sfx_support/phone_static_ex";
        _audioReplacements["event:/char/dialogue/sfx_support/phone_static_mom"] = "event:/char/dialogue/pusheen/sfx_support/phone_static_mom";

        // Granny SFX replacements
        _audioReplacements["event:/char/granny/cane_tap"] = "event:/char/granny/cane_tap";
        _audioReplacements["event:/char/granny/laugh_firstphrase"] = "event:/char/granny/laugh_firstphrase";
        _audioReplacements["event:/char/granny/laugh_oneha"] = "event:/char/granny/laugh_oneha";

        // Madeline SFX replacements
        _audioReplacements["event:/char/madeline/jump"] = "event:/char/madeline/jump";
        _audioReplacements["event:/char/madeline/landing"] = "event:/char/madeline/landing";
        _audioReplacements["event:/char/madeline/death"] = "event:/char/madeline/death";
        _audioReplacements["event:/char/madeline/footstep"] = "event:/char/madeline/footstep";
        _audioReplacements["event:/char/madeline/dash_pink_left"] = "event:/char/madeline/dash_pink_left";
        _audioReplacements["event:/char/madeline/dash_pink_right"] = "event:/char/madeline/dash_pink_right";
        _audioReplacements["event:/char/madeline/dash_red_left"] = "event:/char/madeline/dash_red_left";
        _audioReplacements["event:/char/madeline/dash_red_right"] = "event:/char/madeline/dash_red_right";

        // Badeline SFX replacements
        _audioReplacements["event:/char/badeline/jump"] = "event:/char/badeline/jump";
        _audioReplacements["event:/char/badeline/landing"] = "event:/char/badeline/landing";
        _audioReplacements["event:/char/badeline/dash_red_left"] = "event:/char/badeline/dash_red_left";
        _audioReplacements["event:/char/badeline/dash_red_right"] = "event:/char/badeline/dash_red_right";

        // Oshiro SFX replacements
        _audioReplacements["event:/char/oshiro/boss_charge"] = "event:/char/oshiro/boss_charge";
        _audioReplacements["event:/char/oshiro/boss_slam_first"] = "event:/char/oshiro/boss_slam_first";
        _audioReplacements["event:/char/oshiro/boss_slam_final"] = "event:/char/oshiro/boss_slam_final";

        // Theo SFX replacements
        _audioReplacements["event:/char/theo/phone_taps_loop"] = "event:/char/theo/phone_taps_loop";
        _audioReplacements["event:/char/theo/resort_ceilingvent_hey"] = "event:/char/theo/resort_ceilingvent_hey";
        _audioReplacements["event:/char/theo/yolo_fist"] = "event:/char/theo/yolo_fist";

        // UI Game audio replacements
        _audioReplacements["event:/ui/game/chatoptions_appear"] = "event:/ui/game/chatoptions_appear";
        _audioReplacements["event:/ui/game/chatoptions_roll_down"] = "event:/ui/game/chatoptions_roll_down";
        _audioReplacements["event:/ui/game/chatoptions_roll_up"] = "event:/ui/game/chatoptions_roll_up";
        _audioReplacements["event:/ui/game/chatoptions_select"] = "event:/ui/game/chatoptions_select";
        _audioReplacements["event:/ui/game/general_text_loop"] = "event:/ui/game/general_text_loop";
        _audioReplacements["event:/ui/game/hotspot_main_in"] = "event:/ui/game/hotspot_main_in";
        _audioReplacements["event:/ui/game/hotspot_main_out"] = "event:/ui/game/hotspot_main_out";
        _audioReplacements["event:/ui/game/hotspot_note_in"] = "event:/ui/game/hotspot_note_in";
        _audioReplacements["event:/ui/game/hotspot_note_out"] = "event:/ui/game/hotspot_note_out";
        _audioReplacements["event:/ui/game/increment_dashcount"] = "event:/ui/game/increment_dashcount";
        _audioReplacements["event:/ui/game/increment_strawberry"] = "event:/ui/game/increment_strawberry";
        _audioReplacements["event:/ui/game/lookout_off"] = "event:/ui/game/lookout_off";
        _audioReplacements["event:/ui/game/lookout_on"] = "event:/ui/game/lookout_on";
        _audioReplacements["event:/ui/game/memorial_dream_loop"] = "event:/ui/game/memorial_dream_loop";
        _audioReplacements["event:/ui/game/memorial_dream_text_in"] = "event:/ui/game/memorial_dream_text_in";
        _audioReplacements["event:/ui/game/memorial_dream_text_loop"] = "event:/ui/game/memorial_dream_text_loop";
        _audioReplacements["event:/ui/game/memorial_dream_text_out"] = "event:/ui/game/memorial_dream_text_out";
        _audioReplacements["event:/ui/game/memorial_text_in"] = "event:/ui/game/memorial_text_in";
        _audioReplacements["event:/ui/game/memorial_text_loop"] = "event:/ui/game/memorial_text_loop";
        _audioReplacements["event:/ui/game/memorial_text_out"] = "event:/ui/game/memorial_text_out";
        _audioReplacements["event:/ui/game/pause"] = "event:/ui/game/pause";
        _audioReplacements["event:/ui/game/textadvance_madeline"] = "event:/ui/game/textadvance_madeline";
        _audioReplacements["event:/ui/game/textadvance_other"] = "event:/ui/game/textadvance_other";
        _audioReplacements["event:/ui/game/textbox_madeline_in"] = "event:/ui/game/textbox_madeline_in";
        _audioReplacements["event:/ui/game/textbox_madeline_out"] = "event:/ui/game/textbox_madeline_out";
        _audioReplacements["event:/ui/game/textbox_other_in"] = "event:/ui/game/textbox_other_in";
        _audioReplacements["event:/ui/game/textbox_other_out"] = "event:/ui/game/textbox_other_out";
        _audioReplacements["event:/ui/game/tutorial_note_flip_back"] = "event:/ui/game/tutorial_note_flip_back";
        _audioReplacements["event:/ui/game/tutorial_note_flip_front"] = "event:/ui/game/tutorial_note_flip_front";
        _audioReplacements["event:/ui/game/unpause"] = "event:/ui/game/unpause";

        // UI Main menu audio replacements
        _audioReplacements["event:/ui/main/assist_button_info"] = "event:/ui/main/assist_button_info";
        _audioReplacements["event:/ui/main/assist_button_no"] = "event:/ui/main/assist_button_no";
        _audioReplacements["event:/ui/main/assist_button_yes"] = "event:/ui/main/assist_button_yes";
        _audioReplacements["event:/ui/main/assist_info_whistle"] = "event:/ui/main/assist_info_whistle";
        _audioReplacements["event:/ui/main/bside_intro_text"] = "event:/ui/main/bside_intro_text";
        _audioReplacements["event:/ui/main/button_back"] = "event:/ui/main/button_back";
        _audioReplacements["event:/ui/main/button_climb"] = "event:/ui/main/button_climb";
        _audioReplacements["event:/ui/main/button_invalid"] = "event:/ui/main/button_invalid";
        _audioReplacements["event:/ui/main/button_lowkey"] = "event:/ui/main/button_lowkey";
        _audioReplacements["event:/ui/main/button_select"] = "event:/ui/main/button_select";
        _audioReplacements["event:/ui/main/button_toggle_off"] = "event:/ui/main/button_toggle_off";
        _audioReplacements["event:/ui/main/button_toggle_on"] = "event:/ui/main/button_toggle_on";
        _audioReplacements["event:/ui/main/message_confirm"] = "event:/ui/main/message_confirm";
        _audioReplacements["event:/ui/main/postcard_ch1_in"] = "event:/ui/main/postcard_ch1_in";
        _audioReplacements["event:/ui/main/postcard_ch1_out"] = "event:/ui/main/postcard_ch1_out";
        _audioReplacements["event:/ui/main/postcard_ch2_in"] = "event:/ui/main/postcard_ch2_in";
        _audioReplacements["event:/ui/main/postcard_ch2_out"] = "event:/ui/main/postcard_ch2_out";
        _audioReplacements["event:/ui/main/postcard_ch3_in"] = "event:/ui/main/postcard_ch3_in";
        _audioReplacements["event:/ui/main/postcard_ch3_out"] = "event:/ui/main/postcard_ch3_out";
        _audioReplacements["event:/ui/main/postcard_ch4_in"] = "event:/ui/main/postcard_ch4_in";
        _audioReplacements["event:/ui/main/postcard_ch4_out"] = "event:/ui/main/postcard_ch4_out";
        _audioReplacements["event:/ui/main/postcard_ch5_in"] = "event:/ui/main/postcard_ch5_in";
        _audioReplacements["event:/ui/main/postcard_ch5_out"] = "event:/ui/main/postcard_ch5_out";
        _audioReplacements["event:/ui/main/postcard_ch6_in"] = "event:/ui/main/postcard_ch6_in";
        _audioReplacements["event:/ui/main/postcard_ch6_out"] = "event:/ui/main/postcard_ch6_out";
        _audioReplacements["event:/ui/main/postcard_csides_in"] = "event:/ui/main/postcard_csides_in";
        _audioReplacements["event:/ui/main/postcard_csides_out"] = "event:/ui/main/postcard_csides_out";
        _audioReplacements["event:/ui/main/rename_entry_accept"] = "event:/ui/main/rename_entry_accept";
        _audioReplacements["event:/ui/main/rename_entry_backspace"] = "event:/ui/main/rename_entry_backspace";
        _audioReplacements["event:/ui/main/rename_entry_char"] = "event:/ui/main/rename_entry_char";
        _audioReplacements["event:/ui/main/rename_entry_rollover"] = "event:/ui/main/rename_entry_rollover";
        _audioReplacements["event:/ui/main/rename_entry_space"] = "event:/ui/main/rename_entry_space";
        _audioReplacements["event:/ui/main/rollover_down"] = "event:/ui/main/rollover_down";
        _audioReplacements["event:/ui/main/rollover_up"] = "event:/ui/main/rollover_up";
        _audioReplacements["event:/ui/main/savefile_begin"] = "event:/ui/main/savefile_begin";
        _audioReplacements["event:/ui/main/savefile_delete"] = "event:/ui/main/savefile_delete";
        _audioReplacements["event:/ui/main/savefile_rename_start"] = "event:/ui/main/savefile_rename_start";
        _audioReplacements["event:/ui/main/savefile_rollover_down"] = "event:/ui/main/savefile_rollover_down";
        _audioReplacements["event:/ui/main/savefile_rollover_first"] = "event:/ui/main/savefile_rollover_first";
        _audioReplacements["event:/ui/main/savefile_rollover_up"] = "event:/ui/main/savefile_rollover_up";
        _audioReplacements["event:/ui/main/title_firstinput"] = "event:/ui/main/title_firstinput";
        _audioReplacements["event:/ui/main/whoosh_large_in"] = "event:/ui/main/whoosh_large_in";
        _audioReplacements["event:/ui/main/whoosh_large_out"] = "event:/ui/main/whoosh_large_out";
        _audioReplacements["event:/ui/main/whoosh_list_in"] = "event:/ui/main/whoosh_list_in";
        _audioReplacements["event:/ui/main/whoosh_list_out"] = "event:/ui/main/whoosh_list_out";
        _audioReplacements["event:/ui/main/whoosh_savefile_in"] = "event:/ui/main/whoosh_savefile_in";
        _audioReplacements["event:/ui/main/whoosh_savefile_out"] = "event:/ui/main/whoosh_savefile_out";

        // UI Postgame audio replacements
        _audioReplacements["event:/ui/postgame/crystal_heart"] = "event:/ui/postgame/crystal_heart";
        _audioReplacements["event:/ui/postgame/death_appear"] = "event:/ui/postgame/death_appear";
        _audioReplacements["event:/ui/postgame/death_count"] = "event:/ui/postgame/death_count";
        _audioReplacements["event:/ui/postgame/death_final"] = "event:/ui/postgame/death_final";
        _audioReplacements["event:/ui/postgame/goldberry_count"] = "event:/ui/postgame/goldberry_count";
        _audioReplacements["event:/ui/postgame/strawberry_count"] = "event:/ui/postgame/strawberry_count";
        _audioReplacements["event:/ui/postgame/strawberry_total"] = "event:/ui/postgame/strawberry_total";
        _audioReplacements["event:/ui/postgame/strawberry_total_all"] = "event:/ui/postgame/strawberry_total_all";
        _audioReplacements["event:/ui/postgame/unlock_bside"] = "event:/ui/postgame/unlock_bside";
        _audioReplacements["event:/ui/postgame/unlock_newchapter"] = "event:/ui/postgame/unlock_newchapter";
        _audioReplacements["event:/ui/postgame/unlock_newchapter_icon"] = "event:/ui/postgame/unlock_newchapter_icon";

        // UI World Map audio replacements
        _audioReplacements["event:/ui/world_map/chapter/back"] = "event:/ui/world_map/chapter/back";
        _audioReplacements["event:/ui/world_map/chapter/checkpoint_back"] = "event:/ui/world_map/chapter/checkpoint_back";
        _audioReplacements["event:/ui/world_map/chapter/checkpoint_photo_add"] = "event:/ui/world_map/chapter/checkpoint_photo_add";
        _audioReplacements["event:/ui/world_map/chapter/checkpoint_photo_remove"] = "event:/ui/world_map/chapter/checkpoint_photo_remove";
        _audioReplacements["event:/ui/world_map/chapter/checkpoint_start"] = "event:/ui/world_map/chapter/checkpoint_start";
        _audioReplacements["event:/ui/world_map/chapter/level_select"] = "event:/ui/world_map/chapter/level_select";
        _audioReplacements["event:/ui/world_map/chapter/pane_contract"] = "event:/ui/world_map/chapter/pane_contract";
        _audioReplacements["event:/ui/world_map/chapter/pane_expand"] = "event:/ui/world_map/chapter/pane_expand";
        _audioReplacements["event:/ui/world_map/chapter/tab_roll_left"] = "event:/ui/world_map/chapter/tab_roll_left";
        _audioReplacements["event:/ui/world_map/chapter/tab_roll_right"] = "event:/ui/world_map/chapter/tab_roll_right";
        _audioReplacements["event:/ui/world_map/icon/assist_skip"] = "event:/ui/world_map/icon/assist_skip";
        _audioReplacements["event:/ui/world_map/icon/flip_left"] = "event:/ui/world_map/icon/flip_left";
        _audioReplacements["event:/ui/world_map/icon/flip_right"] = "event:/ui/world_map/icon/flip_right";
        _audioReplacements["event:/ui/world_map/icon/roll_left"] = "event:/ui/world_map/icon/roll_left";
        _audioReplacements["event:/ui/world_map/icon/roll_right"] = "event:/ui/world_map/icon/roll_right";
        _audioReplacements["event:/ui/world_map/icon/select"] = "event:/ui/world_map/icon/select";
        _audioReplacements["event:/ui/world_map/journal/back"] = "event:/ui/world_map/journal/back";
        _audioReplacements["event:/ui/world_map/journal/heart_grab"] = "event:/ui/world_map/journal/heart_grab";
        _audioReplacements["event:/ui/world_map/journal/heart_release"] = "event:/ui/world_map/journal/heart_release";
        _audioReplacements["event:/ui/world_map/journal/heart_roll"] = "event:/ui/world_map/journal/heart_roll";
        _audioReplacements["event:/ui/world_map/journal/heart_shift_down"] = "event:/ui/world_map/journal/heart_shift_down";
        _audioReplacements["event:/ui/world_map/journal/heart_shift_up"] = "event:/ui/world_map/journal/heart_shift_up";
        _audioReplacements["event:/ui/world_map/journal/page_cover_back"] = "event:/ui/world_map/journal/page_cover_back";
        _audioReplacements["event:/ui/world_map/journal/page_cover_forward"] = "event:/ui/world_map/journal/page_cover_forward";
        _audioReplacements["event:/ui/world_map/journal/page_main_back"] = "event:/ui/world_map/journal/page_main_back";
        _audioReplacements["event:/ui/world_map/journal/page_main_forward"] = "event:/ui/world_map/journal/page_main_forward";
        _audioReplacements["event:/ui/world_map/journal/select"] = "event:/ui/world_map/journal/select";
        _audioReplacements["event:/ui/world_map/whoosh/1000ms_back"] = "event:/ui/world_map/whoosh/1000ms_back";
        _audioReplacements["event:/ui/world_map/whoosh/1000ms_forward"] = "event:/ui/world_map/whoosh/1000ms_forward";
        _audioReplacements["event:/ui/world_map/whoosh/400ms_back"] = "event:/ui/world_map/whoosh/400ms_back";
        _audioReplacements["event:/ui/world_map/whoosh/400ms_forward"] = "event:/ui/world_map/whoosh/400ms_forward";
        _audioReplacements["event:/ui/world_map/whoosh/600ms_back"] = "event:/ui/world_map/whoosh/600ms_back";
        _audioReplacements["event:/ui/world_map/whoosh/600ms_forward"] = "event:/ui/world_map/whoosh/600ms_forward";
        _audioReplacements["event:/ui/world_map/whoosh/700ms_back"] = "event:/ui/world_map/whoosh/700ms_back";
        _audioReplacements["event:/ui/world_map/whoosh/700ms_forward"] = "event:/ui/world_map/whoosh/700ms_forward";
        _audioReplacements["event:/ui/world_map/whoosh/900ms_back"] = "event:/ui/world_map/whoosh/900ms_back";
        _audioReplacements["event:/ui/world_map/whoosh/900ms_forward"] = "event:/ui/world_map/whoosh/900ms_forward";

        Logger.Log(LogLevel.Info, "MaggyHelper", $"PusheenAudioHook: Loaded {_audioReplacements.Count} audio replacement mappings");
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
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Failed to find Audio.Play method for IL hook.");
                return;
            }

            _audioPlayILHook = new ILHook(target, IL_Audio_Play);
            Logger.Log(LogLevel.Debug, "MaggyHelper", "Audio.Play IL hook installed");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to install Audio.Play IL hook: {ex.Message}");
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

            Logger.Log(LogLevel.Debug, "MaggyHelper", "Audio.Play IL hook patched successfully");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Error in IL_Audio_Play: {ex.Message}");
        }
    }

    private static string ReplaceAudioPath(string originalPath)
    {
        if (_audioReplacements.TryGetValue(originalPath, out string replacementPath))
        {
            Logger.Log(LogLevel.Debug, "MaggyHelper/PusheenAudioHook",
                $"Replacing audio: {originalPath} -> {replacementPath}");
            return replacementPath;
        }
        return originalPath;
    }

    /// <summary>
    /// Add or update an audio replacement mapping at runtime.
    /// </summary>
    public static void AddReplacement(string vanillaPath, string pusheenPath)
    {
        if (!string.IsNullOrWhiteSpace(vanillaPath) && !string.IsNullOrWhiteSpace(pusheenPath))
        {
            _audioReplacements[vanillaPath] = pusheenPath;
            Logger.Log(LogLevel.Debug, "MaggyHelper/PusheenAudioHook",
                $"Added audio replacement: {vanillaPath} -> {pusheenPath}");
        }
    }

    /// <summary>
    /// Remove an audio replacement mapping.
    /// </summary>
    public static void RemoveReplacement(string vanillaPath)
    {
        if (_audioReplacements.Remove(vanillaPath))
        {
            Logger.Log(LogLevel.Debug, "MaggyHelper/PusheenAudioHook",
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
