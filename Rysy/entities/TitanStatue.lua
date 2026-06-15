-- RySy plugin for MaggyHelper - TitanStatue (Ch12 Titan Tower)
local Entity = {}

Entity.name = "MaggyHelper/TitanStatue"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 5,
        canAwaken = entity.canAwaken or false,
        isAnimated = entity.isAnimated or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local state = entity.isAnimated and "active" or "inactive"
    return {
        texture = "objects/titan_statue/" .. state,
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
