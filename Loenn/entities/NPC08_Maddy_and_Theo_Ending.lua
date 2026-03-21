local npc08_maddy_and_theo_ending = {}

npc08_maddy_and_theo_ending.name = "MaggyHelper/NPC08_Maddy_and_Theo_Ending"
npc08_maddy_and_theo_ending.depth = 0
npc08_maddy_and_theo_ending.justification = {0.5, 1.0}
npc08_maddy_and_theo_ending.texture = "characters/madeline/idle00"

npc08_maddy_and_theo_ending.placements = {
    {
        name = "NPC08_Maddy_and_Theo_Ending",
        data = {
            dialogKey = "ingeste_maddy_theo_08_ending",
            flagName = "maddy_theo_08_ending",
            spriteId = "madeline"
        }
    }
}

npc08_maddy_and_theo_ending.fieldInformation = {
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
            "theo",
            "player",
            "badeline"
        }
    }
}

return npc08_maddy_and_theo_ending
