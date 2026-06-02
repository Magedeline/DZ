-- Node Actions Form Field for Lönn
-- Provides autocomplete for NPC node-based actions and behaviors

local nodeActions = {}

nodeActions.fieldType = "dropdown"
nodeActions.allowEmpty = true
nodeActions.editable = true

-- Node action categories
nodeActions.options = {
    -- Movement Actions
    "walk_to_node",
    "run_to_node", 
    "sneak_to_node",
    "float_to_node",
    "teleport_to_node",
    "jump_to_node",
    "climb_to_node",
    "fly_to_node",
    
    -- Wait Actions
    "wait_at_node",
    "idle_at_node",
    "look_around",
    "rest_at_node",
    "guard_position",
    "patrol_from_node",
    
    -- Interaction Actions
    "interact_with_object",
    "pick_up_item",
    "examine_area",
    "operate_switch",
    "open_door",
    "close_door",
    "ring_bell",
    "knock_on_door",
    
    -- Animation Actions
    "play_animation",
    "loop_animation",
    "random_animation",
    "emotion_change",
    "gesture_at_node",
    "point_to_next",
    "beckon_player",
    "wave_goodbye",
    
    -- Dialog Actions
    "say_line",
    "start_dialog",
    "monologue",
    "call_out",
    "whisper",
    "sing",
    "laugh",
    "cry",
    
    -- Combat Actions
    "attack_position",
    "defend_position",
    "charge_attack",
    "ranged_attack",
    "cast_spell",
    "block",
    "dodge",
    "retreat",
    
    -- Patrol Actions
    "patrol_forward",
    "patrol_backward",
    "patrol_random",
    "change_direction",
    "reverse_path",
    "skip_to_node",
    "return_to_start",
    "end_patrol",
    
    -- Environmental Actions
    "trigger_event",
    "activate_mechanism",
    "light_torch",
    "extinguish_fire",
    "water_plants",
    "feed_animals",
    "clean_area",
    "repair_object",
    
    -- Timing Actions
    "wait_for_player",
    "wait_for_signal",
    "wait_for_dialog",
    "wait_random_time",
    "schedule_action",
    "timed_sequence",
    "countdown_timer",
    "alarm_trigger",
    
    -- Conditional Actions
    "check_flag",
    "check_item",
    "check_health",
    "check_time",
    "check_weather",
    "check_player_state",
    "check_distance",
    "check_visibility",
    
    -- State Changes
    "set_state_active",
    "set_state_inactive", 
    "set_state_hostile",
    "set_state_friendly",
    "set_state_neutral",
    "set_state_sleeping",
    "set_state_alert",
    "set_state_confused",
    
    -- Sound Actions
    "play_footstep",
    "play_voice_clip",
    "play_ambient_sound",
    "play_action_sound",
    "stop_all_sounds",
    "fade_audio",
    "echo_call",
    "sound_alarm",
    
    -- Special Actions
    "transform",
    "become_invisible",
    "become_visible",
    "phase_through",
    "duplicate_self",
    "merge_with_other",
    "split_into_parts",
    "power_up",
    
    -- Tutorial Actions
    "show_hint",
    "highlight_object",
    "demonstrate_action",
    "teach_mechanic",
    "correct_player",
    "encourage_player",
    "provide_feedback",
    "unlock_ability",
    
    -- Cutscene Actions
    "cutscene_walk",
    "cutscene_talk",
    "cutscene_gesture",
    "cutscene_wait",
    "cutscene_exit",
    "camera_focus_here",
    "dramatic_entrance",
    "dramatic_exit"
}

-- Predefined action sequences for common NPC behaviors
nodeActions.sequences = {
    simple_patrol = "walk_to_node,wait_at_node,look_around,patrol_forward",
    guard_duty = "walk_to_node,guard_position,look_around,patrol_from_node",
    shopkeeper = "walk_to_node,idle_at_node,interact_with_object,say_line",
    quest_giver = "walk_to_node,beckon_player,start_dialog,gesture_at_node",
    hostile_enemy = "walk_to_node,set_state_hostile,attack_position,charge_attack",
    friendly_helper = "walk_to_node,set_state_friendly,wave_goodbye,provide_feedback",
    tutorial_guide = "walk_to_node,show_hint,demonstrate_action,encourage_player",
    boss_sequence = "dramatic_entrance,set_state_hostile,attack_position,power_up",
    cutscene_actor = "cutscene_walk,camera_focus_here,cutscene_talk,dramatic_exit"
}

-- Node action categories for filtering
nodeActions.categories = {
    movement = {"walk_to_node", "run_to_node", "sneak_to_node", "float_to_node", "teleport_to_node"},
    interaction = {"interact_with_object", "pick_up_item", "examine_area", "operate_switch"},
    combat = {"attack_position", "defend_position", "charge_attack", "ranged_attack"},
    dialog = {"say_line", "start_dialog", "monologue", "call_out"},
    timing = {"wait_at_node", "wait_for_player", "wait_for_signal", "timed_sequence"},
    special = {"transform", "become_invisible", "power_up", "duplicate_self"}
}

return nodeActions