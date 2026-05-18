local npc20_asriel = {}

npc20_asriel.name = "MaggyHelper/NPC20_Asriel"
npc20_asriel.depth = 0
npc20_asriel.justification = {0.5, 1.0}
npc20_asriel.texture = "characters/Maggy/DesoloZantas/asriel/idle00"

npc20_asriel.placements = {
    {
        name = "NPC20_Asriel",
        data = {
            dialogKey = "ingeste_asriel_20_final",
            flagName = "asriel_20_final",
            spriteId = "asriel",
            talking = false
        }
    }
}

npc20_asriel.fieldInformation = {
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
            "asriel",
            "toriel",
            "theo",
            "Kglobal::Player",
            "madeline"
        }
    },
    talking = {
        fieldType = "boolean"
    }
}

return npc20_asriel
