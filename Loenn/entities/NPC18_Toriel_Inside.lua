local npc17_toriel_inside = {}

npc17_toriel_inside.name = "MaggyHelper/NPC18_Toriel_Inside"
npc17_toriel_inside.depth = 0
npc17_toriel_inside.justification = {0.5, 1.0}
npc17_toriel_inside.texture = "characters/Maggy/DesoloZantas/toriel/idle00"

npc17_toriel_inside.placements = {
    {
        name = "NPC18_Toriel_Inside",
        data = {
            dialogKey = "ingeste_toriel_18_inside",
            flagName = "toriel_18_inside",
            spriteId = "toriel"
        }
    }
}

npc17_toriel_inside.fieldInformation = {
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

return npc17_toriel_inside
