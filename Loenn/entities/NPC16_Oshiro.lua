local npc16_oshiro = {}

npc16_oshiro.name = "MaggyHelper/NPC17_Oshiro"
npc16_oshiro.depth = 0
npc16_oshiro.justification = {0.5, 1.0}
npc16_oshiro.texture = "characters/Maggy/DesoloZantas/oshiro/oshiro00"

npc16_oshiro.placements = {
    {
        name = "NPC17_Oshiro",
        data = {
            dialogKey = "ingeste_oshiro_17_final",
            flagName = "oshiro_17_final",
            spriteId = "oshiro",
            talking = false
        }
    }
}

npc16_oshiro.fieldInformation = {
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
            "oshiro",
            "theo",
            "Kglobal::Player",
            "madeline"
        }
    },
    talking = {
        fieldType = "boolean"
    }
}

return npc16_oshiro

