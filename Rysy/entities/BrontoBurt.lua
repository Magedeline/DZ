local Entity = {}

Entity.name = "DZ/BrontoBurt"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        behavior = entity.behavior or "fly",
        facing = entity.facing or 1,
        patrolDistance = entity.patrolDistance or 64
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/brontoburt/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
