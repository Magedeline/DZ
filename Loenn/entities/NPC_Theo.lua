local npc_theo = {}

npc_theo.name = "DesoloZantas/NPC_Theo"
npc_theo.depth = 0
npc_theo.justification = {0.5, 1.0}
npc_theo.texture = "characters/theo/idle00"

npc_theo.placements = {
    {
        name = "NPC_Theo_Prologue",
        data = {
            dialogKey = "CH0_THEO_A",
            flagName = "theo_prologue_met",
            enabledByDefault = true
        }
    },
    {
        name = "NPC_Theo_About_Magolor",
        data = {
            dialogKey = "CH0_THEO_0_B",
            flagName = "theo_magolor_info",
            enabledByDefault = true
        }
    },
    {
        name = "NPC_Theo_With_Magolor",
        data = {
            dialogKey = "CH4_MAGOLOR_AND_THEO",
            flagName = "theo_magolor_together",
            enabledByDefault = true
        }
    }
}

npc_theo.fieldInformation = {
    dialogKey = {
        fieldType = "string",
        options = {
            "CH0_THEO_A",
            "CH0_THEO_0_B", 
            "CH4_MAGOLOR_AND_THEO"
        }
    },
    flagName = {
        fieldType = "string"
    },
    enabledByDefault = {
        fieldType = "boolean"
    }
}

return npc_theo