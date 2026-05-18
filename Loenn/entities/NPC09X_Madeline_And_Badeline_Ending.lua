local npc09x_madeline_and_badeline_ending = {}

npc09x_madeline_and_badeline_ending.name = "MaggyHelper/NPC09X_Madeline_and_Badeline_Ending"
npc09x_madeline_and_badeline_ending.depth = 0
npc09x_madeline_and_badeline_ending.justification = {0.5, 1.0}
npc09x_madeline_and_badeline_ending.texture = "characters/Maggy/DesoloZantas/madeline/idle00"

npc09x_madeline_and_badeline_ending.placements = {
    {
        name = "NPC09X_Madeline_and_Badeline_Ending",
        data = {
            ch15EasterEgg = false
        }
    }
}

npc09x_madeline_and_badeline_ending.fieldInformation = {
    ch15EasterEgg = {
        fieldType = "boolean",
        editable = true
    }
}

return npc09x_madeline_and_badeline_ending
