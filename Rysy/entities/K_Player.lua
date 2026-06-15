local Entity = {}

Entity.name = "MaggyHelper/K_Player"
Entity.depth = -1

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        spriteMode = entity.spriteMode or "Madeline",
        introType = entity.introType or "Transition",
        maxHealth = entity.maxHealth or 6,
        kirbyMode = entity.kirbyMode or false,
        combatEnabled = entity.combatEnabled or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/player/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
