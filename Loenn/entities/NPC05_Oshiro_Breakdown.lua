local npc05_oshiro_breakdown = {}

npc05_oshiro_breakdown.name = "MaggyHelper/NPC05_Oshiro_Breakdown"
npc05_oshiro_breakdown.depth = 0
npc05_oshiro_breakdown.justification = {0.5, 1.0}
npc05_oshiro_breakdown.texture = "characters/Maggy/DesoloZantas/oshiro/oshiro25"

npc05_oshiro_breakdown.placements = {
    {
        name = "NPC05_Oshiro_Breakdown",
        data = {
            dialogKey = "ingeste_oshiro_05_breakdown",
            flagName = "oshiro_05_breakdown",
            spriteId = "oshiro"
        }
    }
}

npc05_oshiro_breakdown.fieldInformation = {
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
            "oshiro",
            "theo",
            "Kglobal::Player"
        }
    }
}

return npc05_oshiro_breakdown

