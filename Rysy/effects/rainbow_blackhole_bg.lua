local Effect = {}

Effect.name = "DZ/RainbowBlackholeBg"

function Effect.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        scrollX = entity.scrollX or 0.1,
        scrollY = entity.scrollY or 0.1,
        color = entity.color or "Rainbow",
        intensity = entity.intensity or 1.0
    }
end

function Effect.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        type = "background",
        texture = "bgs/rainbow_blackhole",
        x = x,
        y = y
    }
end

return Effect
