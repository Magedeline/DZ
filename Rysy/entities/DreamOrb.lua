-- RySy plugin for MaggyHelper - DreamOrb
local Entity = {}

Entity.name = "MaggyHelper/DreamOrb"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        oneUse = entity.oneUse or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/dreamorb/00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
