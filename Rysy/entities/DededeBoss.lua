-- RySy plugin for MaggyHelper - King Dedede Boss
local Entity = {}

Entity.name = "MaggyHelper/DededeBoss"
Entity.depth = -10000

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 25,
        attackCooldown = entity.attackCooldown or 1.5
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/bosses/dededeBoss/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
