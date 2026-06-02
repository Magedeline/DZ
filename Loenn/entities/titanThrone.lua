local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local titanThrone = {}

titanThrone.name = "MaggyHelper/TitanThrone"
titanThrone.depth = -100
titanThrone.fieldInformation = {
    activationRadius = {
        minimumValue = 0
    }
}
titanThrone.placements = {
    {
        name = "manual",
        data = {
            bossEntity = "MaggyHelper/KingTitanBoss",
            activationRadius = 100,
            autoActivate = false,
            cutsceneId = "CH15_ROARING_TITAN_KING_BATTLE"
        }
    },
    {
        name = "auto",
        data = {
            bossEntity = "MaggyHelper/KingTitanBoss",
            activationRadius = 100,
            autoActivate = true,
            cutsceneId = "CH15_ROARING_TITAN_KING_BATTLE"
        }
    }
}

function titanThrone.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/titan_throne/inactive", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.6, 0.5, 0.4, 1.0}
    return sprite
end

function titanThrone.selection(room, entity)
    return utils.rectangle(entity.x - 40, entity.y - 120, 80, 120)
end

return titanThrone
