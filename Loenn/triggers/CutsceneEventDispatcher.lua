local eventOptions = {
    "ch0_theo",
    "ch0_ending",
    "mod_city_end_1",
    "ch1_mod_city_end",
    "ch2_chara_intro",
    "ch2_chara_mirror",
    "chara_trap",
    "ch2_chara_trap",
    "call_kirby",
    "ch2_call_kirby",
    "ch2_journal",
    "ch3_meetup",
    "ch3_first_step",
    "mod_city_end_2",
    "ch3_mod_city_end",
    "ch4_escape",
    "ch4_call_mom",
    "ch4_mirror",
    "ch4_chara_warning",
    "ch4_ending",
    "ch5_see_maddy",
    "ch5_diary",
    "ch5_guestbook",
    "ch5_memo",
    "ch5_maddy_phone",
    "ch5_ending",
    "ch5_magorlor_escape",
    "ch5_oshiro_lobby",
    "ch5_oshiro_clutter",
    "ch6_intro",
    "ch6_stronghold",
    "ch6_end",
    "ch7_enter",
    "ch7_intro",
    "ch7_see_maddy_mirror",
    "ch7_genocide_mirror_portal",
    "ch7_genocide_vision_intro",
    "ch7_genocide_vision_finale",
    "ch7_genocide_wakeup",
    "ch7_pre_ingeste",
    "ch7_see_maddy",
    "ch7_found_maddy",
    "ch7_mirror_portal",
    "ch7_darker",
    "ch8_plat",
    "ch8_reflectionmod",
    "ch8_theo",
    "ch8_charaboss_intro",
    "ch8_intro_chara_boss",
    "ch8_chara_boss_mid",
    "ch8_chara_boss_center",
    "ch8_chara_boss_end",
    "ch8_end",
    "ch8_star_jump_end",
    "ch9_arrivial",
    "ch9_fake_saved",
    "ch9_credits",
    "ch9_message_end",
    "ch9_end",
    "ch10_intro",
    "CH10_flowey_intro",
    "CH10_flowey_intro_scene",
    "ch10_house",
    "ch10_house_indoor",
    "ch10_house_outdoor",
    "ch10_pre_boss",
    "ch10_post_boss",
    "ch10_piano_start",
    "ch10_roxus_start",
    "ch10_sans_restaurant",
    "ch10_titan_tower_approach",
    "ch10_titan_boss_battle",
    "ch10_maddy_baddy_chara_intro",
    "ch11_intro",
    "ch11_town",
    "ch11_marlet_pre_boss",
    "ch11_marlet_boss_end",
    "ch11_bar_arrival",
    "ch11_cowboy_bar_intro",
    "ch11_cinematic_bar",
    "ch11_maggy",
    "ch11_maggy_end",
    "ch11_collecting_mini_heart_enough",
    "ch11_boss_intro",
    "ch11_boss_mid",
    "ch11_boss_outro",
    "ch11_starlo_and_marlet",
    "ch12_intro",
    "ch12_titan_tower",
    "ch12_titan_pre_boss",
    "ch12_undyne_refused_to_died",
    "ch12_titan_post_boss",
    "ch12_end",
    "ch13_hot_lava",
    "ch13_axis_intro",
    "ch13_well_prepared",
    "ch13_axis_pre_boss",
    "ch13_axis_post_boss",
    "ch13_end",
    "ch13_intro",
    "ch13_meta_knight_encounter",
    "ch13_axis_boss_battle",
    "ch14_intro_core",
    "ch14_giga_axis_pre_boss",
    "ch14_giga_axis_post_boss",
    "ch14_enter_last_elevator",
    "ch14_intro",
    "ch14_hollow_programmer",
    "ch14_giant_axis_battle",
    "ch15_exit_last_elevator",
    "ch15_zantas_1",
    "ch15_zantas_2",
    "ch15_judgement",
    "ch15_intro_roaring_titan",
    "ch15_barrier",
    "ch15_roaring_titan_pre_boss",
    "ch15_roaring_titan_post_boss",
    "ch15_flowey_transformation",
    "ch15_mountain_peak_arrival",
    "ch15_roaring_titan_king_battle",
    "ch15_titan_council_judgment",
    "ch16_els_intro",
    "ch16_els_finale",
    "ch16_els_outro",
    "ch16_exited",
    "ch16_end",
    "ch16_Epilouge",
    "ch16_barrier_breaks",
    "ch16_corrupted_reality_intro",
    "ch16_lost_souls_unite",
    "ch16_save_file_battle",
    "ch17_epilouge",
    "ch17_credits",
    "ch17_ending_mod",
    "ch18_outro",
    "ch19_another_dimension_intro",
    "ch19_goto_the_future",
    "ch19_goto_the_past",
    "ch19_chara_help",
    "ch19_hub_second_intro",
    "ch19_hub_second_intro_trigger",
    "ch19_loop",
    "ch19_big_final_room",
    "ch19_free_bird",
    "ch19_kill_the_bird",
    "ch19_miss_the_bird",
    "ch19_gravestone",
    "ch19_final_launch",
    "ch19_souls_discarded",
    "ch19_beyond_the_void",
    "ch19_memories_of_the_past",
    "ch19_els_breaks_free",
    "ch19_broken_star_warrior",
    "ch19_edge_of_universe",
    "ch20_bird_guidance_intro",
    "ch20_intro",
    "ch20_fake_madeline_and_badeline",
    "ch20_tess_fake_pre_boss",
    "ch20_tess_fake_post_boss",
    "ch20_nothiness",
    "ch20_asriel_god_boss_identity_reveal",
    "ch20_asriel_god_boss_identity_reveal_trigger",
    "ch20_tess_pre_boss_for_real",
    "ch20_saved",
    "ch20_ending",
    "ch20_asriel_angel_of_death_boss_intro",
    "ch20_boss_mid",
    "ch20_boss_end",
    "ch20_asriel_boss_end",
    "ch20_final_boss_defeat",
    "ch20_rainbow_blossom_tree",
    "ch20_restoration_and_farewell",
    "ch20_end_later",
    "ch20_end_cinematic",
    "ch20_white_cymbal_fade_teleport_video",
    "ch21_beaches",
    "ch21_special_thanks",
    "ch21_epilogue_credits",
    "ch21_two_worlds_unite"
}

