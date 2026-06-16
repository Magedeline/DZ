-- Loenn plugin for DZ - Shadow Kirby Boss Entity
local drawableSprite = require("structs.drawable_sprite")

local kirbyBoss = {}

kirbyBoss.name = "DZ/KirbyBoss"
kirbyBoss.depth = -10000
kirbyBoss.placements = {
    name = "default",
    data = {
        health = 15,
        attackCooldown = 2.0,
        bossMusic = "event:/music/pusheen/miniboss/main0"
    }
}

kirbyBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    attackCooldown = {
        minimumValue = 0.5
    }
}

kirbyBoss.fieldOrder = {
    "x", "y",
    "health",
    "attackCooldown",
    "bossMusic"
}

function kirbyBoss.sprite(room, entity)
    local texture = "objects/bosses/kirbyBoss/idle00"
    return drawableSprite.fromTexture(texture, entity)
end

return kirbyBoss
