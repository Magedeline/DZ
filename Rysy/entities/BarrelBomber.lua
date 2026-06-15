-- RySy plugin for MaggyHelper - BarrelBomber (Ch11 Western)
local Entity = {}

Entity.name = "MaggyHelper/BarrelBomber"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 1,
        detectionRange = entity.detectionRange or 80,
        explosionRadius = entity.explosionRadius or 100,
        fuseTime = entity.fuseTime or 1.5
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/barrel_bomber/hidden",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
