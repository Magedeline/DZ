local Entity = {}

Entity.name = "DZ/DashCopyBerry"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        ability = entity.ability or "Cutter",
        checkpoint = entity.checkpoint or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "collectibles/dashcopyberry/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
