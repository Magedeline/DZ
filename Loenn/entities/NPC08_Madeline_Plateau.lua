local npc08_madeline_plateau = {}

npc08_madeline_plateau.name = "MaggyHelper/NPC08_Madeline_Plateau"
npc08_madeline_plateau.depth = 0
npc08_madeline_plateau.justification = {0.5, 1.0}
npc08_madeline_plateau.texture = "characters/madeline/idle00"

npc08_madeline_plateau.placements = {
    {
        name = "NPC08_Madeline_Plateau",
        data = {
            dialogKey = "ingeste_madeline_08_plateau",
            flagName = "madeline_08_plateau",
            spriteId = "madeline"
        }
    }
}

npc08_madeline_plateau.fieldInformation = {
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
            "player",
            "badeline",
            "theo"
        }
    }
}

return npc08_madeline_plateau
