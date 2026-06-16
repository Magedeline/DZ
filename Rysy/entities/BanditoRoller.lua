-- RySy plugin for DZ - BanditoRoller (Ch11 Western)
local Entity = {}

Entity.name = "DZ/BanditoRoller"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 2,
        rollSpeed = entity.rollSpeed or 150,
        bounceSpeed = entity.bounceSpeed or 200,
        detectionRange = entity.detectionRange or 200
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/bandito_roller/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
