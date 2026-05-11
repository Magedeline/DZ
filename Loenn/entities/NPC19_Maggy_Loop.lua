local npc19_maggy_loop = {}

npc19_maggy_loop.name = "MaggyHelper/NPC19_Maggy_Loop"
npc19_maggy_loop.depth = 0
npc19_maggy_loop.justification = {0.5, 1.0}
npc19_maggy_loop.texture = "characters/Maggy/DesoloZantas/magolor/idle00"

npc19_maggy_loop.placements = {
    {
        name = "NPC19_Maggy_Loop",
        data = {
            dialogKey = "ingeste_maggy_19_loop",
            flagName = "maggy_19_loop",
            spriteId = "magolor"
        }
    }
}

npc19_maggy_loop.fieldInformation = {
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
            "magolor",
            "theo",
            "Kglobal::Player",
            "madeline"
        }
    }
}

return npc19_maggy_loop

