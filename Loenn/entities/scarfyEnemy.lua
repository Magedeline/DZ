-- Loenn plugin for DZ - Scarfy Enemy Entity
local drawableSprite = require("structs.drawable_sprite")

local scarfyEnemy = {}

scarfyEnemy.name = "DZ/ScarfyEnemy"
scarfyEnemy.depth = 0
scarfyEnemy.placements = {
    name = "ScarfyEnemy",
    data = {
        health = 2,
        moveSpeed = 20.0,
        chaseSpeed = 100.0,
        canBeInhaled = false
    }
}

scarfyEnemy.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    moveSpeed = {
        minimumValue = 0
    },
    chaseSpeed = {
        minimumValue = 0
    }
}

scarfyEnemy.fieldOrder = {
    "x", "y",
    "health",
    "moveSpeed",
    "chaseSpeed",
    "canBeInhaled"
}

function scarfyEnemy.sprite(room, entity)
    local texture = "objects/enemies/scarfy/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return scarfyEnemy
