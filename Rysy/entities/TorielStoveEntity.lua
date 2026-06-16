-- RySy plugin for DZ - TorielStoveEntity (Ch10 Ruins)
local Entity = {}

Entity.name = "DZ/TorielStoveEntity"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        canInteract = entity.canInteract ~= false,
        hasPie = entity.hasPie ~= false,
        healAmount = entity.healAmount or 3,
        dialogueId = entity.dialogueId or "TORIEL_STOVE",
        cookDuration = entity.cookDuration or 5.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/toriel_stove/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
