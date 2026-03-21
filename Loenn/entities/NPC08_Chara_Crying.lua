local npc08_chara_crying = {}

npc08_chara_crying.name = "MaggyHelper/NPC08_Chara_Crying"
npc08_chara_crying.depth = 0
npc08_chara_crying.justification = {0.5, 1.0}
npc08_chara_crying.texture = "characters/chara/idle00"

npc08_chara_crying.placements = {
    {
        name = "NPC08_Chara_Crying",
        data = {
            dialogKey = "ingeste_chara_08_crying",
            flagName = "chara_08_crying",
            spriteId = "chara"
        }
    }
}

npc08_chara_crying.fieldInformation = {
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
    }
}

return npc08_chara_crying
