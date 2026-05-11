local npc05_oshiro_suite = {}

npc05_oshiro_suite.name = "MaggyHelper/NPC05_Oshiro_Suite"
npc05_oshiro_suite.depth = 0
npc05_oshiro_suite.justification = {0.5, 1.0}
npc05_oshiro_suite.texture = "characters/Maggy/DesoloZantas/oshiro/oshiro25"

npc05_oshiro_suite.placements = {
    {
        name = "NPC05_Oshiro_Suite",
        data = {
            dialogKey = "ingeste_oshiro_05_suite",
            flagName = "oshiro_05_suite",
            spriteId = "oshiro"
        }
    }
}

npc05_oshiro_suite.fieldInformation = {
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

return npc05_oshiro_suite

