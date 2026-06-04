-- Loenn plugin for MaggyHelper - King Dedede Boss Entity
local drawableSprite = require("structs.drawable_sprite")

local dededeBoss = {}

dededeBoss.name = "MaggyHelper/DededeBoss"
dededeBoss.depth = -10000
dededeBoss.placements = {
    name = "default",
    data = {
        health = 25,
        attackCooldown = 1.5,
        bossMusic = "guid://{38e2f39c-382d-4136-86fd-e24520f3b71e}"
    }
}

dededeBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    attackCooldown = {
        minimumValue = 0.5
    }
}

dededeBoss.fieldOrder = {
    "x", "y",
    "health",
    "attackCooldown",
    "bossMusic"
}

function dededeBoss.sprite(room, entity)
    local texture = "objects/bosses/dededeBoss/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return dededeBoss

