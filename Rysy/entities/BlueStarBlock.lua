local Entity = {}

Entity.name = "DZ/BlueStarBlock"
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
    local width = entity.width or 8
    local height = entity.height or 8
    local area = width * height
    local texture
    if area >= 256 then
        texture = "objects/bluestarblock/oversized"
    elseif area >= 128 then
        texture = "objects/bluestarblock/large"
    else
        texture = "objects/bluestarblock/normal"
    end
    return {
        texture = texture,
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
