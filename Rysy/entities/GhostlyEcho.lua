-- RySy plugin for DZ - GhostlyEcho (Ch10 Ruins)
local Entity = {}

Entity.name = "DZ/GhostlyEcho"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        behavior = entity.behavior or "Mirror",
        mirrorDelay = entity.mirrorDelay or 0.5,
        fadeTime = entity.fadeTime or 2.0,
        alpha = entity.alpha or 0.6,
        isDangerous = entity.isDangerous ~= false,
        isSolid = entity.isSolid or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/ghostly_echo/dormant",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
