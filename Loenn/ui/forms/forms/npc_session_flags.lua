local npcSessionFlags = {}

npcSessionFlags.fieldType = "string"
npcSessionFlags.validator = function(value)
    return true -- All strings are valid session flags
end

-- Session flag suggestions based on English.txt dialog system
npcSessionFlags.options = {
    -- Meeting flags
    "met_theo",
    "met_granny", 
    "met_madeline",
    "met_oshiro",
    "met_badeline",
    "met_magalor",
    "met_kirby",
    "met_chara",
    "met_ralsei",
    "met_sans",
    "met_papyrus",
    "met_toriel",
    "met_asgore",
    "met_undyne",
    "met_gaster",
    "met_asriel",
    "met_flowey",
    
    -- Interaction flags
    "talked_to_theo",
    "talked_to_granny",
    "talked_to_madeline",
    "talked_to_oshiro",
    "talked_to_badeline",
    "talked_to_magalor",
    "talked_to_kirby",
    "talked_to_chara",
    "talked_to_ralsei",
    "talked_to_sans",
    "talked_to_papyrus",
    "talked_to_toriel",
    "talked_to_asgore",
    "talked_to_undyne",
    "talked_to_gaster",
    "talked_to_asriel",
    "talked_to_flowey",
    
    -- Chapter progression flags
    "chapter_0_complete",
    "chapter_1_complete",
    "chapter_2_complete", 
    "chapter_3_complete",
    "chapter_4_complete",
    "chapter_5_complete",
    "chapter_6_complete",
    "chapter_7_complete",
    "chapter_8_complete",
    "chapter_9_complete",
    "chapter_10_complete",
    "chapter_16_complete",
    "chapter_18_complete",
    "chapter_19_complete",
    "chapter_20_complete",
    
    -- Story progression flags
    "prologue_started",
    "prologue_complete",
    "nightmare_started",
    "nightmare_escaped",
    "star_warrior_awakened",
    "unexpected_alliances_formed",
    "resort_visited",
    "stronghold_reached",
    "mirror_hell_entered",
    "mirror_hell_escaped", 
    "outrun_started",
    "summit_reached",
    "ruins_explored",
    "core_accessed",
    "final_battle_started",
    "universe_saved",
    
    -- Character-specific flags
    "chara_befriended",
    "chara_hostile",
    "badeline_merged",
    "kirby_powered_up",
    "ralsei_freed",
    "oshiro_breakdown_witnessed",
    "theo_escaped_hotel",
    "granny_met_afterlife",
    "madeline_crystal_freed",
    "gaster_cured",
    
    -- Item/Power flags
    "heart_gem_obtained",
    "determination_power",
    "star_warrior_power",
    "delta_warrior_power",
    "wavefaze_learned",
    "super_dash_unlocked",
    "dream_feather_obtained",
    "resetia_moonberry_collected",
    "final_heart_received",
    
    -- Quest flags
    "theo_quest_started",
    "theo_quest_complete",
    "granny_quest_started",
    "granny_quest_complete",
    "oshiro_quest_started",
    "oshiro_quest_complete",
    "monster_freedom_quest",
    "universe_save_quest",
    "chara_redemption_quest",
    
    -- Location flags
    "celeste_mountain_climbed",
    "ingeste_mountain_reached",
    "forbidden_city_entered",
    "resort_explored",
    "stronghold_infiltrated",
    "mirror_temple_completed",
    "summit_conquered",
    "ruins_discovered",
    "core_reached",
    "void_entered",
    "space_traversed",
    
    -- Special event flags
    "flowey_encountered",
    "seeker_chase_survived",
    "hotel_breakdown_survived",
    "gondola_panic_overcome",
    "chara_boss_defeated",
    "omega_zero_confronted",
    "final_boss_defeated",
    "universe_reset_prevented",
    
    -- Tutorial flags
    "basic_controls_learned",
    "dash_tutorial_complete",
    "climbing_tutorial_complete",
    "wavefaze_tutorial_complete",
    "npc_interaction_learned",
    
    -- B-Side, C-Side, D-Side flags
    "bside_unlocked",
    "cside_unlocked", 
    "dside_unlocked",
    "rmx_side_unlocked",
    "bside_complete",
    "cside_complete",
    "dside_complete", 
    "rmx_side_complete",
    
    -- Ending flags
    "good_ending_achieved",
    "true_ending_achieved",
    "epilogue_unlocked",
    "monsters_freed",
    "peace_restored",
    "friendship_ending",
    
    -- Collectible flags
    "strawberry_collector",
    "platinum_berry_collected",
    "crystal_heart_collected",
    "cassette_found",
    "golden_berry_collected",
    
    -- Phone call flags
    "phone_call_received",
    "gaster_call_answered",
    "undyne_call_received",
    "sans_call_received",
    "toriel_call_received",
    "theo_call_received",
    
    -- Dream/Mirror flags
    "dream_sequence_entered",
    "mirror_broken",
    "chara_mirror_escaped",
    "dream_feather_used",
    "nightmare_overcome",
    
    -- Boss battle flags
    "chara_boss_phase_1",
    "chara_boss_phase_2", 
    "chara_boss_phase_3",
    "omega_zero_phase_1",
    "omega_zero_phase_2",
    "omega_zero_final_form",
    "boss_save_file_used",
    "boss_loop_broken",
    
    -- Character relationship flags
    "madeline_badeline_reconciled",
    "chara_asriel_remembered",
    "theo_friendship_level_1",
    "theo_friendship_level_2",
    "theo_friendship_level_3",
    "granny_wisdom_received",
    "kirby_mentor_role",
    
    -- Determination flags
    "determination_awakened",
    "determination_mastered",
    "determination_shared",
    "stay_determined",
    "hopes_and_dreams_power",
    
    -- Generic utility flags
    "player_detected",
    "proximity_triggered",
    "interaction_available",
    "one_time_event_used",
    "conditional_met",
    "flag_check_passed",
    "event_triggered",
    "cutscene_played",
    "dialog_seen",
    "tutorial_shown"
}

npcSessionFlags.editable = true
npcSessionFlags.description = "Session flag to set or check (used for game state tracking)"

return npcSessionFlags