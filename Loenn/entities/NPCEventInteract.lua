local npc_event_interact = {}

npc_event_interact.name = "MaggyHelper/NPCEventInteract"
npc_event_interact.depth = 0
npc_event_interact.texture = "characters/player/idle00"

-- Reference to C# NPCs in Source/Entities/ folder
-- NPCs 00-21 integrated with cs flag support
-- NPC00_Kirby_Start, NPC01_Magolor_Intro, NPC02_Chara_First, NPC03_Maggy, NPC04_Ralsei_Mirror,
-- NPC05_Oshiro_Breakdown, NPC06_Gaster_Lab, NPC07_Maddy_Mirror, NPC08_Maggy_Ending, NPC08_Madeline_Plateau,
-- NPC08_Maddy_and_Theo_Ending, NPC08_Chara_Crying, NPC09_Sans_Phone, NPC10_Flowey_Ruins,
-- NPC11_Papyrus_Voice, NPC12_Undyne_Battle, NPC13_Asgore_Core, NPC14_Alphys_Lab, NPC15_Mettaton_Show,
-- NPC16_Toriel, NPC16_Theo, NPC16_Oshiro, NPC16_Kirby, NPC17_Toriel_Outside, NPC17_Toriel_Inside,
-- NPC18_Secrets_Gathering, NPC19_Maggy_Loop, NPC19_Gravestone, NPC20_Madeline, NPC20_Granny, NPC20_Asriel,
-- NPC21_Final_Boss, NPC_ZFinal1_Boss, NPCEvent

-- Available Character Sprite Folders:
-- Graphics/Atlases/Gameplay/characters/:
-- asriel, badeline, bird, boss, chara, charaBoss, finalboss, flowey, Kirby, Kirby_No_Backpack,
-- Kirby_Playback, Kirby_with_weapon, MaddyCrystal, madeline, Magolor, monsters, npcs, oldlady,
-- oshiro, Phase1Boss, player, player_badeline, player_no_backpack, player_playback, ralsei,
-- ralsei_gl, ralsei_playback, savepoint, space_monster, theo, theo_carry_maddy, toriel,
-- vessel_boss, vessel_you, ZeroBoss, zfinal_boss

npc_event_interact.fieldInformation = {
    npcType = {
        fieldType = "string",
        editable = true,
        options = {
            "NPC00_Kirby_Start",
            "NPC01_Magolor_Intro", 
            "NPC02_Chara_First",
            "NPC03_Maggy",
            "NPC04_Ralsei_Mirror",
            "NPC05_Oshiro_Breakdown",
            "NPC06_Gaster_Lab",
            "NPC07_Maddy_Mirror",
            "NPC08_Maggy_Ending",
            "NPC08_Madeline_Plateau",
            "NPC08_Maddy_and_Theo_Ending",
            "NPC08_Chara_Crying",
            "NPC09_Sans_Phone",
            "NPC09_SavePoint",
            "NPC09_SavePoint_A",
            "NPC09_SavePoint_B",
            "NPC09_SavePoint_C",
            "NPC09_SavePoint_D",
            "NPC09_SavePoint_Trap",
            "NPC10_Flowey_Ruins",
            "NPC11_Papyrus_Voice",
            "NPC12_Undyne_Battle",
            "NPC13_Asgore_Core",
            "NPC14_Alphys_Lab",
            "NPC15_Mettaton_Show",
            "NPC16_Toriel",
            "NPC16_Theo",
            "NPC16_Oshiro", 
            "NPC16_Kirby",
            "NPC17_Toriel_Outside",
            "NPC17_Toriel_Inside",
            "NPC18_Secrets_Gathering",
            "NPC19_Maggy_Loop",
            "NPC19_Gravestone",
            "NPC20_Madeline",
            "NPC20_Granny",
            "NPC20_Asriel",
            "NPC21_Final_Boss"
        }
    },
    spriteId = {
        fieldType = "string",
        editable = true,
        options = {
            "theo",
            "chara",
            "kirby", 
            "ralsei",
            "madeline",
            "toriel",
            "asriel",
            "maggy",
            "oshiro",
            "granny",
            "badeline",
            "flowey",
            "gaster",
            "papyrus",
            "sans",
            "undyne",
            "asgore",
            "alphys",
            "mettaton",
            "ghost",
            "mom",
            "bird",
            "maddydead",
            "monsters"
        }
    },
    hasLight = {
        fieldType = "boolean",
        editable = true
    },
    npcName = {
        fieldType = "string",
        editable = true
    },
    enableSpriteSwap = {
        fieldType = "boolean",
        editable = true
    },
    alternateSprite = {
        fieldType = "string",
        editable = true,
        options = {
            "theo",
            "chara", 
            "kirby",
            "ralsei",
            "madeline",
            "toriel",
            "asriel",
            "maggy",
            "oshiro",
            "granny",
            "badeline",
            "flowey",
            "gaster",
            "papyrus",
            "sans",
            "undyne",
            "asgore",
            "alphys",
            "mettaton",
            "ghost",
            "mom",
            "bird",
            "maddydead",
            "monsters"
        }
    },
    csFlag = {
        fieldType = "string",
        editable = true
    },
    dialogKey = {
        fieldType = "string",
        editable = true
    },
    triggerFlag = {
        fieldType = "string", 
        editable = true
    },
    requireFlag = {
        fieldType = "string",
        editable = true
    }
}

npc_event_interact.placements = {
    -- NPCs 00-05 (Early Game)
    {
        name = "NPC00 - Kirby Start",
        data = {
            npcType = "NPC00_Kirby_Start",
            spriteId = "kirby",
            hasLight = true,
            npcName = "Kirby",
            enableSpriteSwap = false,
            alternateSprite = "kirby",
            csFlag = "npc00_kirby_met",
            dialogKey = "CH1_KIRBY_INTRO",
            triggerFlag = "met_kirby",
            requireFlag = ""
        }
    },
    {
        name = "NPC01 - Magolor Intro", 
        data = {
            npcType = "NPC01_Magolor_Intro",
            spriteId = "maggy",
            hasLight = true,
            npcName = "Magolor",
            enableSpriteSwap = false,
            alternateSprite = "maggy",
            csFlag = "npc01_magolor_met",
            dialogKey = "CH1_MAGGY_A",
            triggerFlag = "met_magolor",
            requireFlag = ""
        }
    },
    {
        name = "NPC02 - Chara First",
        data = {
            npcType = "NPC02_Chara_First", 
            spriteId = "chara",
            hasLight = true,
            npcName = "Chara",
            enableSpriteSwap = true,
            alternateSprite = "badeline",
            csFlag = "npc02_chara_met",
            dialogKey = "CH2_CHARA_INTRO",
            triggerFlag = "met_chara",
            requireFlag = ""
        }
    },
    {
        name = "NPC03 - Maggy",
        data = {
            npcType = "NPC03_Maggy",
            spriteId = "maggy",
            hasLight = true,
            npcName = "Magolor",
            enableSpriteSwap = false,
            alternateSprite = "maggy", 
            csFlag = "npc03_maggy_talked",
            dialogKey = "CH3_MAGGY_TALK",
            triggerFlag = "maggy_conversation",
            requireFlag = "WassupMagolor"
        }
    },
    {
        name = "NPC04 - Ralsei Mirror",
        data = {
            npcType = "NPC04_Ralsei_Mirror",
            spriteId = "ralsei",
            hasLight = true,
            npcName = "Ralsei",
            enableSpriteSwap = false,
            alternateSprite = "ralsei",
            csFlag = "npc04_ralsei_met",
            dialogKey = "RALSEI_INTRO",
            triggerFlag = "met_ralsei",
            requireFlag = ""
        }
    },
    {
        name = "NPC05 - Oshiro Breakdown",
        data = {
            npcType = "NPC05_Oshiro_Breakdown",
            spriteId = "oshiro",
            hasLight = true,
            npcName = "Mr. Oshiro",
            enableSpriteSwap = false,
            alternateSprite = "oshiro",
            csFlag = "npc05_oshiro_breakdown",
            dialogKey = "CH5_OSHIRO_BREAKDOWN",
            triggerFlag = "oshiro_breakdown", 
            requireFlag = ""
        }
    },
    
    -- NPCs 06-10 (Mid Game)
    {
        name = "NPC06 - Gaster Lab",
        data = {
            npcType = "NPC06_Gaster_Lab",
            spriteId = "gaster",
            hasLight = true,
            npcName = "Dr. Gaster",
            enableSpriteSwap = false,
            alternateSprite = "gaster",
            csFlag = "npc06_gaster_lab",
            dialogKey = "CH6_GASTERVOICE",
            triggerFlag = "met_gaster",
            requireFlag = ""
        }
    },
    {
        name = "NPC07 - Maddy Mirror",
        data = {
            npcType = "NPC07_Maddy_Mirror",
            spriteId = "madeline",
            hasLight = true,
            npcName = "Madeline",
            enableSpriteSwap = true,
            alternateSprite = "badeline",
            csFlag = "npc07_maddy_mirror",
            dialogKey = "CH7_MADELINE_MIRROR",
            triggerFlag = "madeline_mirror",
            requireFlag = ""
        }
    },
    {
        name = "NPC08 - Maggy Ending",
        data = {
            npcType = "NPC08_Maggy_Ending",
            spriteId = "maggy",
            hasLight = true,
            npcName = "Magolor",
            enableSpriteSwap = false,
            alternateSprite = "maggy",
            csFlag = "npc08_maggy_ending",
            dialogKey = "CH8_MAGGY_ENDING",
            triggerFlag = "maggy_ending",
            requireFlag = ""
        }
    },
    {
        name = "NPC08 - Madeline Plateau",
        data = {
            npcType = "NPC08_Madeline_Plateau",
            spriteId = "madeline",
            hasLight = true,
            npcName = "Madeline",
            enableSpriteSwap = false,
            alternateSprite = "badeline",
            csFlag = "npc08_madeline_plateau", 
            dialogKey = "CH8_MADELINE_PLATEAU",
            triggerFlag = "madeline_plateau",
            requireFlag = ""
        }
    },
    {
        name = "NPC08 - Maddy and Theo Ending",
        data = {
            npcType = "NPC08_Maddy_and_Theo_Ending",
            spriteId = "madeline",
            hasLight = true,
            npcName = "Madeline & Theo",
            enableSpriteSwap = true,
            alternateSprite = "theo",
            csFlag = "npc08_maddy_theo_ending",
            dialogKey = "",
            triggerFlag = "ch8_endingmod",
            requireFlag = ""
        }
    },
        {
        name = "NPC08 - Maggy Ending",
        data = {
            npcType = "NPC08_Magolor_Ending",
            spriteId = "maggy",
            hasLight = true,
            npcName = "Magolor",
            enableSpriteSwap = true,
            alternateSprite = "maggy",
            csFlag = "npc08_magolor_ending",
            dialogKey = "",
            triggerFlag = "ch8_endingmod",
            requireFlag = ""
        }
    },
    {
        name = "NPC08 - Chara Crying",
        data = {
            npcType = "NPC08_Chara_Crying",
            spriteId = "chara",
            hasLight = true,
            npcName = "Chara",
            enableSpriteSwap = false,
            alternateSprite = "chara",
            csFlag = "npc08_chara_crying",
            dialogKey = "CH8_CHARA_CRYING",
            triggerFlag = "chara_crying",
            requireFlag = ""
        }
    },
    {
        name = "NPC09 - Sans Phone",
        data = {
            npcType = "NPC09_Sans_Phone",
            spriteId = "sans",
            hasLight = true,
            npcName = "Sans",
            enableSpriteSwap = false,
            alternateSprite = "sans",
            csFlag = "npc09_sans_phone",
            dialogKey = "CH9_TORIEL_AND_SANS_VOICEPHONECALLMESSAGE",
            triggerFlag = "sans_phone",
            requireFlag = ""
        }
    },
    {
        name = "NPC09 - Fake Save Point",
        data = {
            npcType = "NPC09_SavePoint",
            spriteId = "savepoint",
            hasLight = true,
            npcName = "FakeSavePoint",
            enableSpriteSwap = false,
            alternateSprite = "savepoint",
            csFlag = "ch9_fakesave_stage_a",
            dialogKey = "CH9_SPACE_SAVE_FILEA",
            triggerFlag = "fakesavepoint_interact",
            requireFlag = ""
        }
    },
    {
        name = "NPC09 - Fake Save Point (Stage A)",
        data = {
            npcType = "NPC09_SavePoint_A",
            spriteId = "savepoint",
            hasLight = true,
            npcName = "FakeSavePoint_A",
            enableSpriteSwap = false,
            alternateSprite = "savepoint",
            csFlag = "ch9_fakesave_stage_a",
            dialogKey = "CH9_SPACE_SAVE_FILEA",
            triggerFlag = "fakesavepoint_stage_a",
            requireFlag = ""
        }
    },
    {
        name = "NPC09 - Fake Save Point (Stage B)",
        data = {
            npcType = "NPC09_SavePoint_B",
            spriteId = "savepoint",
            hasLight = true,
            npcName = "FakeSavePoint_B",
            enableSpriteSwap = false,
            alternateSprite = "savepoint",
            csFlag = "ch9_fakesave_stage_b",
            dialogKey = "CH9_SPACE_SAVE_FILEB",
            triggerFlag = "fakesavepoint_stage_b",
            requireFlag = "ch9_fakesave_stage_a"
        }
    },
    {
        name = "NPC09 - Fake Save Point (Stage C)",
        data = {
            npcType = "NPC09_SavePoint_C",
            spriteId = "savepoint",
            hasLight = true,
            npcName = "FakeSavePoint_C",
            enableSpriteSwap = false,
            alternateSprite = "savepoint",
            csFlag = "ch9_fakesave_stage_c",
            dialogKey = "CH9_SPACE_SAVE_FILEC",
            triggerFlag = "fakesavepoint_stage_c",
            requireFlag = "ch9_fakesave_stage_b"
        }
    },
    {
        name = "NPC09 - Fake Save Point (Stage D)",
        data = {
            npcType = "NPC09_SavePoint_D",
            spriteId = "savepoint",
            hasLight = true,
            npcName = "FakeSavePoint_D",
            enableSpriteSwap = false,
            alternateSprite = "savepoint",
            csFlag = "ch9_fakesave_stage_d",
            dialogKey = "CH9_SPACE_SAVE_FILED",
            triggerFlag = "fakesavepoint_stage_d",
            requireFlag = "ch9_fakesave_stage_c"
        }
    },
    {
        name = "NPC09 - Fake Save Point (Trap)",
        data = {
            npcType = "NPC09_SavePoint_Trap",
            spriteId = "savepoint",
            hasLight = true,
            npcName = "FakeSavePoint_Trap",
            enableSpriteSwap = false,
            alternateSprite = "savepoint",
            csFlag = "ch9_fakesave_trap",
            dialogKey = "CH9_SPACE_TRAP_SAVE_FILE",
            triggerFlag = "fakesavepoint_trap",
            requireFlag = "ch9_fakesave_stage_d"
        }
    },
    {
        name = "NPC10 - Flowey Ruins",
        data = {
            npcType = "NPC10_Flowey_Ruins",
            spriteId = "flowey",
            hasLight = true,
            npcName = "Flowey",
            enableSpriteSwap = false,
            alternateSprite = "flowey",
            csFlag = "npc10_flowey_ruins",
            dialogKey = "CH10_FLOWEY_INTRO",
            triggerFlag = "met_flowey",
            requireFlag = ""
        }
    },
    
    -- NPCs 11-15 (Late Game)
    {
        name = "NPC11 - Papyrus Voice",
        data = {
            npcType = "NPC11_Papyrus_Voice",
            spriteId = "papyrus",
            hasLight = true,
            npcName = "Papyrus",
            enableSpriteSwap = false,
            alternateSprite = "papyrus",
            csFlag = "npc11_papyrus_voice",
            dialogKey = "CH11_PAPYRUS_VOICE",
            triggerFlag = "met_papyrus",
            requireFlag = ""
        }
    },
    {
        name = "NPC12 - Undyne Battle",
        data = {
            npcType = "NPC12_Undyne_Battle",
            spriteId = "undyne",
            hasLight = true,
            npcName = "Undyne",
            enableSpriteSwap = false,
            alternateSprite = "undyne",
            csFlag = "npc12_undyne_battle",
            dialogKey = "CH12_UNDYNE_BATTLE",
            triggerFlag = "met_undyne",
            requireFlag = ""
        }
    },
    {
        name = "NPC13 - Asgore Core",
        data = {
            npcType = "NPC13_Asgore_Core",
            spriteId = "asgore",
            hasLight = true,
            npcName = "Asgore",
            enableSpriteSwap = false,
            alternateSprite = "asgore",
            csFlag = "npc13_asgore_core",
            dialogKey = "CH13_ASGORE_CORE",
            triggerFlag = "met_asgore",
            requireFlag = ""
        }
    },
    {
        name = "NPC14 - Alphys Lab",
        data = {
            npcType = "NPC14_Alphys_Lab",
            spriteId = "alphys",
            hasLight = true,
            npcName = "Dr. Alphys",
            enableSpriteSwap = false,
            alternateSprite = "alphys",
            csFlag = "npc14_alphys_lab",
            dialogKey = "CH13_EXTRA_NPC_ALPHYS_LAB",
            triggerFlag = "met_alphys",
            requireFlag = ""
        }
    },
    {
        name = "NPC15 - Mettaton Show",
        data = {
            npcType = "NPC15_Mettaton_Show",
            spriteId = "mettaton",
            hasLight = true,
            npcName = "Mettaton",
            enableSpriteSwap = false,
            alternateSprite = "mettaton",
            csFlag = "npc15_mettaton_show",
            dialogKey = "CH15_METTATON_SHOW",
            triggerFlag = "met_mettaton",
            requireFlag = ""
        }
    },
    
    -- NPCs 16-21 (End Game)
    {
        name = "NPC16 - Toriel",
        data = {
            npcType = "NPC16_Toriel",
            spriteId = "toriel",
            hasLight = true,
            npcName = "Toriel",
            enableSpriteSwap = false,
            alternateSprite = "toriel",
            csFlag = "npc16_toriel",
            dialogKey = "CH16_TORIEL_A",
            triggerFlag = "met_toriel",
            requireFlag = ""
        }
    },
    {
        name = "NPC16 - Theo",
        data = {
            npcType = "NPC16_Theo",
            spriteId = "theo",
            hasLight = true,
            npcName = "Theo",
            enableSpriteSwap = false,
            alternateSprite = "theo",
            csFlag = "npc16_theo",
            dialogKey = "CH16_THEO_CORE",
            triggerFlag = "theo_core",
            requireFlag = ""
        }
    },
    {
        name = "NPC16 - Oshiro",
        data = {
            npcType = "NPC16_Oshiro",
            spriteId = "oshiro",
            hasLight = true,
            npcName = "Mr. Oshiro",
            enableSpriteSwap = false,
            alternateSprite = "oshiro",
            csFlag = "npc16_oshiro",
            dialogKey = "CH16_OSHIRO_CORE",
            triggerFlag = "oshiro_core",
            requireFlag = ""
        }
    },
    {
        name = "NPC16 - Kirby",
        data = {
            npcType = "NPC16_Kirby",
            spriteId = "kirby",
            hasLight = true,
            npcName = "Kirby",
            enableSpriteSwap = false,
            alternateSprite = "kirby",
            csFlag = "npc16_kirby",
            dialogKey = "CH16_KIRBY_CORE",
            triggerFlag = "kirby_core",
            requireFlag = ""
        }
    },
    {
        name = "NPC17 - Toriel Outside",
        data = {
            npcType = "NPC17_Toriel_Outside",
            spriteId = "toriel",
            hasLight = true,
            npcName = "Toriel",
            enableSpriteSwap = false,
            alternateSprite = "toriel",
            csFlag = "npc17_toriel_outside",
            dialogKey = "CH17_TORIEL_OUTSIDE",
            triggerFlag = "toriel_outside",
            requireFlag = ""
        }
    },
    {
        name = "NPC17 - Toriel Inside",
        data = {
            npcType = "NPC17_Toriel_Inside",
            spriteId = "toriel",
            hasLight = true,
            npcName = "Toriel",
            enableSpriteSwap = false,
            alternateSprite = "toriel",
            csFlag = "npc17_toriel_inside",
            dialogKey = "ingeste_toriel_17_inside",
            triggerFlag = "toriel_inside",
            requireFlag = ""
        }
    },
    {
        name = "NPC19 - Maggy Loop",
        data = {
            npcType = "NPC19_Maggy_Loop",
            spriteId = "maggy",
            hasLight = true,
            npcName = "Magolor",
            enableSpriteSwap = false,
            alternateSprite = "maggy",
            csFlag = "npc19_maggy_loop",
            dialogKey = "CH19_MAGGY_LOOP",
            triggerFlag = "maggy_loop",
            requireFlag = ""
        }
    },
    {
        name = "NPC19 - Gravestone",
        data = {
            npcType = "NPC19_Gravestone",
            spriteId = "maddydead",
            hasLight = false,
            npcName = "gravestone",
            enableSpriteSwap = false,
            alternateSprite = "maddydead",
            csFlag = "npc19_gravestone",
            dialogKey = "CH19_GRAVESTONE",
            triggerFlag = "",
            requireFlag = "maddy_gravestone"
        }
    },
    {
        name = "NPC20 - Madeline",
        data = {
            npcType = "NPC20_Madeline",
            spriteId = "madeline",
            hasLight = true,
            npcName = "Madeline",
            enableSpriteSwap = true,
            alternateSprite = "badeline",
            csFlag = "npc20_madeline",
            dialogKey = "CH20_MADELINE_FINAL",
            triggerFlag = "madeline_final",
            requireFlag = ""
        }
    },
    {
        name = "NPC20 - Granny",
        data = {
            npcType = "NPC20_Granny",
            spriteId = "granny",
            hasLight = true,
            npcName = "Granny",
            enableSpriteSwap = false,
            alternateSprite = "granny",
            csFlag = "npc20_granny",
            dialogKey = "CH20_MADANDBADFINALLYMEETGRANNY",
            triggerFlag = "met_granny_final",
            requireFlag = ""
        }
    },
    {
        name = "NPC20 - Asriel",
        data = {
            npcType = "NPC20_Asriel",
            spriteId = "asriel",
            hasLight = true,
            npcName = "Asriel",
            enableSpriteSwap = false,
            alternateSprite = "asriel",
            csFlag = "npc20_asriel",
            dialogKey = "ingeste_asriel_20_final",
            triggerFlag = "asriel_final",
            requireFlag = ""
        }
    },
    {
        name = "NPC21 - Final Boss",
        data = {
            npcType = "NPC21_Final_Boss",
            spriteId = "boss",
            hasLight = true,
            npcName = "Final Boss",
            enableSpriteSwap = true,
            alternateSprite = "finalboss",
            csFlag = "npc21_final_boss",
            dialogKey = "CH21_FINAL_BOSS",
            triggerFlag = "final_boss_encountered",
            requireFlag = ""
        }
    }
}

