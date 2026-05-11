local npc05_oshiro_hallway1 = {}

npc05_oshiro_hallway1.name = "MaggyHelper/NPC05_Oshiro_Hallway1"
npc05_oshiro_hallway1.depth = 0
npc05_oshiro_hallway1.justification = {0.5, 1.0}
npc05_oshiro_hallway1.texture = "characters/Maggy/DesoloZantas/oshiro/oshiro25"

npc05_oshiro_hallway1.placements = {
    {
        name = "NPC05_Oshiro_Hallway1",
        data = {
            dialogKey = "ingeste_oshiro_05_hallway1",
            flagName = "oshiro_05_hallway1",
            spriteId = "oshiro"
        }
    }
}

npc05_oshiro_hallway1.fieldInformation = {
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

return npc05_oshiro_hallway1

