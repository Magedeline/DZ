local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local ghostlyEcho = {}

ghostlyEcho.name = "DZ/GhostlyEcho"
ghostlyEcho.depth = -50
ghostlyEcho.fieldInformation = {
    mirrorDelay = {
        minimumValue = 0
    },
    fadeTime = {
        minimumValue = 0
    },
    alpha = {
        minimumValue = 0,
        maximumValue = 1
    },
    behavior = {
        options = {"Mirror", "Reverse", "Patrol", "Chase"},
        editable = false
    }
}
ghostlyEcho.placements = {
    {
        name = "main",
        data = {
            behavior = "Mirror",
            mirrorDelay = 0.5,
            fadeTime = 2.0,
            alpha = 0.6,
            isDangerous = true,
            isSolid = false
        }
    },
    {
        name = "chase",
        data = {
            behavior = "Chase",
            mirrorDelay = 0.5,
            fadeTime = 2.0,
            alpha = 0.6,
            isDangerous = true,
            isSolid = false
        }
    }
}

function ghostlyEcho.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/ghostly_echo/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.7, 0.8, 1.0, entity.alpha or 0.6}
    return sprite
end

function ghostlyEcho.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 24, 16, 24)
end

return ghostlyEcho
