local npc05_oshiro_hallway2 = {}

npc05_oshiro_hallway2.name = "MaggyHelper/NPC05_Oshiro_Hallway2"
npc05_oshiro_hallway2.depth = 0
npc05_oshiro_hallway2.justification = {0.5, 1.0}
npc05_oshiro_hallway2.texture = "characters/Maggy/DesoloZantas/oshiro/oshiro25"

npc05_oshiro_hallway2.placements = {
    {
        name = "NPC05_Oshiro_Hallway2",
        data = {
            dialogKey = "ingeste_oshiro_05_hallway2",
            flagName = "oshiro_05_hallway2",
            spriteId = "oshiro"
        }
    }
}

npc05_oshiro_hallway2.fieldInformation = {
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

return npc05_oshiro_hallway2

