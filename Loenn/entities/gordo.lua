-- Loenn plugin for DZ - Gordo Enemy Entity
local drawableSprite = require("structs.drawable_sprite")

local gordo = {}

gordo.name = "DZ/Gordo"
gordo.depth = 0
gordo.placements = {
    {
        name = "stationary",
        data = {
            movementType = "Stationary",
            moveDistance = 48.0,
            moveSpeed = 40.0,
            pauseDuration = 0.5
        }
    },
    {
        name = "horizontal",
        data = {
            movementType = "Horizontal",
            moveDistance = 48.0,
            moveSpeed = 40.0,
            pauseDuration = 0.5
        }
    },
    {
        name = "vertical",
        data = {
            movementType = "Vertical",
            moveDistance = 48.0,
            moveSpeed = 40.0,
            pauseDuration = 0.5
        }
    },
    {
        name = "diagonal",
        data = {
            movementType = "Diagonal",
            moveDistance = 48.0,
            moveSpeed = 40.0,
            pauseDuration = 0.5
        }
    },
    {
        name = "circular",
        data = {
            movementType = "Circular",
            moveDistance = 48.0,
            moveSpeed = 40.0,
            pauseDuration = 0.5
        }
    }
}

gordo.fieldInformation = {
    movementType = {
        options = {
            "Stationary",
            "Horizontal",
            "Vertical",
            "Diagonal",
            "Circular"
        },
        editable = false
    },
    moveDistance = {
        minimumValue = 0
    },
    moveSpeed = {
        minimumValue = 0
    },
    pauseDuration = {
        minimumValue = 0
    }
}

gordo.fieldOrder = {
    "x", "y",
    "movementType",
    "moveDistance",
    "moveSpeed",
    "pauseDuration"
}

function gordo.sprite(room, entity)
    local texture = "objects/enemies/gordo/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return gordo
