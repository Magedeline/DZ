-- RySy plugin for DZ - LavaBubble (Ch13 Inferno)
local Entity = {}

Entity.name = "DZ/LavaBubble"
Entity.depth = -200

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        riseSpeed = entity.riseSpeed or 80.0,
        burstHeight = entity.burstHeight or 100.0,
        damageRadius = entity.damageRadius or 50.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/lava_bubble/forming00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
