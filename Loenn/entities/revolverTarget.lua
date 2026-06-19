local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local revolverTarget = {}

revolverTarget.name = "DZ/RevolverTarget"
revolverTarget.depth = -50
revolverTarget.fieldInformation = {
    targetType = {
        options = {"Static", "Popup", "Moving", "Swinging"},
        editable = false
    },
    points = {
        minimumValue = 0
    },
    showTime = {
        minimumValue = 0
    },
    resetTime = {
        minimumValue = 0
    },
    moveSpeed = {
        minimumValue = 0
    }
}
revolverTarget.placements = {
    {
        name = "main",
        data = {
            targetType = "Static",
            points = 100,
            showTime = 2.0,
            resetTime = 3.0,
            moveSpeed = 50
        }
    },
    {
        name = "popup",
        data = {
            targetType = "Popup",
            points = 150,
            showTime = 2.0,
            resetTime = 3.0,
            moveSpeed = 50
        }
    },
    {
        name = "moving",
        data = {
            targetType = "Moving",
            points = 200,
            showTime = 2.0,
            resetTime = 3.0,
            moveSpeed = 50
        }
    }
}

function revolverTarget.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/revolver_target/ready", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function revolverTarget.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 20, 20, 20)
end

return revolverTarget
