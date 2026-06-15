-- RySy plugin for MaggyHelper - Buzzo Mid-Boss
local Entity = {}

Entity.name = "MaggyHelper/BuzzoBoss"
Entity.depth = -8500

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 280,
        maxHealth = entity.maxHealth or 280
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/buzzo/body_idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
