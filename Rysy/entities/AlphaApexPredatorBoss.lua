-- RySy plugin for DZ - Alpha Apex Predator Boss
local Entity = {}

Entity.name = "DZ/AlphaApexPredatorBoss"
Entity.depth = -8500

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 1600,
        maxHealth = entity.maxHealth or 1600
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/monsters/predator00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
