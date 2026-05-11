local npc05_oshiro_rooftop = {}

npc05_oshiro_rooftop.name = "MaggyHelper/NPC05_Oshiro_Rooftop"
npc05_oshiro_rooftop.depth = 0
npc05_oshiro_rooftop.justification = {0.5, 1.0}
npc05_oshiro_rooftop.texture = "characters/Maggy/DesoloZantas/oshiro/oshiro25"

npc05_oshiro_rooftop.placements = {
    {
        name = "NPC05_Oshiro_Rooftop",
        data = {
            dialogKey = "ingeste_oshiro_05_rooftop",
            flagName = "oshiro_05_rooftop",
            spriteId = "oshiro"
        }
    }
}

npc05_oshiro_rooftop.fieldInformation = {
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

return npc05_oshiro_rooftop

