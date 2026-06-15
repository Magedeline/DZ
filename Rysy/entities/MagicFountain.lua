-- RySy plugin for MaggyHelper - MagicFountain (Ch12 Titan Tower)
local Entity = {}

Entity.name = "MaggyHelper/MagicFountain"
Entity.depth = 0

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        fountainType = entity.fountainType or "healing",
        particleCount = entity.particleCount or 50,
        isActive = entity.isActive ~= false,
        usesRemaining = entity.usesRemaining or -1,
        healAmount = entity.healAmount or 1
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/fountain/fountain_idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
