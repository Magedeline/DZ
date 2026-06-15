local Effect = {}

Effect.name = "MaggyHelper/PopstarBackground"

function Effect.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        scrollX = entity.scrollX or 0.5,
        scrollY = entity.scrollY or 0.5
    }
end

function Effect.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        type = "background",
        texture = "bgs/popstar_background",
        x = x,
        y = y
    }
end

return Effect
