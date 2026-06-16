local Entity = {}

Entity.name = "DZ/ClutterDoor"
Entity.depth = 0

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 32,
        height = entity.height or 32,
        type = entity.type or "Green"
    }
end

function Entity.draw(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 32
    local height = entity.height or 32
    return {
        type = "rectangle",
        x = x,
        y = y,
        width = width,
        height = height,
        color = {0.4, 0.4, 0.4, 0.8},
        borderColor = {1.0, 1.0, 1.0, 1.0}
    }
end

return Entity
