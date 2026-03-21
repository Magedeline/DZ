local npc07_chara = {}

npc07_chara.name = "MaggyHelper/NPC07_Chara"
npc07_chara.depth = 0
npc07_chara.justification = {0.5, 1.0}
npc07_chara.texture = "characters/chara/idle00"

npc07_chara.placements = {
    {
        name = "NPC07_Chara",
        data = {
            dialogKey = "ingeste_chara_07_hell",
            flagName = "chara_07_hell",
            spriteId = "chara",
            index = 0,
            entityId = "chara_07_hell"
        }
    }
}

npc07_chara.fieldInformation = {
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
            "chara",
            "theo",
            "player",
            "madeline"
        }
    },
    index = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 10
    },
    entityId = {
        fieldType = "string"
    }
}

return npc07_chara
