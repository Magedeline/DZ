-- RySy plugin for DZ - CardShark (Ch11 Western)
local Entity = {}

Entity.name = "DZ/CardShark"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 2,
        detectionRange = entity.detectionRange or 180,
        throwInterval = entity.throwInterval or 1.5,
        cardsPerThrow = entity.cardsPerThrow or 3,
        patrolDistance = entity.patrolDistance or 80
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/card_shark/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
