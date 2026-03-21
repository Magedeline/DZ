local npc15x_madeline_and_badeline_ending = {}

npc15x_madeline_and_badeline_ending.name = "MaggyHelper/NPC15X_Madeline_and_Badeline_Ending"
npc15x_madeline_and_badeline_ending.depth = 0
npc15x_madeline_and_badeline_ending.justification = {0.5, 1.0}
npc15x_madeline_and_badeline_ending.texture = "characters/madeline/idle00"

npc15x_madeline_and_badeline_ending.placements = {
    {
        name = "NPC15X_Madeline_and_Badeline_Ending",
        data = {
            ch15EasterEgg = false
        }
    }
}

npc15x_madeline_and_badeline_ending.fieldInformation = {
    ch15EasterEgg = {
        fieldType = "boolean",
        editable = true
    }
}

return npc15x_madeline_and_badeline_ending
