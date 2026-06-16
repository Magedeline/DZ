-- RySy plugin for DZ - Blackhole Angel Boss (final boss)
local Entity = {}

Entity.name = "DZ/BlackholeAngelBoss"
Entity.depth = -8500

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 1500,
        maxHealth = entity.maxHealth or 1500
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/blackhole_angel/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
