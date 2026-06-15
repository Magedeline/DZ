-- RySy plugin for MaggyHelper - Bridge
local Entity = {}

Entity.name = "MaggyHelper/Bridge"
Entity.depth = 0
Entity.minimumSize = {8, 8}
Entity.canResize = {true, false}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 160,
        height = entity.height or 8,
        getLevelFlag = entity.getLevelFlag or ""
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/bridge/bridge",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
