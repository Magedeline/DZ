local npc16_theo = {}

npc16_theo.name = "MaggyHelper/NPC17_Theo"
npc16_theo.depth = 0
npc16_theo.justification = {0.5, 1.0}
npc16_theo.texture = "characters/Maggy/DesoloZantas/theo/theo00"

npc16_theo.placements = {
    {
        name = "NPC17_Theo",
        data = {
            dialogKey = "ingeste_theo_17_final",
            flagName = "theo_17_final",
            spriteId = "theo",
            talking = false
        }
    }
}

npc16_theo.fieldInformation = {
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
            "theo",
            "Kglobal::Player",
            "madeline",
            "kirby"
        }
    },
    talking = {
        fieldType = "boolean"
    }
}

return npc16_theo
