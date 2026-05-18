local npc17_toriel_outside = {}

npc17_toriel_outside.name = "MaggyHelper/NPC18_Toriel_Outside"
npc17_toriel_outside.depth = 0
npc17_toriel_outside.justification = {0.5, 1.0}
npc17_toriel_outside.texture = "characters/Maggy/DesoloZantas/toriel/idle00"

npc17_toriel_outside.placements = {
    {
        name = "NPC18_Toriel_Outside",
        data = {
            dialogKey = "ingeste_toriel_18_outside",
            flagName = "toriel_18_outside",
            spriteId = "toriel"
        }
    }
}

npc17_toriel_outside.fieldInformation = {
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
    }
}

return npc17_toriel_outside