local cutsceneEventDispatcher = {}

cutsceneEventDispatcher.name = "MaggyHelper/CutsceneEventDispatcher"
cutsceneEventDispatcher.canResize = {true, true}
cutsceneEventDispatcher.placements = {
    {
        name = "default",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            event = ""
        }
    },
    {
        name = "chapter_intro",
        data = {
            x = 0,
            y = 0,
            width = 24,
            height = 16,
            event = "ch10_intro"
        }
    },
    {
        name = "boss_intro",
        data = {
            x = 0,
            y = 0,
            width = 32,
            height = 16,
            event = "ch11_boss_intro"
        }
    },
    {
        name = "boss_mid",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            event = "ch20_boss_mid"
        }
    },
    {
        name = "boss_end",
        data = {
            x = 0,
            y = 0,
            width = 24,
            height = 16,
            event = "ch20_boss_end"
        }
    },
    {
        name = "ending",
        data = {
            x = 0,
            y = 0,
            width = 32,
            height = 16,
            event = "ch20_ending"
        }
    },
    {
        name = "credits",
        data = {
            x = 0,
            y = 0,
            width = 32,
            height = 16,
            event = "ch21_epilogue_credits"
        }
    }
}

cutsceneEventDispatcher.fieldInformation = {
    event = {
        fieldType = "string",
        editable = true,
        options = eventOptions
    }
}

cutsceneEventDispatcher.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "event"
}

function cutsceneEventDispatcher.color(room, entity)
    if entity.event == nil or entity.event == "" then
        return {0.85, 0.35, 0.35, 0.8}
    end

    return {0.35, 0.8, 0.95, 0.8}
end

function cutsceneEventDispatcher.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        {
            texture = "ahorn/entityTrigger",
            x = entity.x,
            y = entity.y,
            scaleX = width / 8,
            scaleY = height / 8,
            color = cutsceneEventDispatcher.color(room, entity)
        }
    }
end

function cutsceneEventDispatcher.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        entity.x,
        entity.y,
        width,
        height
    }
end

return cutsceneEventDispatcher