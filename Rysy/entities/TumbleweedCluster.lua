-- RySy plugin for DZ - TumbleweedCluster (Ch11 Western)
local Entity = {}

Entity.name = "DZ/TumbleweedCluster"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        rollSpeed = entity.rollSpeed or 180,
        pushForce = entity.pushForce or 100,
        tumbleweedCount = entity.tumbleweedCount or 3,
        bounceChance = entity.bounceChance or 0.3
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/tumbleweed/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
