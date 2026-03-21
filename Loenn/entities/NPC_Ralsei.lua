local npc_ralsei = {}

npc_ralsei.name = "DesoloZantas/NPC_Ralsei"
npc_ralsei.depth = 0
npc_ralsei.justification = {0.5, 1.0}
npc_ralsei.texture = "characters/ralsei/idle00"

npc_ralsei.placements = {
    {
        name = "NPC_Ralsei_Trapped",
        data = {
            dialogKey = "RALSEI_FREE",
            flagName = "ralsei_freed",
            enabledByDefault = false
        }
    },
    {
        name = "NPC_Ralsei_Legend_Teller",
        data = {
            dialogKey = "CH4_LEGEND_A",
            flagName = "ralsei_legend_told",
            enabledByDefault = true
        }
    },
    {
        name = "NPC_Ralsei_Helper",
        data = {
            dialogKey = "CH4_CHARA_2ND_INTRO",
            flagName = "ralsei_helper",
            enabledByDefault = true
        }
    }
}

npc_ralsei.fieldInformation = {
    dialogKey = {
        fieldType = "string",
        options = {
            "RALSEI_FREE",
            "CH4_LEGEND_A",
            "CH4_LEGENDOUTRO",
            "CH4_CHARA_2ND_INTRO"
        }
    },
    flagName = {
        fieldType = "string"
    },
    enabledByDefault = {
        fieldType = "boolean"
    }
}

return npc_ralsei