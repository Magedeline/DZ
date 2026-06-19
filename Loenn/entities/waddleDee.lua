-- Loenn plugin for DZ - Waddle Dee Enemy Entity
local drawableSprite = require("structs.drawable_sprite")

local waddleDee = {}

waddleDee.name = "DZ/WaddleDee"
waddleDee.depth = 0
waddleDee.placements = {
    {
        name = "main",
        data = {
        health = 1,
        moveSpeed = 30.0,
        patrolDistance = 50.0,
        canBeInhaled = true
        }
    }
}

waddleDee.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    moveSpeed = {
        minimumValue = 0
    },
    patrolDistance = {
        minimumValue = 0
    }
}

waddleDee.fieldOrder = {
    "x", "y",
    "health",
    "moveSpeed",
    "patrolDistance",
    "canBeInhaled"
}

function waddleDee.sprite(room, entity)
    local texture = "objects/enemies/waddleDee/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return waddleDee
