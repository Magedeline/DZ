-- SidMapEnterTrigger: place in 10_Ruins_Lobby.bin lobby_hub room.
-- Each portal area should have one of these, pointing to its target SID.
local sidMapEnterTrigger = {}

sidMapEnterTrigger.name = "MaggyHelper/SidMapEnterTrigger"
sidMapEnterTrigger.placements = {
    {
        name = "SM1 - Fragment I",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_SM1",
            targetRoom      = "lvl_sm1_start",
            requiredFlag    = "",
            lockedDialogKey = "RUINS_SM_LOCKED"
        }
    },
    {
        name = "SM2 - Fragment II",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_SM2",
            targetRoom      = "lvl_sm2_start",
            requiredFlag    = "",
            lockedDialogKey = "RUINS_SM_LOCKED"
        }
    },
    {
        name = "SM3 - Fragment III",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_SM3",
            targetRoom      = "lvl_sm3_start",
            requiredFlag    = "",
            lockedDialogKey = "RUINS_SM_LOCKED"
        }
    },
    {
        name = "SM4 - Fragment IV",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_SM4",
            targetRoom      = "lvl_sm4_start",
            requiredFlag    = "",
            lockedDialogKey = "RUINS_SM_LOCKED"
        }
    },
    {
        name = "SM5 - Fragment V",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_SM5",
            targetRoom      = "lvl_sm5_start",
            requiredFlag    = "",
            lockedDialogKey = "RUINS_SM_LOCKED"
        }
    },
    {
        name = "SM6 - Fragment VI",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_SM6",
            targetRoom      = "lvl_sm6_start",
            requiredFlag    = "",
            lockedDialogKey = "RUINS_SM_LOCKED"
        }
    },
    {
        name = "EX - Hidden Relic",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_EX",
            targetRoom      = "lvl_ex_start",
            requiredFlag    = "ruins_all_sm_complete",
            lockedDialogKey = "RUINS_EX_LOCKED"
        }
    },
    {
        name = "Boss - Ancient Warden",
        data = {
            width = 40, height = 80,
            targetSid       = "Maggy/SmallMaps/10_Ruins_Boss",
            targetRoom      = "lvl_boss_arena",
            requiredFlag    = "ruins_any_sm_complete",
            lockedDialogKey = "RUINS_BOSS_LOCKED"
        }
    },
    {
        name = "Generic (custom SID)",
        data = {
            width = 40, height = 80,
            targetSid       = "",
            targetRoom      = "",
            requiredFlag    = "",
            lockedDialogKey = "SUBMAP_LOCKED_DEFAULT"
        }
    }
}

sidMapEnterTrigger.fieldInformation = {
    targetSid       = { fieldType = "string" },
    targetRoom      = { fieldType = "string" },
    requiredFlag    = { fieldType = "string" },
    lockedDialogKey = { fieldType = "string" }
}

sidMapEnterTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "targetSid", "targetRoom",
    "requiredFlag", "lockedDialogKey"
}

sidMapEnterTrigger.color = { 0.4, 0.8, 1.0 }  -- light blue

return sidMapEnterTrigger
