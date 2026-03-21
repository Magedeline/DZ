local InteractTrigger = {
    name = "MaggyHelper/IngesteInteractTrigger", -- Match the CustomEntity name in InteractTrigger.cs
    placements = {
        {
            name = "InteractTrigger",
            data = {
                x = 0,
                y = 0,
                width = 16, -- Updated to match interact_trigger.lua default
                height = 16, -- Updated to match interact_trigger.lua default
                prompt = "Press {button} to interact", -- Added from interact_trigger.lua
                onInteract = "", -- Added from interact_trigger.lua
                onlyOnce = false, -- Added from interact_trigger.lua
                event = "ch2_poem", -- Default event (can be customized in the editor)
                event_2 = "ch3_diary", -- Optional additional events
                event_3 = "ch3_guestbook", -- Add more as needed
            },
        },
    },
    fieldInformation = {
        prompt = {
            fieldType = "string"
        },
        onInteract = {
            fieldType = "string"
        },
        onlyOnce = {
            fieldType = "boolean"
        },
        event = {
            options = { 
                -- Chapter Events
                "ch2_poem", "ch3_diary", "ch3_guestbook", "ch3_memo", "ch5_mirror_reflection", "ch5_see_maddy", "ch5_see_maddy_b", "ch5_maddy_phone",
                -- NPC Events
                "npc_toriel", "npc_magolor", "npc_madntheo", "npc_madeline", "npc_08chara_crying",
                -- ComfCounter NPC Events
                "comf_npc_default_increment", "comf_npc_default_complete", "comf_npc_default_max",
                "comf_npc_small_increment", "comf_npc_small_complete", "comf_npc_small_max",
                "comf_npc_large_increment", "comf_npc_large_complete", "comf_npc_large_max"
            },
            editable = true
        },
        event_2 = {
            options = { 
                -- Chapter Events
                "ch2_poem", "ch3_diary", "ch3_guestbook", "ch3_memo", "ch5_mirror_reflection", "ch5_see_maddy", "ch5_see_maddy_b", "ch5_maddy_phone",
                -- NPC Events
                "npc_toriel", "npc_magolor", "npc_madntheo", "npc_madeline", "npc_08chara_crying",
                -- ComfCounter NPC Events
                "comf_npc_default_increment", "comf_npc_default_complete", "comf_npc_default_max",
                "comf_npc_small_increment", "comf_npc_small_complete", "comf_npc_small_max",
                "comf_npc_large_increment", "comf_npc_large_complete", "comf_npc_large_max"
            },
            editable = true
        },
        event_3 = {
            options = { 
                -- Chapter Events
                "ch2_poem", "ch3_diary", "ch3_guestbook", "ch3_memo", "ch5_mirror_reflection", "ch5_see_maddy", "ch5_see_maddy_b", "ch5_maddy_phone",
                -- NPC Events
                "npc_toriel", "npc_magolor", "npc_madntheo", "npc_madeline", "npc_08chara_crying",
                -- ComfCounter NPC Events
                "comf_npc_default_increment", "comf_npc_default_complete", "comf_npc_default_max",
                "comf_npc_small_increment", "comf_npc_small_complete", "comf_npc_small_max",
                "comf_npc_large_increment", "comf_npc_large_complete", "comf_npc_large_max"
            },
            editable = true
        },
    }
}

return InteractTrigger
