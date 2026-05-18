local npc03_maggy = {}

npc03_maggy.name = "MaggyHelper/NPC03_Maggy"
npc03_maggy.depth = 0
npc03_maggy.justification = {0.5, 1.0}
npc03_maggy.texture = "characters/Maggy/DesoloZantas/magolor/idle00"

npc03_maggy.placements = {
    {
        name = "NPC03_Maggy",
        data = {
            dialogKey = "ingeste_maggy_03_conversation",
            flagName = "maggy_03_talked",
            spriteId = "magolor"
        }
    }
}

npc03_maggy.fieldInformation = {
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
    }
}

return npc03_maggy
