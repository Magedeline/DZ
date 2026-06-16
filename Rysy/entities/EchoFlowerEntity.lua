-- RySy plugin for DZ - EchoFlowerEntity (Ch10 Ruins)
local Entity = {}

Entity.name = "DZ/EchoFlowerEntity"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        echoDelay = entity.echoDelay or 0.5,
        echoSpeed = entity.echoSpeed or 200,
        cooldownTime = entity.cooldownTime or 1.0,
        maxEchoes = entity.maxEchoes or 3
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/echo_flower/idle",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
