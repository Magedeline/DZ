local npc05_magolor_vents = {}

npc05_magolor_vents.name = "MaggyHelper/NPC05_Magolor_Vents"
npc05_magolor_vents.depth = 0
npc05_magolor_vents.justification = {0.5, 1.0}
npc05_magolor_vents.texture = "characters/Maggy/DesoloZantas/magolor/idle00"

npc05_magolor_vents.placements = {
    {
        name = "NPC05_Magolor_Vents",
        data = {
            dialogKey = "CH5_MAGGY_VENTS",
            flagName = "magolorVentsTalked",
            spriteId = "magolor"
        }
    }
}

npc05_magolor_vents.fieldInformation = {
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

return npc05_magolor_vents
