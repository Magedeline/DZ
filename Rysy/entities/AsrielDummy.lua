-- RySy plugin for MaggyHelper - AsrielDummy
local Entity = {}

Entity.name = "MaggyHelper/AsrielDummy"
Entity.depth = 0

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        facing = entity.facing or 1,
        animation = entity.animation or "idle",
        scale = entity.scale or 1.0,
        alpha = entity.alpha or 1.0,
        isVisible = entity.isVisible ~= false,
        playAnimationOnSpawn = entity.playAnimationOnSpawn or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0
    return {
        texture = "characters/asriel/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale
    }
end

return Entity
