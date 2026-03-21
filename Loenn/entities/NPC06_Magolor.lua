local npc06_magolor = {}

npc06_magolor.name = "DesoloZantas/NPC06_Magolor"
npc06_magolor.depth = 0
npc06_magolor.justification = {0.5, 1.0}
npc06_magolor.texture = "characters/magolor/idle00"

npc06_magolor.placements = {
    {
        name = "NPC06_Magolor",
        data = {
            dialogue = "NPC06_MAGOLOR_DEFAULT",
            floating = true
        }
    }
}

npc06_magolor.fieldInformation = {
    dialogue = {
        fieldType = "string",
        options = {
            "NPC06_MAGOLOR_DEFAULT",
            "NPC06_MAGOLOR_GONDOLA_INTRO",
            "NPC06_MAGOLOR_GONDOLA_EXPLAIN",
            "NPC06_MAGOLOR_GONDOLA_READY", 
            "NPC06_MAGOLOR_GONDOLA_ENCOURAGE",
            "NPC06_MAGOLOR_GONDOLA_COMPLETE"
        }
    },
    floating = {
        fieldType = "boolean"
    }
}

return npc06_magolor