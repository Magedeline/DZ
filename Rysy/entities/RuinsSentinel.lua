-- RySy plugin for MaggyHelper - RuinsSentinel (Ch10 Ruins)
local Entity = {}

Entity.name = "MaggyHelper/RuinsSentinel"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 3,
        detectionRange = entity.detectionRange or 150,
        attackRange = entity.attackRange or 60,
        moveSpeed = entity.moveSpeed or 50,
        patrolDistance = entity.patrolDistance or 100
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/ruins_sentinel/dormant",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
