local eventTrigger = {}

eventTrigger.name = "DZ/EventTrigger"
eventTrigger.depth = 0
eventTrigger.placements = {
    name = "event_trigger",
    data = {
        event = "ch9_ending",
        onSpawn = false,
        onlyOnce = true
    }
}

-- Field options for the event property
eventTrigger.fieldInformation = {
    event = {
        fieldType = "string",
        options = {
            { displayName = "End City", value = "end_city" },
            { displayName = "End Old Site (Dream)", value = "end_oldsite_dream" },
            { displayName = "End Old Site (Awake)", value = "end_oldsite_awake" },
            { displayName = "Chapter 5 - See Theo", value = "ch5_see_theo" },
            { displayName = "Chapter 5 - Found Theo", value = "ch5_found_theo" },
            { displayName = "Chapter 5 - Mirror Reflection", value = "ch5_mirror_reflection" },
            { displayName = "Chapter 5 - Cancel See Theo", value = "cancel_ch5_see_theo" },
            { displayName = "Chapter 6 - Boss Intro", value = "ch6_boss_intro" },
            { displayName = "Chapter 6 - Reflect", value = "ch6_reflect" },
            { displayName = "Chapter 7 - Summit", value = "ch7_summit" },
            { displayName = "Chapter 8 - Door", value = "ch8_door" },
            { displayName = "Chapter 9 - Go to Future", value = "ch9_goto_the_future" },
            { displayName = "Chapter 9 - Go to Past", value = "ch9_goto_the_past" },
            { displayName = "Chapter 9 - Moon Intro", value = "ch9_moon_intro" },
            { displayName = "Chapter 9 - Hub Intro", value = "ch9_hub_intro" },
            { displayName = "Chapter 9 - Hub Transition Out", value = "ch9_hub_transition_out" },
            { displayName = "Chapter 9 - Badeline Helps", value = "ch9_badeline_helps" },
            { displayName = "Chapter 9 - Farewell", value = "ch9_farewell" },
            { displayName = "Chapter 9 - Ending", value = "ch9_ending" },
            { displayName = "Chapter 9 - End Golden", value = "ch9_end_golden" },
            { displayName = "Chapter 9 - Final Room", value = "ch9_final_room" },
            { displayName = "Chapter 9 - Ding Ding Ding", value = "ch9_ding_ding_ding" },
            { displayName = "Chapter 9 - Golden Snapshot", value = "ch9_golden_snapshot" },
            { displayName = "Custom Event 1 - First Step", value = "cs03_first_step" },
            { displayName = "Custom Event 2 - Meetup", value = "cs03_meetup" },
            { displayName = "Custom Event 3 - Mod Ending", value = "cs03_mod_ending" },
        }
    }
}

return eventTrigger
