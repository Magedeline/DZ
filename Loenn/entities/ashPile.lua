-- Loenn plugin for DZ - AshPile (Ch13 Inferno)
-- Volcanic ash pile that slows climbing players; can collapse
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local ashPile = {}

ashPile.name = "DZ/AshPile"
ashPile.depth = -100
ashPile.minimumSize = {8, 8}
ashPile.canResize = {true, true}

ashPile.placements = {
    {
        name = "main",
        data = {
            width = 32,
            height = 16,
            climbSlowFactor = 0.5,
            shiftInterval = 3.0,
            isClimbable = true
        }
    }
}

ashPile.fieldInformation = {
    climbSlowFactor = {
        minimumValue = 0.1,
        maximumValue = 1.0
    },
    shiftInterval = {
        minimumValue = 0.5
    },
    isClimbable = {
        fieldType = "boolean"
    }
}

ashPile.fieldOrder = {
    "x", "y", "width", "height",
    "climbSlowFactor",
    "shiftInterval",
    "isClimbable"
}

function ashPile.sprite(room, entity)
    local width = entity.width or 32
    local height = entity.height or 16
    local sprite = drawableSprite.fromTexture("objects/ash_pile/stable00", entity)
    if sprite then
        sprite:setJustification(0.0, 0.0)
        sprite:setScale(width / sprite.meta.width, height / sprite.meta.height)
        return {sprite}
    end
    return {}
end

function ashPile.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 32, entity.height or 16)
end

return ashPile
