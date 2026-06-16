-- RySy plugin for DZ - SpiralStaircase (Ch12 Titan Tower)
local Entity = {}

Entity.name = "DZ/SpiralStaircase"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        rotationSpeed = entity.rotationSpeed or 0.5,
        maxSpeed = entity.maxSpeed or 2.0,
        platformCount = entity.platformCount or 8,
        radius = entity.radius or 100,
        clockwise = entity.clockwise ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/spiral_staircase/center",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
