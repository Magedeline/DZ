local npc00_theo = {}

npc00_theo.name = "MaggyHelper/NPC00_Theo"
npc00_theo.depth = 0
npc00_theo.justification = {0.5, 1.0}
npc00_theo.texture = "characters/Maggy/DesoloZantas/theo/theo00"

npc00_theo.placements = {
    {
        name = "NPC00_Theo",
        data = {
            dialogKey = "ingeste_theo_00_house",
            flagName = "theo_00_house",
            spriteId = "theo"
        }
    }
}

npc00_theo.fieldInformation = {
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
            "theo",
            "Kglobal::Player",
            "madeline"
        }
    }
}

return npc00_theo

