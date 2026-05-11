local npc01_maggy = {}

npc01_maggy.name = "MaggyHelper/NPC01_Maggy"
npc01_maggy.depth = 0
npc01_maggy.justification = {0.5, 1.0}
npc01_maggy.texture = "characters/Maggy/DesoloZantas/magolor/idle00"

npc01_maggy.placements = {
    {
        name = "NPC01_Maggy",
        data = {
            dialogKey = "ingeste_maggy_01_conversation",
            flagName = "maggy_01_talked",
            spriteId = "magolor",
            currentConversation = 0
        }
    }
}

npc01_maggy.fieldInformation = {
    dialogKey = {
        fieldType = "string"
    },
    flagName = {
        fieldType = "string"
    },
    spriteId = {
        fieldType = "string",
        editable = true,
        options = {
            "magolor",
            "theo",
            "Kglobal::Player"
        }
    },
    currentConversation = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 10
    }
}

return npc01_maggy

