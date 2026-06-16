local Entity = {}

Entity.name = "DZ/GoldBlock"
Entity.depth = 0

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 16,
        height = entity.height or 16,
        permanent = entity.permanent or true
    }
end

function Entity.draw(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 16
    local height = entity.height or 16
    return {
        type = "rectangle",
        x = x,
        y = y,
        width = width,
        height = height,
        color = {0.9, 0.7, 0.2, 0.8},
        borderColor = {1.0, 0.9, 0.4, 1.0}
    }
end

return Entity
