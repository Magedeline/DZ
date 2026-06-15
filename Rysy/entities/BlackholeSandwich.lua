-- RySy plugin for MaggyHelper - BlackholeSandwich
local Entity = {}

Entity.name = "MaggyHelper/BlackholeSandwich"
Entity.depth = -50
Entity.minimumSize = {8, 8}
Entity.canResize = {true, true}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        width = entity.width or 64,
        height = entity.height or 128,
        mode = entity.mode or "Hot",
        speed = entity.speed or 80.0,
        glitchy = entity.glitchy ~= false,
        canSwitch = entity.canSwitch ~= false,
        switchFlag = entity.switchFlag or ""
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/IngesteHelper/blackhole_sandwich_space",
        x = x,
        y = y,
        justificationX = 0.0,
        justificationY = 0.0
    }
end

return Entity
