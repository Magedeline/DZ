local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local darkMirror = {}

darkMirror.name = "DZ/DarkMirror"
darkMirror.depth = -50
darkMirror.fieldInformation = {
    health = {
        minimumValue = 1
    }
}
darkMirror.placements = {
    {
        name = "main",
        data = {
            health = 2,
            revealsSecret = false
        }
    },
    {
        name = "secret",
        data = {
            health = 2,
            revealsSecret = true
        }
    }
}

function darkMirror.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/dark_mirror/intact", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.0, 0.0, 0.4, 0.8}
    return sprite
end

function darkMirror.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 64, 48, 64)
end

return darkMirror
