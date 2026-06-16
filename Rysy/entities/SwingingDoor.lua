-- RySy plugin for DZ - SwingingDoor (Ch11 Western)
local Entity = {}

Entity.name = "DZ/SwingingDoor"
Entity.depth = -50

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        swingSpeed = entity.swingSpeed or 3.0,
        knockbackForce = entity.knockbackForce or 150,
        isLocked = entity.isLocked or false,
        isDoubleDoor = entity.isDoubleDoor ~= false,
        autoCloseTime = entity.autoCloseTime or 2.0
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return {
        texture = "objects/swinging_door/closed",
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity
