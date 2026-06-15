-- RySy plugin for MaggyHelper - VolcanicRock (Ch13 Inferno)
local Entity = {}

Entity.name = "MaggyHelper/VolcanicRock"
Entity.depth = -200

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        spawnInterval = entity.spawnInterval or 3.0,
        rockSpeed = entity.rockSpeed or 120.0,
        rockCount = entity.rockCount or 3
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/volcanic_rock/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
