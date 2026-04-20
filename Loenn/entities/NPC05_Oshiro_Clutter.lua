local npc05_oshiro_clutter = {}

npc05_oshiro_clutter.name = "MaggyHelper/NPC05_Oshiro_Clutter"
npc05_oshiro_clutter.depth = 0
npc05_oshiro_clutter.justification = {0.5, 1.0}
npc05_oshiro_clutter.texture = "characters/oshiro/oshiro25"

npc05_oshiro_clutter.placements = {
    {
        name = "NPC05_Oshiro_Clutter",
        data = {
            dialogKey = "ingeste_oshiro_05_clutter",
            flagName = "oshiro_05_clutter",
            spriteId = "oshiro"
        }
    }
}

npc05_oshiro_clutter.fieldInformation = {
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
            "player"
        }
    }
}

return npc05_oshiro_clutter
