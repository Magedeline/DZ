-- RySy plugin for MaggyHelper - FloweyTrap (Ch10 Ruins)
local Entity = {}

Entity.name = "MaggyHelper/FloweyTrap"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 2,
        detectionRange = entity.detectionRange or 120,
        retractRange = entity.retractRange or 180,
        pelletCount = entity.pelletCount or 5,
        pelletSpeed = entity.pelletSpeed or 150,
        attackInterval = entity.attackInterval or 1.5,
        attackPattern = entity.attackPattern or "Circular"
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/flowey_trap/hidden",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
