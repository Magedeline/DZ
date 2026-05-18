local npc07_maddy_mirror = {}

npc07_maddy_mirror.name = "MaggyHelper/NPC07_Maddy_Mirror"
npc07_maddy_mirror.depth = 0
npc07_maddy_mirror.justification = {0.5, 1.0}
npc07_maddy_mirror.texture = "characters/Maggy/DesoloZantas/madeline/idle00"

npc07_maddy_mirror.placements = {
    {
        name = "NPC07_Maddy_Mirror",
        data = {
            dialogKey = "ingeste_maddy_07_mirror",
            flagName = "maddy_07_mirror",
            spriteId = "madeline"
        }
    }
}

npc07_maddy_mirror.fieldInformation = {
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
            "madeline",
            "Kglobal::Player",
            "badeline",
            "theo"
        }
    }
}

return npc07_maddy_mirror
