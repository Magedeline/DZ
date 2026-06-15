-- RySy plugin for MaggyHelper - LavaBubbleSpawner (Ch13 Inferno)
local Entity = {}

Entity.name = "MaggyHelper/LavaBubbleSpawner"
Entity.depth = -200

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        spawnInterval = entity.spawnInterval or 2.0,
        maxBubbles = entity.maxBubbles or 3
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/lava_bubble/forming00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity
