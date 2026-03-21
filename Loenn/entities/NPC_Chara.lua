local npc_chara = {}

npc_chara.name = "DesoloZantas/NPC_Chara"
npc_chara.depth = 0
npc_chara.justification = {0.5, 1.0}
npc_chara.texture = "characters/chara/idle00"

npc_chara.placements = {
    {
        name = "NPC_Chara",
        data = {
            dialogKey = "CH2_CHARA_INTRO",
            flagName = "chara_met",
            enabledByDefault = true
        }
    },
    {
        name = "NPC_Chara_Second_Encounter",
        data = {
            dialogKey = "CH4_CHARA_2ND_INTRO",
            flagName = "chara_second_encounter",
            enabledByDefault = false
        }
    },
    {
        name = "NPC_Chara_Payphone",
        data = {
            dialogKey = "KIRBY_PAYPHONE_DREAM_END",
            flagName = "chara_payphone_encounter",
            enabledByDefault = false
        }
    }
}

npc_chara.fieldInformation = {
    dialogKey = {
        fieldType = "string",
        options = {
            "CH2_CHARA_INTRO",
            "CH2_CHARA_BREAKSOUT", 
            "CH4_CHARA_2ND_INTRO",
            "KIRBY_PAYPHONE_DREAM_END"
        }
    },
    flagName = {
        fieldType = "string"
    },
    enabledByDefault = {
        fieldType = "boolean"
    }
}

return npc_chara