-- RySy plugin for MaggyHelper - StarBlock
local Entity = {}

Entity.name = "MaggyHelper/StarBlock"
Entity.depth = -10000
Entity.minimumSize = {8, 8}
Entity.canResize = {true, true}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 8,
        height = entity.height or 8
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local w = entity.width or 8
    local h = entity.height or 8
    local area = w * h
    local tex = "objects/starblock/normal"
    if area >= 256 then tex = "objects/starblock/oversized"
    elseif area >= 128 then tex = "objects/starblock/large" end
    return {
        texture = tex,
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
