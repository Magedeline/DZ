local drawableSprite = require("structs.drawable_sprite")

local soulColor = {0.0, 0.0, 1.0, 1.0} -- Blue
local soulIndex = 5

local integritySoulBoost = {}

integritySoulBoost.name = "DZ/IntegritySoulBoost"
integritySoulBoost.depth = -1000000
integritySoulBoost.nodeLineRenderType = "line"
integritySoulBoost.nodeLimits = {1, -1}

integritySoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    abilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    speedMultiplier = {
        fieldType = "number",
        minimumValue = 1.0
    },
    momentumDuration = {
        fieldType = "number",
        minimumValue = 0.5
    }
}

integritySoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera", "canSkip", "oneUse",
    "boostSpeed", "abilityDuration",
    "speedMultiplier", "momentumDuration", "allowWallBounce"
}

integritySoulBoost.placements = {
    {
        name = "integrity_soul_boost",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 480.0,
            abilityDuration = 2.0,
            speedMultiplier = 1.8,
            momentumDuration = 3.0,
            allowWallBounce = true
        }
    }
}

function integritySoulBoost.sprite(room, entity)
    local sprites = {}
    
    local mainSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", entity)
    mainSprite:setJustification(0.5, 0.5)
    mainSprite:setColor(soulColor)
    table.insert(sprites, mainSprite)
    
    local soulSprite = drawableSprite.fromTexture("objects/sevensoulboost/soul/vessel_soul0" .. soulIndex, entity)
    soulSprite:setJustification(0.5, 0.5)
    soulSprite:setColor(soulColor)
    soulSprite:addPosition(0, -12)
    table.insert(sprites, soulSprite)
    
    return sprites
end

function integritySoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local nodeSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", {x = node.x, y = node.y})
    nodeSprite:setJustification(0.5, 0.5)
    nodeSprite:setColor({soulColor[1], soulColor[2], soulColor[3], 0.4})
    table.insert(sprites, nodeSprite)
    
    return sprites
end

function integritySoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return integritySoulBoost
