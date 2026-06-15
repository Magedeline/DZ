local Effect = {}

Effect.name = "MaggyHelper/ELSTrueFinalBackdrop"

function Effect.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        scrollX = entity.scrollX or 0.05,
        scrollY = entity.scrollY or 0.05,
        blendMode = entity.blendMode or "Additive"
    }
end

function Effect.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        type = "background",
        texture = "bgs/els_true_final_backdrop",
        x = x,
        y = y
    }
end

return Effect