-- Function to handle sprite swapping based on conditions
function npc_event_interact.sprite(room, entity)
    local spriteId = entity.spriteId or "theo"
    local enableSpriteSwap = entity.enableSpriteSwap or false
    local alternateSprite = entity.alternateSprite or "chara"
    
    -- You could add logic here to determine when to use alternate sprite
    -- For now, it uses the primary spriteId unless sprite swapping is enabled
    local currentSprite = spriteId
    if enableSpriteSwap then
        -- Add your sprite swap logic here
        -- For example, swap based on game state, player position, etc.
        currentSprite = alternateSprite
    end
    
    -- Map sprite IDs to texture paths (expanded for all NPC types)
    local spriteTextures = {
        theo = "characters/theo/idle00",
        chara = "characters/chara/idle00", 
        kirby = "characters/kirby/idle00",
        ralsei = "characters/ralsei/idle00",
        madeline = "characters/player/idle00",
        toriel = "characters/toriel/idle00",
        asriel = "characters/asriel/idle00",
        maggy = "characters/magolor/idle00",
        oshiro = "characters/oshiro/idle00",
        granny = "characters/oldlady/idle00",
        badeline = "characters/badeline/idle00",
        flowey = "characters/flowey/idle00",
        gaster = "characters/gaster/idle00",
        papyrus = "characters/papyrus/idle00",
        sans = "characters/sans/idle00",
        undyne = "characters/undyne/idle00",
        asgore = "characters/asgore/idle00",
        alphys = "characters/alphys/idle00",
        mettaton = "characters/mettaton/idle00",
        ghost = "characters/ghost/idle00",
        mom = "characters/mom/idle00",
        bird = "characters/bird/idle00",
        monsters = "characters/monsters/idle00",
        boss = "characters/boss/idle00",
        maddydead = "characters/gravestones/maddygrave00",
        finalboss = "characters/finalboss/idle00"
    }
    
    return spriteTextures[currentSprite] or spriteTextures["theo"]
end

-- Function to handle cs flag logic
function npc_event_interact.getCSFlag(entity)
    return entity.csFlag or "default_flag"
end

-- Function to get dialog key based on NPC type
function npc_event_interact.getDialogKey(entity)
    return entity.dialogKey or "DEFAULT_DIALOG"
end

-- Function to check if NPC should be visible based on flags
function npc_event_interact.shouldShow(entity, session)
    if not entity.requireFlag or entity.requireFlag == "" then
        return true
    end
    return session and session:GetFlag(entity.requireFlag)
end

return npc_event_interact
