-- RySy plugin for MaggyHelper - FlameGeyser (Ch13 Inferno)
local Entity = {}

Entity.name = "MaggyHelper/FlameGeyser"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        eruptInterval = entity.eruptInterval or 4.0,
        eruptDuration = entity.eruptDuration or 1.0,
        warningTime = entity.warningTime or 1.0,
        flameHeight = entity.flameHeight or 200,
        damageRadius = entity.damageRadius or 30
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/flame_geyser/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
