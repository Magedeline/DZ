local Entity = {}

Entity.name = "DZ/MetaKnightBoss"
Entity.depth = -12500

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 20,
        attackCooldown = entity.attackCooldown or 0.8
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/metaknight/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
