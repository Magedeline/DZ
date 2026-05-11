local npc3_theo = {}

npc3_theo.name = "MaggyHelper/NPC03_Theo"
npc3_theo.depth = 0
npc3_theo.justification = {0.5, 1.0}
npc3_theo.texture = "characters/Maggy/DesoloZantas/theo/theo00"

npc3_theo.placements = {
    {
        name = "NPC03_Theo",
        data = {
            dialogKey = "ingeste_theo_03_conversation",
            flagName = "theo_03_talked",
            spriteId = "theo"
        }
    }
}

npc3_theo.fieldInformation = {
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

return npc3_theo

