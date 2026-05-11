-- Loenn plugin for MaggyHelper - Meta Knight Boss Entity
local drawableSprite = require("structs.drawable_sprite")

local metaKnightBoss = {}

metaKnightBoss.name = "MaggyHelper/MetaKnightBoss"
metaKnightBoss.depth = -10000
metaKnightBoss.placements = {
    name = "default",
    data = {
        health = 20,
        attackCooldown = 0.8,
        bossMusic = "event:/desolo_zantas/music/lvl13/metarminator_kight"
    }
}

metaKnightBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    attackCooldown = {
        minimumValue = 0.1
    }
}

metaKnightBoss.fieldOrder = {
    "x", "y",
    "health",
    "attackCooldown",
    "bossMusic"
}

function metaKnightBoss.sprite(room, entity)
    local texture = "objects/bosses/metaKnightBoss/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return metaKnightBoss
