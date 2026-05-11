local npc20_madeline = {}

npc20_madeline.name = "MaggyHelper/NPC20_Madeline"
npc20_madeline.depth = 0
npc20_madeline.justification = {0.5, 1.0}
npc20_madeline.texture = "characters/Maggy/DesoloZantas/madeline/idle00"

npc20_madeline.placements = {
    {
        name = "NPC20_Madeline",
        data = {
            dialogKey = "ingeste_madeline_20_saved",
            flagName = "madeline_20_saved",
            spriteId = "madeline",
            talking = false
        }
    }
}

npc20_madeline.fieldInformation = {
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
            "madeline",
            "Kglobal::Player",
            "badeline",
            "theo"
        }
    },
    talking = {
        fieldType = "boolean"
    }
}

return npc20_madeline

