-- Cutscene Actions Form Field for Lönn
-- Provides autocomplete for cutscene action types and sequences

local cutsceneActions = {}

cutsceneActions.fieldType = "dropdown"
cutsceneActions.allowEmpty = true
cutsceneActions.editable = true

-- Main cutscene action categories
cutsceneActions.options = {
    -- Character Actions
    "walk_to_position",
    "face_direction", 
    "face_player",
    "face_character",
    "play_animation",
    "set_emotion",
    "gesture",
    "bow",
    "wave",
    "point",
    "nod",
    "shake_head",
    "surprised",
    "happy",
    "sad",
    "angry",
    "worried",
    "confused",
    
    -- Movement Actions
    "move_to_node",
    "move_to_player",
    "move_relative",
    "teleport_to",
    "follow_path",
    "patrol_area",
    "circle_around",
    "back_away",
    "approach_slowly",
    "rush_forward",
    
    -- Dialog Actions
    "say_dialog",
    "start_conversation",
    "end_conversation",
    "interrupt_dialog",
    "whisper",
    "shout",
    "think_bubble",
    "internal_monologue",
    
    -- Camera Actions
    "camera_focus",
    "camera_pan",
    "camera_zoom",
    "camera_shake",
    "camera_follow",
    "camera_return_to_player",
    "camera_smooth_transition",
    "camera_instant_cut",
    
    -- Audio Actions
    "play_music",
    "stop_music",
    "fade_music",
    "play_sound",
    "play_voice",
    "ambient_sound",
    "footstep_sound",
    "interaction_sound",
    
    -- Visual Effects
    "screen_flash",
    "screen_fade",
    "screen_shake",
    "color_tint",
    "blur_effect",
    "particle_effect",
    "lightning_flash",
    "magic_sparkle",
    "dust_cloud",
    "energy_burst",
    
    -- Timing Actions
    "wait",
    "wait_for_dialog",
    "wait_for_animation", 
    "wait_for_player",
    "wait_for_input",
    "pause_briefly",
    "dramatic_pause",
    
    -- Interaction Actions
    "enable_interaction",
    "disable_interaction",
    "force_interaction",
    "end_interaction",
    "show_prompt",
    "hide_prompt",
    
    -- Flag Management
    "set_flag",
    "clear_flag",
    "check_flag",
    "increment_counter",
    "save_state",
    "load_state",
    
    -- Scene Management
    "spawn_entity",
    "despawn_entity",
    "enable_entity",
    "disable_entity",
    "trigger_event",
    "start_minigame",
    "end_minigame",
    
    -- Transition Actions
    "fade_in",
    "fade_out",
    "slide_transition",
    "zoom_transition",
    "spiral_transition",
    "level_transition",
    "chapter_transition",
    
    -- Special Actions
    "freeze_player",
    "unfreeze_player",
    "give_ability",
    "take_ability",
    "heal_player",
    "damage_player",
    "checkpoint_save",
    "respawn_player",
    
    -- Tutorial Actions
    "show_tutorial",
    "highlight_control",
    "show_objective",
    "complete_objective",
    "tutorial_prompt",
    "practice_mode",
    
    -- Boss Actions
    "boss_enter",
    "boss_attack",
    "boss_vulnerable",
    "boss_phase_change",
    "boss_defeated",
    "boss_escape",
    
    -- Dream/Memory Actions
    "dream_sequence_start",
    "dream_sequence_end",
    "flashback_start",
    "flashback_end",
    "memory_fragment",
    "nightmare_effect",
    
    -- Environmental Actions
    "weather_change",
    "time_of_day",
    "lighting_change",
    "background_change",
    "foreground_change",
    "wind_effect",
    "rain_effect",
    "snow_effect"
}

-- Predefined action sequences for common cutscene types
cutsceneActions.sequences = {
    basic_greeting = "approach_slowly,wave,say_dialog,wait,nod,walk_away",
    dramatic_reveal = "camera_pan,dramatic_pause,camera_zoom,screen_flash,play_music,say_dialog",
    boss_introduction = "boss_enter,camera_focus,play_music,boss_attack,screen_shake,say_dialog",
    tutorial_start = "freeze_player,show_tutorial,highlight_control,wait_for_input,unfreeze_player",
    character_meeting = "face_each_other,surprised,pause_briefly,say_dialog,happy,nod",
    farewell_sequence = "sad,say_dialog,wave,walk_away,fade_out,set_flag",
    power_up_sequence = "magic_sparkle,give_ability,energy_burst,play_sound,happy",
    dream_transition = "fade_out,dream_sequence_start,color_tint,play_music,fade_in"
}

return cutsceneActions