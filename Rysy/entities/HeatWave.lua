-- RySy plugin for DZ - HeatWave (Ch13 Inferno)
local Entity = {}

Entity.name = "DZ/HeatWave"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        maxRadius = entity.maxRadius or 150,
        expansionSpeed = entity.expansionSpeed or 100,
        pushForce = entity.pushForce or 150,
        interval = entity.interval or 5.0,
        isActive = entity.isActive ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "effects/heat_wave/dormant",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
