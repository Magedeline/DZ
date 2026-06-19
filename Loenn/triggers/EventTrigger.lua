local eventOptions = {
    -- Vanilla events
    "end_city",
    "end_oldsite_dream",
    "end_oldsite_awake",
    "ch5_see_theo",
    "ch5_found_theo",
    "ch5_mirror_reflection",
    "cancel_ch5_see_theo",
    "ch6_boss_intro",
    "ch6_reflect",
    "ch7_summit",
    "ch8_door",
    "ch9_goto_the_future",
    "ch9_goto_the_past",
    "ch9_moon_intro",
    "ch9_hub_intro",
    "ch9_hub_transition_out",
    "ch9_badeline_helps",
    "ch9_farewell",
    "ch9_ending",
    "ch9_end_golden",
    "ch9_final_room",
    "ch9_ding_ding_ding",
    "ch9_golden_snapshot",

    -- Chapter 1
    "cs01_mod_ending",

    -- Chapter 2
    "cs02_chara_intro",

    -- Chapter 3
    "cs03_first_step",
    "cs03_meetup",
    "cs03_mod_ending",

    -- Chapter 4
    "cs04_chara_warning",

    -- Chapter 7
    "cs07_darker",
    "cs07_genocide_vision_finale",
    "cs07_genocide_vision_intro",
    "cs07_genocide_wakeup",

    -- Chapter 9
    "cs09_area_complete",
    "cs09_credits",
    "cs09_golden_flower",
    "cs09_message_end",

    -- Chapter 13
    "ch13_tenna_pre_intro_video",
    "ch13_tenna_earth_video",

    -- Chapter 15
    "ch15_zantas_1",
    "ch15_zantas_2",

    -- Chapter 16
    "cs16_barrier_breaks",
    "cs16_corrupted_reality_intro",
    "cs16_els_finale",
    "cs16_els_intro",
    "cs16_els_outro",
    "cs16_lost_souls_unite",
    "cs16_save_file_battle",

    -- Chapter 19
    "cs19_another_dimension_intro",
    "cs19_gravestone",
    "cs19_beyond_the_void",
    "cs19_chara_helps",
    "cs19_edge_of_universe",
    "cs19_hub_second_intro",
    "cs19_trapin_loop",

    -- Chapter 21
    "cs21_cast",
    "cs21_epilogue_credits",
    "cs21_fake_the_end",
    "cs21_final_cutscenes",
    "cs21_final_titan_summit",
    "cs21_special_thanks_dodge_credits",
    "cs21_two_worlds_unite",
    "cs21_saved",
    "cs21_farewell",
    "cs21_ending"
}

local eventTrigger = {}

eventTrigger.name = "DZ/DZEventTrigger"
eventTrigger.canResize = {true, true}
eventTrigger.placements = {
    {
        name = "main",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            event = "",
            onSpawn = false
        }
    }
}

eventTrigger.fieldInformation = {
    event = {
        fieldType = "string",
        editable = true,
        options = eventOptions
    },
    onSpawn = {
        fieldType = "boolean"
    }
}

eventTrigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "event",
    "onSpawn"
}

function eventTrigger.color(room, entity)
    if entity.event == nil or entity.event == "" then
        return {0.85, 0.35, 0.35, 0.8}
    end

    return {0.35, 0.8, 0.95, 0.8}
end

function eventTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        {
            texture = "ahorn/entityTrigger",
            x = entity.x,
            y = entity.y,
            scaleX = width / 8,
            scaleY = height / 8,
            color = eventTrigger.color(room, entity)
        }
    }
end

function eventTrigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        entity.x,
        entity.y,
        width,
        height
    }
end

return eventTrigger