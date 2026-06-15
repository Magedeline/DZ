local Entity = {}

Entity.name = "MaggyHelper/KirbyPuffJumpRefill"
Entity.depth = -100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        oneUse = entity.oneUse or false,
        jumps = entity.jumps or 5
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/puffrefill/idle00",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
