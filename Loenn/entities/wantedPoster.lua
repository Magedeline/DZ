local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local wantedPoster = {}

wantedPoster.name = "DZ/WantedPoster"
wantedPoster.depth = -50
wantedPoster.fieldInformation = {
    bountyReward = {
        minimumValue = 0
    },
    enemyCount = {
        minimumValue = 1
    }
}
wantedPoster.placements = {
    {
        name = "main",
        data = {
            bountyName = "OUTLAW",
            bountyReward = 100,
            enemyType = "DZ/BanditoRoller",
            enemyCount = 3
        }
    }
}

function wantedPoster.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/wanted_poster/unread", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function wantedPoster.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 24, 16, 24)
end

return wantedPoster
