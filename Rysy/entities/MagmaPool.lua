-- RySy plugin for DZ - MagmaPool (Ch13 Inferno)
local Entity = {}

Entity.name = "DZ/MagmaPool"
Entity.depth = -50
Entity.minimumSize = {16, 16}
Entity.canResize = {true, true}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 48,
        height = entity.height or 16,
        bubbleInterval = entity.bubbleInterval or 0.5,
        eruptInterval = entity.eruptInterval or 3.0,
        isInstantDeath = entity.isInstantDeath ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/magma_pool/idle",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
