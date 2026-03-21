local npc16_kirby = {}

npc16_kirby.name = "MaggyHelper/NPC17_Kirby"
npc16_kirby.depth = 0
npc16_kirby.justification = {0.5, 1.0}
npc16_kirby.texture = "characters/kirby/idle00"

npc16_kirby.placements = {
    {
        name = "NPC17_Kirby",
        data = {
            dialogKey = "ingeste_kirby_17_ending",
            flagName = "kirby_17_ending",
            spriteId = "kirby",
            talking = false
        }
    }
}

npc16_kirby.fieldInformation = {
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
            "kirby",
            "theo",
            "player",
            "madeline"
        }
    },
    talking = {
        fieldType = "boolean"
    }
}

return npc16_kirby
