local Entity = {}

Entity.name = "MaggyHelper/ELSTerminaFinalBoss"
Entity.depth = -12500

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        difficultyMode = entity.difficultyMode or 0,
        fromCutscene = entity.fromCutscene or false,
        hasFiveHeartGems = entity.hasFiveHeartGems or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/els_termina_final/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
