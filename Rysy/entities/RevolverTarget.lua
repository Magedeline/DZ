-- RySy plugin for MaggyHelper - RevolverTarget (Ch11 Western)
local Entity = {}

Entity.name = "MaggyHelper/RevolverTarget"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        targetType = entity.targetType or "Static",
        points = entity.points or 100,
        showTime = entity.showTime or 2.0,
        resetTime = entity.resetTime or 3.0,
        moveSpeed = entity.moveSpeed or 50
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/revolver_target/ready",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
