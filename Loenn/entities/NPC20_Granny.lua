local npc20_granny = {}

npc20_granny.name = "MaggyHelper/NPC20_Granny"
npc20_granny.depth = 0
npc20_granny.justification = {0.5, 1.0}
npc20_granny.texture = "characters/Maggy/DesoloZantas/oldlady/idle00"

npc20_granny.placements = {
    {
        name = "NPC20_Granny",
        data = {
            dialogKey = "ingeste_granny_20_final",
            flagName = "granny_20_final",
            spriteId = "oldlady",
            talking = false
        }
    }
}

npc20_granny.fieldInformation = {
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
            "oldlady",
            "granny",
            "theo",
            "Kglobal::Player",
            "madeline"
        }
    },
    talking = {
        fieldType = "boolean"
    }
}

return npc20_granny
