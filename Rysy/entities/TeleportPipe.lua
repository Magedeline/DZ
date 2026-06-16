local Entity = {}

Entity.name = "DZ/TeleportPipe"
Entity.depth = 100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        targetRoom = entity.targetRoom or "",
        targetId = entity.targetId or 0,
        direction = entity.direction or "Up",
        locked = entity.locked or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/teleportpipe/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
