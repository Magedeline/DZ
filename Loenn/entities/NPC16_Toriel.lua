local npc16_toriel = {}

npc16_toriel.name = "MaggyHelper/NPC17_Toriel"
npc16_toriel.depth = 0
npc16_toriel.justification = {0.5, 1.0}
npc16_toriel.texture = "characters/Maggy/DesoloZantas/toriel/idle00"

npc16_toriel.placements = {
    {
        name = "default",
        data = {
            dialogKey = "ingeste_toriel_17_final",
            flagName = "toriel_17_final",
            spriteId = "toriel",
            talking = false
        }
    }
}

npc16_toriel.fieldInformation = {
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
            "toriel",
            "theo",
            "Kglobal::Player",
            "madeline",
            "asriel"
        }
    },
    talking = {
        fieldType = "boolean"
    }
}

return npc16_toriel
