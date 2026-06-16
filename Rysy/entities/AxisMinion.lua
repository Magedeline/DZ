-- RySy plugin for DZ - AxisMinion (Ch13 Inferno)
local Entity = {}

Entity.name = "DZ/AxisMinion"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 2,
        moveSpeed = entity.moveSpeed or 60,
        detectionRange = entity.detectionRange or 120,
        patrolDistance = entity.patrolDistance or 80
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/axis_minion/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
