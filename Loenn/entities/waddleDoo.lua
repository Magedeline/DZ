-- Loenn plugin for DZ - Waddle Doo Enemy Entity
local drawableSprite = require("structs.drawable_sprite")

local waddleDoo = {}

waddleDoo.name = "DZ/WaddleDoo"
waddleDoo.depth = 0
waddleDoo.placements = {
    name = "WaddleDoo",
    data = {
        health = 2,
        moveSpeed = 25.0,
        attackCooldown = 3.0,
        canBeInhaled = true
    }
}

waddleDoo.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    moveSpeed = {
        minimumValue = 0
    },
    attackCooldown = {
        minimumValue = 0.5
    }
}

waddleDoo.fieldOrder = {
    "x", "y",
    "health",
    "moveSpeed",
    "attackCooldown",
    "canBeInhaled"
}

function waddleDoo.sprite(room, entity)
    local texture = "objects/enemies/waddleDoo/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return waddleDoo
