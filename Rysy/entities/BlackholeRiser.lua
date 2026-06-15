-- RySy plugin for MaggyHelper - BlackholeRiser
local Entity = {}

Entity.name = "MaggyHelper/BlackholeRiser"
Entity.depth = -50
Entity.canResize = {true, false}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 32,
        speed = entity.speed or 120.0,
        maxHeight = entity.maxHeight or 200.0,
        riseDelay = entity.riseDelay or 1.0,
        glitchy = entity.glitchy ~= false,
        looping = entity.looping ~= false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/IngesteHelper/blackhole_riser_base",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
