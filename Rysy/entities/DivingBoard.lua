-- RySy plugin for DZ - DivingBoard
local Entity = {}

Entity.name = "DZ/DivingBoard"
Entity.depth = -1

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        launchSpeed = entity.launchSpeed or -300.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/divingBoard",
        x = x + 12,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
