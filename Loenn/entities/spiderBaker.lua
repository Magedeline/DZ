local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local spiderBaker = {}

spiderBaker.name = "DZ/SpiderBaker"
spiderBaker.depth = -100
spiderBaker.fieldInformation = {
    health = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    webY = {
        fieldType = "integer"
    }
}
spiderBaker.placements = {
    {
        name = "neutral",
        data = {
            health = 2,
            detectionRange = 100,
            webY = -80,
            startFriendly = false
        }
    },
    {
        name = "friendly",
        data = {
            health = 2,
            detectionRange = 100,
            webY = -80,
            startFriendly = true
        }
    }
}

function spiderBaker.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/spider_baker/hanging", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    
    -- Draw web line up
    local webY = entity.webY or -80
    -- Web visual would be handled by game
    
    return sprite
end

function spiderBaker.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 20, 20, 20)
end

return spiderBaker
