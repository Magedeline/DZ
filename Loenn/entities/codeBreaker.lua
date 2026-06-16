local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local codeBreaker = {}

codeBreaker.name = "DZ/CodeBreaker"
codeBreaker.depth = -100
codeBreaker.fieldInformation = {
    code = {},
    inputTimeout = {
        minimumValue = 0
    }
}
codeBreaker.placements = {
    {
        name = "normal",
        data = {
            code = "1234",
            inputTimeout = 5.0
        }
    }
}

function codeBreaker.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/code_breaker/locked", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function codeBreaker.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 40, 48, 40)
end

return codeBreaker
