local npc02_maggy = {}

npc02_maggy.name = "MaggyHelper/NPC02_Maggy"
npc02_maggy.depth = 0
npc02_maggy.justification = {0.5, 1.0}
npc02_maggy.texture = "characters/Maggy/DesoloZantas/magolor/idle00"

npc02_maggy.placements = {
    {
        name = "NPC02_Maggy",
        data = {
            dialogKey = "ingeste_maggy_02_conversation",
            flagName = "maggy_02_talked",
            spriteId = "magolor"
        }
    }
}

npc02_maggy.fieldInformation = {
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

return npc02_maggy
