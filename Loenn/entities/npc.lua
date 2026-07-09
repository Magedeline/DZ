local npc = {}

npc.name = "DZ/NPC"
npc.depth = 0
npc.texture = "character/DZ/player"

npc.placements = {
    name = "npc",
    data = {
        dialogKey = "",
        flagName = "",
        eventId = "",
        spriteId = "",
        cutsceneClass = "",
        onlyOnce = false,
        unskippable = false
    },
    npc = {
        borderColor = {0.0, 1.0, 1.0, 1.0},
        fillColor = {0.0, 1.0, 1.0, 0.4},
        nodeTexture = "objects/LuaCutscenes/hover_idle"
    }
}

return npc
