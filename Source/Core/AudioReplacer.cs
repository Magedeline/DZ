using Celeste;

namespace DZ;

/// <summary>
/// Intercepts every Audio.Play overload and redirects vanilla event paths to
/// their custom Pusheen equivalents in the mod's FMOD banks.
/// </summary>
public static class AudioReplacer
{
    private static bool _loaded;

    private static readonly Dictionary<string, string> Replacements = new()
    {
        // Granny SFX
        ["event:/char/granny/cane_tap"]            = "event:/pusheen/char/granny/cane_tap",
        ["event:/char/granny/laugh_firstphrase"]   = "event:/pusheen/char/granny/laugh_firstphrase",
        ["event:/char/granny/laugh_oneha"]         = "event:/pusheen/char/granny/laugh_oneha",

        // Madeline SFX
        ["event:/char/madeline/jump"]              = "event:/pusheen/char/madeline/jump",
        ["event:/char/madeline/landing"]           = "event:/pusheen/char/madeline/landing",
        ["event:/char/madeline/death"]             = "event:/pusheen/char/madeline/death",
        ["event:/char/madeline/footstep"]          = "event:/pusheen/char/madeline/footstep",
        ["event:/char/madeline/dash_pink_left"]    = "event:/pusheen/char/madeline/dash_pink_left",
        ["event:/char/madeline/dash_pink_right"]   = "event:/pusheen/char/madeline/dash_pink_right",
        ["event:/char/madeline/dash_red_left"]     = "event:/pusheen/char/madeline/dash_red_left",
        ["event:/char/madeline/dash_red_right"]    = "event:/pusheen/char/madeline/dash_red_right",

        // Badeline SFX
        ["event:/char/badeline/jump"]              = "event:/pusheen/char/badeline/jump",
        ["event:/char/badeline/landing"]           = "event:/pusheen/char/badeline/landing",
        ["event:/char/badeline/dash_red_left"]     = "event:/pusheen/char/badeline/dash_red_left",
        ["event:/char/badeline/dash_red_right"]    = "event:/pusheen/char/badeline/dash_red_right",

        // Oshiro SFX
        ["event:/char/oshiro/bossDZ_CHarge"]         = "event:/pusheen/char/oshiro/bossDZ_CHarge",
        ["event:/char/oshiro/boss_slam_first"]     = "event:/pusheen/char/oshiro/boss_slam_first",
        ["event:/char/oshiro/boss_slam_final"]     = "event:/pusheen/char/oshiro/boss_slam_final",

        // Theo SFX
        ["event:/char/theo/phone_taps_loop"]              = "event:/pusheen/char/theo/phone_taps_loop",
        ["event:/char/theo/resort_ceilingvent_hey"]       = "event:/pusheen/char/theo/resort_ceilingvent_hey",
        ["event:/char/theo/yolo_fist"]                    = "event:/pusheen/char/theo/yolo_fist",

        // UI Game
        ["event:/ui/game/chatoptions_appear"]             = "event:/pusheen/ui/game/chatoptions_appear",
        ["event:/ui/game/chatoptions_roll_down"]          = "event:/pusheen/ui/game/chatoptions_roll_down",
        ["event:/ui/game/chatoptions_roll_up"]            = "event:/pusheen/ui/game/chatoptions_roll_up",
        ["event:/ui/game/chatoptions_select"]             = "event:/pusheen/ui/game/chatoptions_select",
        ["event:/ui/game/general_text_loop"]              = "event:/pusheen/ui/game/general_text_loop",
        ["event:/ui/game/hotspot_main_in"]                = "event:/pusheen/ui/game/hotspot_main_in",
        ["event:/ui/game/hotspot_main_out"]               = "event:/pusheen/ui/game/hotspot_main_out",
        ["event:/ui/game/hotspot_note_in"]                = "event:/pusheen/ui/game/hotspot_note_in",
        ["event:/ui/game/hotspot_note_out"]               = "event:/pusheen/ui/game/hotspot_note_out",
        ["event:/ui/game/increment_dashcount"]            = "event:/pusheen/ui/game/increment_dashcount",
        ["event:/ui/game/increment_strawberry"]           = "event:/pusheen/ui/game/increment_strawberry",
        ["event:/ui/game/lookout_off"]                    = "event:/pusheen/ui/game/lookout_off",
        ["event:/ui/game/lookout_on"]                     = "event:/pusheen/ui/game/lookout_on",
        ["event:/ui/game/memorial_dream_loop"]            = "event:/pusheen/ui/game/memorial_dream_loop",
        ["event:/ui/game/memorial_dream_text_in"]         = "event:/pusheen/ui/game/memorial_dream_text_in",
        ["event:/ui/game/memorial_dream_text_loop"]       = "event:/pusheen/ui/game/memorial_dream_text_loop",
        ["event:/ui/game/memorial_dream_text_out"]        = "event:/pusheen/ui/game/memorial_dream_text_out",
        ["event:/ui/game/memorial_text_in"]               = "event:/pusheen/ui/game/memorial_text_in",
        ["event:/ui/game/memorial_text_loop"]             = "event:/pusheen/ui/game/memorial_text_loop",
        ["event:/ui/game/memorial_text_out"]              = "event:/pusheen/ui/game/memorial_text_out",
        ["event:/ui/game/pause"]                          = "event:/pusheen/ui/game/pause",
        ["event:/ui/game/textadvance_madeline"]           = "event:/pusheen/ui/game/textadvance_madeline",
        ["event:/ui/game/textadvance_other"]              = "event:/pusheen/ui/game/textadvance_other",
        ["event:/ui/game/textbox_madeline_in"]            = "event:/pusheen/ui/game/textbox_madeline_in",
        ["event:/ui/game/textbox_madeline_out"]           = "event:/pusheen/ui/game/textbox_madeline_out",
        ["event:/ui/game/textbox_other_in"]               = "event:/pusheen/ui/game/textbox_other_in",
        ["event:/ui/game/textbox_other_out"]              = "event:/pusheen/ui/game/textbox_other_out",
        ["event:/ui/game/tutorial_note_flip_back"]        = "event:/pusheen/ui/game/tutorial_note_flip_back",
        ["event:/ui/game/tutorial_note_flip_front"]       = "event:/pusheen/ui/game/tutorial_note_flip_front",
        ["event:/ui/game/unpause"]                        = "event:/pusheen/ui/game/unpause",

        // UI Main
        ["event:/ui/main/assist_button_info"]             = "event:/pusheen/ui/main/assist_button_info",
        ["event:/ui/main/assist_button_no"]               = "event:/pusheen/ui/main/assist_button_no",
        ["event:/ui/main/assist_button_yes"]              = "event:/pusheen/ui/main/assist_button_yes",
        ["event:/ui/main/1_intro_text"]               = "event:/pusheen/ui/main/1_intro_text",
        ["event:/ui/main/button_back"]                    = "event:/pusheen/ui/main/button_back",
        ["event:/ui/main/button_climb"]                   = "event:/pusheen/ui/main/button_climb",
        ["event:/ui/main/button_invalid"]                 = "event:/pusheen/ui/main/button_invalid",
        ["event:/ui/main/button_lowkey"]                  = "event:/pusheen/ui/main/button_lowkey",
        ["event:/ui/main/button_select"]                  = "event:/pusheen/ui/main/button_select",
        ["event:/ui/main/button_toggle_off"]              = "event:/pusheen/ui/main/button_toggle_off",
        ["event:/ui/main/button_toggle_on"]               = "event:/pusheen/ui/main/button_toggle_on",
        ["event:/ui/main/message_confirm"]                = "event:/pusheen/ui/main/message_confirm",
        ["event:/ui/main/postcardDZ_CH1_in"]                = "event:/pusheen/ui/main/postcardDZ_CH1_in",
        ["event:/ui/main/postcardDZ_CH1_out"]               = "event:/pusheen/ui/main/postcardDZ_CH1_out",
        ["event:/ui/main/postcardDZ_CH2_in"]                = "event:/pusheen/ui/main/postcardDZ_CH2_in",
        ["event:/ui/main/postcardDZ_CH2_out"]               = "event:/pusheen/ui/main/postcardDZ_CH2_out",
        ["event:/ui/main/postcardDZ_CH3_in"]                = "event:/pusheen/ui/main/postcardDZ_CH3_in",
        ["event:/ui/main/postcardDZ_CH3_out"]               = "event:/pusheen/ui/main/postcardDZ_CH3_out",
        ["event:/ui/main/postcardDZ_CH4_in"]                = "event:/pusheen/ui/main/postcardDZ_CH4_in",
        ["event:/ui/main/postcardDZ_CH4_out"]               = "event:/pusheen/ui/main/postcardDZ_CH4_out",
        ["event:/ui/main/postcardDZ_CH5_in"]                = "event:/pusheen/ui/main/postcardDZ_CH5_in",
        ["event:/ui/main/postcardDZ_CH5_out"]               = "event:/pusheen/ui/main/postcardDZ_CH5_out",
        ["event:/ui/main/postcardDZ_CH6_in"]                = "event:/pusheen/ui/main/postcardDZ_CH6_in",
        ["event:/ui/main/postcardDZ_CH6_out"]               = "event:/pusheen/ui/main/postcardDZ_CH6_out",
        ["event:/ui/main/rename_entry_accept"]            = "event:/pusheen/ui/main/rename_entry_accept",
        ["event:/ui/main/rename_entry_backspace"]         = "event:/pusheen/ui/main/rename_entry_backspace",
        ["event:/ui/main/rename_entryDZ_CHar"]              = "event:/pusheen/ui/main/rename_entryDZ_CHar",
        ["event:/ui/main/rename_entry_rollover"]          = "event:/pusheen/ui/main/rename_entry_rollover",
        ["event:/ui/main/rename_entry_space"]             = "event:/pusheen/ui/main/rename_entry_space",
        ["event:/ui/main/rollover_down"]                  = "event:/pusheen/ui/main/rollover_down",
        ["event:/ui/main/rollover_up"]                    = "event:/pusheen/ui/main/rollover_up",
        ["event:/ui/main/savefile_begin"]                 = "event:/pusheen/ui/main/savefile_begin",
        ["event:/ui/main/savefile_delete"]                = "event:/pusheen/ui/main/savefile_delete",
        ["event:/ui/main/savefile_rename_start"]          = "event:/pusheen/ui/main/savefile_rename_start",
        ["event:/ui/main/savefile_rollover_down"]         = "event:/pusheen/ui/main/savefile_rollover_down",
        ["event:/ui/main/savefile_rollover_first"]        = "event:/pusheen/ui/main/savefile_rollover_first",
        ["event:/ui/main/savefile_rollover_up"]           = "event:/pusheen/ui/main/savefile_rollover_up",
        ["event:/ui/main/title_firstinput"]               = "event:/pusheen/ui/main/title_firstinput",
        ["event:/ui/main/whoosh_large_in"]                = "event:/pusheen/ui/main/whoosh_large_in",
        ["event:/ui/main/whoosh_large_out"]               = "event:/pusheen/ui/main/whoosh_large_out",
        ["event:/ui/main/whoosh_list_in"]                 = "event:/pusheen/ui/main/whoosh_list_in",
        ["event:/ui/main/whoosh_list_out"]                = "event:/pusheen/ui/main/whoosh_list_out",
        ["event:/ui/main/whoosh_savefile_in"]             = "event:/pusheen/ui/main/whoosh_savefile_in",
        ["event:/ui/main/whoosh_savefile_out"]            = "event:/pusheen/ui/main/whoosh_savefile_out",

        // UI Postgame
        ["event:/ui/postgame/crystal_heart"]              = "event:/pusheen/ui/postgame/crystal_heart",
        ["event:/ui/postgame/death_appear"]               = "event:/pusheen/ui/postgame/death_appear",
        ["event:/ui/postgame/death_count"]                = "event:/pusheen/ui/postgame/death_count",
        ["event:/ui/postgame/death_final"]                = "event:/pusheen/ui/postgame/death_final",
        ["event:/ui/postgame/goldberry_count"]            = "event:/pusheen/ui/postgame/goldberry_count",
        ["event:/ui/postgame/strawberry_count"]           = "event:/pusheen/ui/postgame/strawberry_count",
        ["event:/ui/postgame/strawberry_total"]           = "event:/pusheen/ui/postgame/strawberry_total",
        ["event:/ui/postgame/strawberry_total_all"]       = "event:/pusheen/ui/postgame/strawberry_total_all",
        ["event:/ui/postgame/unlock_1"]               = "event:/pusheen/ui/postgame/unlock_1",
        ["event:/ui/postgame/unlock_newchapter"]          = "event:/pusheen/ui/postgame/unlock_newchapter",
        ["event:/ui/postgame/unlock_newchapter_icon"]     = "event:/pusheen/ui/postgame/unlock_newchapter_icon",

        // UI World Map
        ["event:/ui/world_map/chapter/back"]                      = "event:/pusheen/ui/world_map/chapter/back",
        ["event:/ui/world_map/chapter/checkpoint_back"]           = "event:/pusheen/ui/world_map/chapter/checkpoint_back",
        ["event:/ui/world_map/chapter/checkpoint_photo_add"]      = "event:/pusheen/ui/world_map/chapter/checkpoint_photo_add",
        ["event:/ui/world_map/chapter/checkpoint_photo_remove"]   = "event:/pusheen/ui/world_map/chapter/checkpoint_photo_remove",
        ["event:/ui/world_map/chapter/checkpoint_start"]          = "event:/pusheen/ui/world_map/chapter/checkpoint_start",
        ["event:/ui/world_map/chapter/level_select"]              = "event:/pusheen/ui/world_map/chapter/level_select",
        ["event:/ui/world_map/chapter/pane_contract"]             = "event:/pusheen/ui/world_map/chapter/pane_contract",
        ["event:/ui/world_map/chapter/pane_expand"]               = "event:/pusheen/ui/world_map/chapter/pane_expand",
        ["event:/ui/world_map/chapter/tab_roll_left"]             = "event:/pusheen/ui/world_map/chapter/tab_roll_left",
        ["event:/ui/world_map/chapter/tab_roll_right"]            = "event:/pusheen/ui/world_map/chapter/tab_roll_right",
        ["event:/ui/world_map/icon/assist_skip"]                  = "event:/pusheen/ui/world_map/icon/assist_skip",
        ["event:/ui/world_map/icon/flip_left"]                    = "event:/pusheen/ui/world_map/icon/flip_left",
        ["event:/ui/world_map/icon/flip_right"]                   = "event:/pusheen/ui/world_map/icon/flip_right",
        ["event:/ui/world_map/icon/roll_left"]                    = "event:/pusheen/ui/world_map/icon/roll_left",
        ["event:/ui/world_map/icon/roll_right"]                   = "event:/pusheen/ui/world_map/icon/roll_right",
        ["event:/ui/world_map/icon/select"]                       = "event:/pusheen/ui/world_map/icon/select",
        ["event:/ui/world_map/journal/back"]                      = "event:/pusheen/ui/world_map/journal/back",
        ["event:/ui/world_map/journal/heart_grab"]                = "event:/pusheen/ui/world_map/journal/heart_grab",
        ["event:/ui/world_map/journal/heart_release"]             = "event:/pusheen/ui/world_map/journal/heart_release",
        ["event:/ui/world_map/journal/heart_roll"]                = "event:/pusheen/ui/world_map/journal/heart_roll",
        ["event:/ui/world_map/journal/heart_shift_down"]          = "event:/pusheen/ui/world_map/journal/heart_shift_down",
        ["event:/ui/world_map/journal/heart_shift_up"]            = "event:/pusheen/ui/world_map/journal/heart_shift_up",
        ["event:/ui/world_map/journal/page_cover_back"]           = "event:/pusheen/ui/world_map/journal/page_cover_back",
        ["event:/ui/world_map/journal/page_cover_forward"]        = "event:/pusheen/ui/world_map/journal/page_cover_forward",
        ["event:/ui/world_map/journal/page_main_back"]            = "event:/pusheen/ui/world_map/journal/page_main_back",
        ["event:/ui/world_map/journal/page_main_forward"]         = "event:/pusheen/ui/world_map/journal/page_main_forward",
        ["event:/ui/world_map/journal/select"]                    = "event:/pusheen/ui/world_map/journal/select",
        ["event:/ui/world_map/whoosh/1000ms_back"]                = "event:/pusheen/ui/world_map/whoosh/1000ms_back",
        ["event:/ui/world_map/whoosh/1000ms_forward"]             = "event:/pusheen/ui/world_map/whoosh/1000ms_forward",
        ["event:/ui/world_map/whoosh/400ms_back"]                 = "event:/pusheen/ui/world_map/whoosh/400ms_back",
        ["event:/ui/world_map/whoosh/400ms_forward"]              = "event:/pusheen/ui/world_map/whoosh/400ms_forward",
        ["event:/ui/world_map/whoosh/600ms_back"]                 = "event:/pusheen/ui/world_map/whoosh/600ms_back",
        ["event:/ui/world_map/whoosh/600ms_forward"]              = "event:/pusheen/ui/world_map/whoosh/600ms_forward",
        ["event:/ui/world_map/whoosh/700ms_back"]                 = "event:/pusheen/ui/world_map/whoosh/700ms_back",
        ["event:/ui/world_map/whoosh/700ms_forward"]              = "event:/pusheen/ui/world_map/whoosh/700ms_forward",
        ["event:/ui/world_map/whoosh/900ms_back"]                 = "event:/pusheen/ui/world_map/whoosh/900ms_back",
        ["event:/ui/world_map/whoosh/900ms_forward"]              = "event:/pusheen/ui/world_map/whoosh/900ms_forward",
    };

    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;

        // Hook every Audio.Play overload so parameterized one-shots (footsteps,
        // dash sounds, UI rolls, etc.) are also redirected to the custom bank.
        On.Celeste.Audio.Play_string += OnAudioPlay;
        On.Celeste.Audio.Play_string_string_float += OnAudioPlayStringFloat;
        On.Celeste.Audio.Play_string_Vector2 += OnAudioPlayPosition;
        On.Celeste.Audio.Play_string_Vector2_string_float += OnAudioPlayPositionParam;
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float += OnAudioPlayPositionParamParam;
    }

    public static void Unload()
    {
        if (!_loaded) return;
        _loaded = false;

        On.Celeste.Audio.Play_string -= OnAudioPlay;
        On.Celeste.Audio.Play_string_string_float -= OnAudioPlayStringFloat;
        On.Celeste.Audio.Play_string_Vector2 -= OnAudioPlayPosition;
        On.Celeste.Audio.Play_string_Vector2_string_float -= OnAudioPlayPositionParam;
        On.Celeste.Audio.Play_string_Vector2_string_float_string_float -= OnAudioPlayPositionParamParam;
    }

    private static string ReplacePath(string path)
    {
        if (path != null && Replacements.TryGetValue(path, out string replacement))
            return replacement;
        return path;
    }

    private static FMOD.Studio.EventInstance OnAudioPlay(On.Celeste.Audio.orig_Play_string orig, string path)
        => orig(ReplacePath(path));

    private static FMOD.Studio.EventInstance OnAudioPlayStringFloat(
        On.Celeste.Audio.orig_Play_string_string_float orig,
        string path, string param, float value)
        => orig(ReplacePath(path), param, value);

    private static FMOD.Studio.EventInstance OnAudioPlayPosition(
        On.Celeste.Audio.orig_Play_string_Vector2 orig,
        string path, Microsoft.Xna.Framework.Vector2 position)
        => orig(ReplacePath(path), position);

    private static FMOD.Studio.EventInstance OnAudioPlayPositionParam(
        On.Celeste.Audio.orig_Play_string_Vector2_string_float orig,
        string path, Microsoft.Xna.Framework.Vector2 position, string param, float value)
        => orig(ReplacePath(path), position, param, value);

    private static FMOD.Studio.EventInstance OnAudioPlayPositionParamParam(
        On.Celeste.Audio.orig_Play_string_Vector2_string_float_string_float orig,
        string path, Microsoft.Xna.Framework.Vector2 position, string param, float value, string param2, float value2)
        => orig(ReplacePath(path), position, param, value, param2, value2);
}
