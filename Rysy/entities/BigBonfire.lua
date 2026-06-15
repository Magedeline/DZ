-- RySy plugin for MaggyHelper - BigBonfire
local Entity = {}

Entity.name = "MaggyHelper/BigBonfire"
Entity.depth = -5

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        mode = entity.mode or "Unlit",
        scale = entity.scale or 2.0,
        lightInner = entity.lightInner or 64.0,
        lightOuter = entity.lightOuter or 128.0,
        bloomRadius = entity.bloomRadius or 64.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/campfire/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
