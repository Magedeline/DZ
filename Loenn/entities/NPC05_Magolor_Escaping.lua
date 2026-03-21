local npc05_magolor_escaping = {}

npc05_magolor_escaping.name = "MaggyHelper/NPC05_Magolor_Escaping"
npc05_magolor_escaping.depth = 0
npc05_magolor_escaping.justification = {0.5, 1.0}
npc05_magolor_escaping.texture = "characters/magolor/idle00"

npc05_magolor_escaping.placements = {
    {
        name = "NPC05_Magolor_Escaping",
        data = {
            dialogKey = "CH5_MAGGY_ESCAPING",
            flagName = "resort_maggy_escaped",
            spriteId = "magolor"
        }
    }
}

npc05_magolor_escaping.fieldInformation = {
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
            "player",
            "madeline"
        }
    }
}

return npc05_magolor_escaping
