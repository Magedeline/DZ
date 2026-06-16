-- RySy plugin for DZ - TowerGargoyle (Ch12 Titan Tower)
local Entity = {}

Entity.name = "DZ/TowerGargoyle"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 2,
        detectionRange = entity.detectionRange or 150,
        swoopSpeed = entity.swoopSpeed or 250,
        glideSpeed = entity.glideSpeed or 100
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/tower_gargoyle/dormant",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
