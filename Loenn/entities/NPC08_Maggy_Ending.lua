local npc08_maggy_ending = {}

npc08_maggy_ending.name = "MaggyHelper/NPC08_Maggy_Ending"
npc08_maggy_ending.depth = 0
npc08_maggy_ending.justification = {0.5, 1.0}
npc08_maggy_ending.texture = "characters/magolor/idle00"

npc08_maggy_ending.placements = {
    {
        name = "NPC08_Maggy_Ending",
        data = {
            dialogKey = "ingeste_maggy_08_ending",
            flagName = "maggy_08_ending",
            spriteId = "magolor"
        }
    }
}

npc08_maggy_ending.fieldInformation = {
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
            "player",
            "madeline"
        }
    }
}

return npc08_maggy_ending
