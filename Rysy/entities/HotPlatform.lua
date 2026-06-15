-- RySy plugin for MaggyHelper - HotPlatform (Ch13 Inferno)
local Entity = {}

Entity.name = "MaggyHelper/HotPlatform"
Entity.depth = -9000
Entity.minimumSize = {8, 8}
Entity.canResize = {true, true}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 32,
        height = entity.height or 8,
        heatRate = entity.heatRate or 20,
        coolRate = entity.coolRate or 10,
        maxHeat = entity.maxHeat or 100
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/hot_platform/cool",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
