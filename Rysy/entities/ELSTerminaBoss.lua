local Entity = {}

Entity.name = "DZ/ELSTerminaBoss"
Entity.depth = -12500

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        phase = entity.phase or 4,
        fromCutscene = entity.fromCutscene or false,
        hardMode = entity.hardMode or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/els_termina/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
