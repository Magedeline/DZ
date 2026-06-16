-- RySy plugin for DZ - SpiderBaker (Ch10 Ruins)
local Entity = {}

Entity.name = "DZ/SpiderBaker"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        health = entity.health or 2,
        detectionRange = entity.detectionRange or 100,
        webY = entity.webY or -80,
        startFriendly = entity.startFriendly or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "characters/spider_baker/hanging",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
