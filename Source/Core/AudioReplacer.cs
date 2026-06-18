using Celeste;

namespace DZ;

/// <summary>
/// Intercepts Audio.Play calls and redirects vanilla event paths to
/// their custom Pusheen equivalents in the mod's FMOD banks.
/// </summary>
public static class AudioReplacer
{
    private static bool _loaded;

    private static readonly Dictionary<string, string> Replacements = new()
    {
        // Granny SFX
        ["event:/char/granny/cane_tap"]            = "event:/Mods/pusheen/char/granny/cane_tap",
        ["event:/char/granny/laugh_firstphrase"]   = "event:/Mods/pusheen/char/granny/laugh_firstphrase",
        ["event:/char/granny/laugh_oneha"]         = "event:/Mods/pusheen/char/granny/laugh_oneha",

        // Madeline SFX
        ["event:/char/madeline/jump"]              = "event:/Mods/pusheen/char/madeline/jump",
        ["event:/char/madeline/landing"]           = "event:/Mods/pusheen/char/madeline/landing",
        ["event:/char/madeline/death"]             = "event:/Mods/pusheen/char/madeline/death",
        ["event:/char/madeline/footstep"]          = "event:/Mods/pusheen/char/madeline/footstep",
        ["event:/char/madeline/dash_pink_left"]    = "event:/Mods/pusheen/char/madeline/dash_pink_left",
        ["event:/char/madeline/dash_pink_right"]   = "event:/Mods/pusheen/char/madeline/dash_pink_right",
        ["event:/char/madeline/dash_red_left"]     = "event:/Mods/pusheen/char/madeline/dash_red_left",
        ["event:/char/madeline/dash_red_right"]    = "event:/Mods/pusheen/char/madeline/dash_red_right",

        // Badeline SFX
        ["event:/char/badeline/jump"]              = "event:/Mods/pusheen/char/badeline/jump",
        ["event:/char/badeline/landing"]           = "event:/Mods/pusheen/char/badeline/landing",
        ["event:/char/badeline/dash_red_left"]     = "event:/Mods/pusheen/char/badeline/dash_red_left",
        ["event:/char/badeline/dash_red_right"]    = "event:/Mods/pusheen/char/badeline/dash_red_right",

        // Oshiro SFX
        ["event:/char/oshiro/boss_charge"]         = "event:/Mods/pusheen/char/oshiro/boss_charge",
        ["event:/char/oshiro/boss_slam_first"]     = "event:/Mods/pusheen/char/oshiro/boss_slam_first",
        ["event:/char/oshiro/boss_slam_final"]     = "event:/Mods/pusheen/char/oshiro/boss_slam_final",

        // Theo SFX
        ["event:/char/theo/phone_taps_loop"]              = "event:/Mods/pusheen/char/theo/phone_taps_loop",
        ["event:/char/theo/resort_ceilingvent_hey"]       = "event:/Mods/pusheen/char/theo/resort_ceilingvent_hey",
        ["event:/char/theo/yolo_fist"]                    = "event:/Mods/pusheen/char/theo/yolo_fist",

        // UI Game
        ["event:/ui/game/chatoptions_appear"]             = "event:/Mods/pusheen/ui/game/chatoptions_appear",
        ["event:/ui/game/chatoptions_roll_down"]          = "event:/Mods/pusheen/ui/game/chatoptions_roll_down",
        ["event:/ui/game/chatoptions_roll_up"]            = "event:/Mods/pusheen/ui/game/chatoptions_roll_up",
        ["event:/ui/game/chatoptions_select"]             = "event:/Mods/pusheen/ui/game/chatoptions_select",
        ["event:/ui/game/general_text_loop"]              = "event:/Mods/pusheen/ui/game/general_text_loop",
        ["event:/ui/game/hotspot_main_in"]                = "event:/Mods/pusheen/ui/game/hotspot_main_in",
        ["event:/ui/game/hotspot_main_out"]               = "event:/Mods/pusheen/ui/game/hotspot_main_out",
        ["event:/ui/game/hotspot_note_in"]                = "event:/Mods/pusheen/ui/game/hotspot_note_in",
        ["event:/ui/game/hotspot_note_out"]               = "event:/Mods/pusheen/ui/game/hotspot_note_out",
        ["event:/ui/game/increment_dashcount"]            = "event:/Mods/pusheen/ui/game/increment_dashcount",
        ["event:/ui/game/increment_strawberry"]           = "event:/Mods/pusheen/ui/game/increment_strawberry",
        ["event:/ui/game/lookout_off"]                    = "event:/Mods/pusheen/ui/game/lookout_off",
        ["event:/ui/game/lookout_on"]                     = "event:/Mods/pusheen/ui/game/lookout_on",
        ["event:/ui/game/memorial_dream_loop"]            = "event:/Mods/pusheen/ui/game/memorial_dream_loop",
        ["event:/ui/game/memorial_dream_text_in"]         = "event:/Mods/pusheen/ui/game/memorial_dream_text_in",
        ["event:/ui/game/memorial_dream_text_loop"]       = "event:/Mods/pusheen/ui/game/memorial_dream_text_loop",
        ["event:/ui/game/memorial_dream_text_out"]        = "event:/Mods/pusheen/ui/game/memorial_dream_text_out",
        ["event:/ui/game/memorial_text_in"]               = "event:/Mods/pusheen/ui/game/memorial_text_in",
        ["event:/ui/game/memorial_text_loop"]             = "event:/Mods/pusheen/ui/game/memorial_text_loop",
        ["event:/ui/game/memorial_text_out"]              = "event:/Mods/pusheen/ui/game/memorial_text_out",
        ["event:/ui/game/pause"]                          = "event:/Mods/pusheen/ui/game/pause",
        ["event:/ui/game/textadvance_madeline"]           = "event:/Mods/pusheen/ui/game/textadvance_madeline",
        ["event:/ui/game/textadvance_other"]              = "event:/Mods/pusheen/ui/game/textadvance_other",
        ["event:/ui/game/textbox_madeline_in"]            = "event:/Mods/pusheen/ui/game/textbox_madeline_in",
        ["event:/ui/game/textbox_madeline_out"]           = "event:/Mods/pusheen/ui/game/textbox_madeline_out",
        ["event:/ui/game/textbox_other_in"]               = "event:/Mods/pusheen/ui/game/textbox_other_in",
        ["event:/ui/game/textbox_other_out"]              = "event:/Mods/pusheen/ui/game/textbox_other_out",
        ["event:/ui/game/tutorial_note_flip_back"]        = "event:/Mods/pusheen/ui/game/tutorial_note_flip_back",
        ["event:/ui/game/tutorial_note_flip_front"]       = "event:/Mods/pusheen/ui/game/tutorial_note_flip_front",
        ["event:/ui/game/unpause"]                        = "event:/Mods/pusheen/ui/game/unpause",

        // UI Main
        ["event:/ui/main/assist_button_info"]             = "event:/Mods/pusheen/ui/main/assist_button_info",
        ["event:/ui/main/assist_button_no"]               = "event:/Mods/pusheen/ui/main/assist_button_no",
        ["event:/ui/main/assist_button_yes"]              = "event:/Mods/pusheen/ui/main/assist_button_yes",
        ["event:/ui/main/assist_info_whistle"]            = "event:/Mods/pusheen/ui/main/assist_info_whistle",
        ["event:/ui/main/bside_intro_text"]               = "event:/Mods/pusheen/ui/main/bside_intro_text",
        ["event:/ui/main/button_back"]                    = "event:/Mods/pusheen/ui/main/button_back",
        ["event:/ui/main/button_climb"]                   = "event:/Mods/pusheen/ui/main/button_climb",
        ["event:/ui/main/button_invalid"]                 = "event:/Mods/pusheen/ui/main/button_invalid",
        ["event:/ui/main/button_lowkey"]                  = "event:/Mods/pusheen/ui/main/button_lowkey",
        ["event:/ui/main/button_select"]                  = "event:/Mods/pusheen/ui/main/button_select",
        ["event:/ui/main/button_toggle_off"]              = "event:/Mods/pusheen/ui/main/button_toggle_off",
        ["event:/ui/main/button_toggle_on"]               = "event:/Mods/pusheen/ui/main/button_toggle_on",
        ["event:/ui/main/message_confirm"]                = "event:/Mods/pusheen/ui/main/message_confirm",
        ["event:/ui/main/postcard_ch1_in"]                = "event:/Mods/pusheen/ui/main/postcard_ch1_in",
        ["event:/ui/main/postcard_ch1_out"]               = "event:/Mods/pusheen/ui/main/postcard_ch1_out",
        ["event:/ui/main/postcard_ch2_in"]                = "event:/Mods/pusheen/ui/main/postcard_ch2_in",
        ["event:/ui/main/postcard_ch2_out"]               = "event:/Mods/pusheen/ui/main/postcard_ch2_out",
        ["event:/ui/main/postcard_ch3_in"]                = "event:/Mods/pusheen/ui/main/postcard_ch3_in",
        ["event:/ui/main/postcard_ch3_out"]               = "event:/Mods/pusheen/ui/main/postcard_ch3_out",
        ["event:/ui/main/postcard_ch4_in"]                = "event:/Mods/pusheen/ui/main/postcard_ch4_in",
        ["event:/ui/main/postcard_ch4_out"]               = "event:/Mods/pusheen/ui/main/postcard_ch4_out",
        ["event:/ui/main/postcard_ch5_in"]                = "event:/Mods/pusheen/ui/main/postcard_ch5_in",
        ["event:/ui/main/postcard_ch5_out"]               = "event:/Mods/pusheen/ui/main/postcard_ch5_out",
        ["event:/ui/main/postcard_ch6_in"]                = "event:/Mods/pusheen/ui/main/postcard_ch6_in",
        ["event:/ui/main/postcard_ch6_out"]               = "event:/Mods/pusheen/ui/main/postcard_ch6_out",
        ["event:/ui/main/postcard_csides_in"]             = "event:/Mods/pusheen/ui/main/postcard_csides_in",
        ["event:/ui/main/postcard_csides_out"]            = "event:/Mods/pusheen/ui/main/postcard_csides_out",
        ["event:/ui/main/rename_entry_accept"]            = "event:/Mods/pusheen/ui/main/rename_entry_accept",
        ["event:/ui/main/rename_entry_backspace"]         = "event:/Mods/pusheen/ui/main/rename_entry_backspace",
        ["event:/ui/main/rename_entry_char"]              = "event:/Mods/pusheen/ui/main/rename_entry_char",
        ["event:/ui/main/rename_entry_rollover"]          = "event:/Mods/pusheen/ui/main/rename_entry_rollover",
        ["event:/ui/main/rename_entry_space"]             = "event:/Mods/pusheen/ui/main/rename_entry_space",
        ["event:/ui/main/rollover_down"]                  = "event:/Mods/pusheen/ui/main/rollover_down",
        ["event:/ui/main/rollover_up"]                    = "event:/Mods/pusheen/ui/main/rollover_up",
        ["event:/ui/main/savefile_begin"]                 = "event:/Mods/pusheen/ui/main/savefile_begin",
        ["event:/ui/main/savefile_delete"]                = "event:/Mods/pusheen/ui/main/savefile_delete",
        ["event:/ui/main/savefile_rename_start"]          = "event:/Mods/pusheen/ui/main/savefile_rename_start",
        ["event:/ui/main/savefile_rollover_down"]         = "event:/Mods/pusheen/ui/main/savefile_rollover_down",
        ["event:/ui/main/savefile_rollover_first"]        = "event:/Mods/pusheen/ui/main/savefile_rollover_first",
        ["event:/ui/main/savefile_rollover_up"]           = "event:/Mods/pusheen/ui/main/savefile_rollover_up",
        ["event:/ui/main/title_firstinput"]               = "event:/Mods/pusheen/ui/main/title_firstinput",
        ["event:/ui/main/whoosh_large_in"]                = "event:/Mods/pusheen/ui/main/whoosh_large_in",
        ["event:/ui/main/whoosh_large_out"]               = "event:/Mods/pusheen/ui/main/whoosh_large_out",
        ["event:/ui/main/whoosh_list_in"]                 = "event:/Mods/pusheen/ui/main/whoosh_list_in",
        ["event:/ui/main/whoosh_list_out"]                = "event:/Mods/pusheen/ui/main/whoosh_list_out",
        ["event:/ui/main/whoosh_savefile_in"]             = "event:/Mods/pusheen/ui/main/whoosh_savefile_in",
        ["event:/ui/main/whoosh_savefile_out"]            = "event:/Mods/pusheen/ui/main/whoosh_savefile_out",

        // UI Postgame
        ["event:/ui/postgame/crystal_heart"]              = "event:/Mods/pusheen/ui/postgame/crystal_heart",
        ["event:/ui/postgame/death_appear"]               = "event:/Mods/pusheen/ui/postgame/death_appear",
        ["event:/ui/postgame/death_count"]                = "event:/Mods/pusheen/ui/postgame/death_count",
        ["event:/ui/postgame/death_final"]                = "event:/Mods/pusheen/ui/postgame/death_final",
        ["event:/ui/postgame/goldberry_count"]            = "event:/Mods/pusheen/ui/postgame/goldberry_count",
        ["event:/ui/postgame/strawberry_count"]           = "event:/Mods/pusheen/ui/postgame/strawberry_count",
        ["event:/ui/postgame/strawberry_total"]           = "event:/Mods/pusheen/ui/postgame/strawberry_total",
        ["event:/ui/postgame/strawberry_total_all"]       = "event:/Mods/pusheen/ui/postgame/strawberry_total_all",
        ["event:/ui/postgame/unlock_bside"]               = "event:/Mods/pusheen/ui/postgame/unlock_bside",
        ["event:/ui/postgame/unlock_newchapter"]          = "event:/Mods/pusheen/ui/postgame/unlock_newchapter",
        ["event:/ui/postgame/unlock_newchapter_icon"]     = "event:/Mods/pusheen/ui/postgame/unlock_newchapter_icon",

        // UI World Map
        ["event:/ui/world_map/chapter/back"]                      = "event:/Mods/pusheen/ui/world_map/chapter/back",
        ["event:/ui/world_map/chapter/checkpoint_back"]           = "event:/Mods/pusheen/ui/world_map/chapter/checkpoint_back",
        ["event:/ui/world_map/chapter/checkpoint_photo_add"]      = "event:/Mods/pusheen/ui/world_map/chapter/checkpoint_photo_add",
        ["event:/ui/world_map/chapter/checkpoint_photo_remove"]   = "event:/Mods/pusheen/ui/world_map/chapter/checkpoint_photo_remove",
        ["event:/ui/world_map/chapter/checkpoint_start"]          = "event:/Mods/pusheen/ui/world_map/chapter/checkpoint_start",
        ["event:/ui/world_map/chapter/level_select"]              = "event:/Mods/pusheen/ui/world_map/chapter/level_select",
        ["event:/ui/world_map/chapter/pane_contract"]             = "event:/Mods/pusheen/ui/world_map/chapter/pane_contract",
        ["event:/ui/world_map/chapter/pane_expand"]               = "event:/Mods/pusheen/ui/world_map/chapter/pane_expand",
        ["event:/ui/world_map/chapter/tab_roll_left"]             = "event:/Mods/pusheen/ui/world_map/chapter/tab_roll_left",
        ["event:/ui/world_map/chapter/tab_roll_right"]            = "event:/Mods/pusheen/ui/world_map/chapter/tab_roll_right",
        ["event:/ui/world_map/icon/assist_skip"]                  = "event:/Mods/pusheen/ui/world_map/icon/assist_skip",
        ["event:/ui/world_map/icon/flip_left"]                    = "event:/Mods/pusheen/ui/world_map/icon/flip_left",
        ["event:/ui/world_map/icon/flip_right"]                   = "event:/Mods/pusheen/ui/world_map/icon/flip_right",
        ["event:/ui/world_map/icon/roll_left"]                    = "event:/Mods/pusheen/ui/world_map/icon/roll_left",
        ["event:/ui/world_map/icon/roll_right"]                   = "event:/Mods/pusheen/ui/world_map/icon/roll_right",
        ["event:/ui/world_map/icon/select"]                       = "event:/Mods/pusheen/ui/world_map/icon/select",
        ["event:/ui/world_map/journal/back"]                      = "event:/Mods/pusheen/ui/world_map/journal/back",
        ["event:/ui/world_map/journal/heart_grab"]                = "event:/Mods/pusheen/ui/world_map/journal/heart_grab",
        ["event:/ui/world_map/journal/heart_release"]             = "event:/Mods/pusheen/ui/world_map/journal/heart_release",
        ["event:/ui/world_map/journal/heart_roll"]                = "event:/Mods/pusheen/ui/world_map/journal/heart_roll",
        ["event:/ui/world_map/journal/heart_shift_down"]          = "event:/Mods/pusheen/ui/world_map/journal/heart_shift_down",
        ["event:/ui/world_map/journal/heart_shift_up"]            = "event:/Mods/pusheen/ui/world_map/journal/heart_shift_up",
        ["event:/ui/world_map/journal/page_cover_back"]           = "event:/Mods/pusheen/ui/world_map/journal/page_cover_back",
        ["event:/ui/world_map/journal/page_cover_forward"]        = "event:/Mods/pusheen/ui/world_map/journal/page_cover_forward",
        ["event:/ui/world_map/journal/page_main_back"]            = "event:/Mods/pusheen/ui/world_map/journal/page_main_back",
        ["event:/ui/world_map/journal/page_main_forward"]         = "event:/Mods/pusheen/ui/world_map/journal/page_main_forward",
        ["event:/ui/world_map/journal/select"]                    = "event:/Mods/pusheen/ui/world_map/journal/select",
        ["event:/ui/world_map/whoosh/1000ms_back"]                = "event:/Mods/pusheen/ui/world_map/whoosh/1000ms_back",
        ["event:/ui/world_map/whoosh/1000ms_forward"]             = "event:/Mods/pusheen/ui/world_map/whoosh/1000ms_forward",
        ["event:/ui/world_map/whoosh/400ms_back"]                 = "event:/Mods/pusheen/ui/world_map/whoosh/400ms_back",
        ["event:/ui/world_map/whoosh/400ms_forward"]              = "event:/Mods/pusheen/ui/world_map/whoosh/400ms_forward",
        ["event:/ui/world_map/whoosh/600ms_back"]                 = "event:/Mods/pusheen/ui/world_map/whoosh/600ms_back",
        ["event:/ui/world_map/whoosh/600ms_forward"]              = "event:/Mods/pusheen/ui/world_map/whoosh/600ms_forward",
        ["event:/ui/world_map/whoosh/700ms_back"]                 = "event:/Mods/pusheen/ui/world_map/whoosh/700ms_back",
        ["event:/ui/world_map/whoosh/700ms_forward"]              = "event:/Mods/pusheen/ui/world_map/whoosh/700ms_forward",
        ["event:/ui/world_map/whoosh/900ms_back"]                 = "event:/Mods/pusheen/ui/world_map/whoosh/900ms_back",
        ["event:/ui/world_map/whoosh/900ms_forward"]              = "event:/Mods/pusheen/ui/world_map/whoosh/900ms_forward",
    };

    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;
        On.Celeste.Audio.Play_string += OnAudioPlay;
        On.Celeste.Audio.Play_string_Vector2 += OnAudioPlayPosition;
    }

    public static void Unload()
    {
        if (!_loaded) return;
        _loaded = false;
        On.Celeste.Audio.Play_string -= OnAudioPlay;
        On.Celeste.Audio.Play_string_Vector2 -= OnAudioPlayPosition;
    }

    private static FMOD.Studio.EventInstance OnAudioPlay(On.Celeste.Audio.orig_Play_string orig, string path)
    {
        if (path != null && Replacements.TryGetValue(path, out string replacement))
            path = replacement;
        return orig(path);
    }

    private static FMOD.Studio.EventInstance OnAudioPlayPosition(On.Celeste.Audio.orig_Play_string_Vector2 orig, string path, Microsoft.Xna.Framework.Vector2 position)
    {
        if (path != null && Replacements.TryGetValue(path, out string replacement))
            path = replacement;
        return orig(path, position);
    }
}
