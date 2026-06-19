local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local abyssalEye = {}

abyssalEye.name = "DZ/AbyssalEye"
abyssalEye.depth = -100
abyssalEye.fieldInformation = {
    gazeRange = {
        minimumValue = 0
    },
    gazeWidth = {
        minimumValue = 0
    },
    health = {
        minimumValue = 1
    }
}
abyssalEye.placements = {
    {
        name = "main",
        data = {
            gazeRange = 200,
            gazeWidth = 30,
            health = 2
        }
    }
}

function abyssalEye.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/abyssal_eye/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 0.5)
    sprite.color = {0.5, 0.0, 0.0, 1.0}
    return sprite
end

function abyssalEye.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 32, 32)
end

return abyssalEye
