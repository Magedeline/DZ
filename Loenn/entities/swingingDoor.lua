local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local swingingDoor = {}

swingingDoor.name = "MaggyHelper/SwingingDoor"
swingingDoor.depth = -50
swingingDoor.fieldInformation = {
    swingSpeed = {
        minimumValue = 0
    },
    knockbackForce = {
        minimumValue = 0
    },
    autoCloseTime = {
        minimumValue = 0
    }
}
swingingDoor.placements = {
    {
        name = "unlocked",
        data = {
            swingSpeed = 3.0,
            knockbackForce = 150,
            isLocked = false,
            isDoubleDoor = true,
            autoCloseTime = 2.0
        }
    },
    {
        name = "locked",
        data = {
            swingSpeed = 3.0,
            knockbackForce = 150,
            isLocked = true,
            isDoubleDoor = true,
            autoCloseTime = 2.0
        }
    }
}

function swingingDoor.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/swinging_door/closed", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function swingingDoor.selection(room, entity)
    return utils.rectangle(entity.x - 20, entity.y - 48, 40, 48)
end

return swingingDoor
