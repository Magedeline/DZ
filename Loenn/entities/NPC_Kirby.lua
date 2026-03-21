local npc_kirby = {}

npc_kirby.name = "DesoloZantas/NPC_Kirby"
npc_kirby.depth = 0
npc_kirby.justification = {0.5, 1.0}
npc_kirby.texture = "characters/kirby/idle00"

npc_kirby.placements = {
    {
        name = "NPC_Kirby_Tutorial",
        data = {
            dialogKey = "EXAMPLE_ADVANCED_CUTSCENE",
            flagName = "kirby_tutorial_complete",
            enabledByDefault = true,
            canFloat = true
        }
    },
    {
        name = "NPC_Kirby_Dream_Lever",
        data = {
            dialogKey = "RALSEI_FREE",
            flagName = "kirby_dream_lever",
            enabledByDefault = true,
            canFloat = true
        }
    },
    {
        name = "NPC_Kirby_Payphone",
        data = {
            dialogKey = "KIRBY_PAYPHONE_AWAKE_END",
            flagName = "kirby_payphone",
            enabledByDefault = true,
            canFloat = false
        }
    }
}

npc_kirby.fieldInformation = {
    dialogKey = {
        fieldType = "string",
        options = {
            "EXAMPLE_ADVANCED_CUTSCENE",
            "EXAMPLE_CONCURRENT_ACTIONS",
            "EXAMPLE_CUSTOM_TRIGGERS",
            "KIRBY_PAYPHONE_DREAM_END",
            "KIRBY_PAYPHONE_AWAKE_END"
        }
    },
    flagName = {
        fieldType = "string"
    },
    enabledByDefault = {
        fieldType = "boolean"
    },
    canFloat = {
        fieldType = "boolean"
    }
}

return npc_kirby