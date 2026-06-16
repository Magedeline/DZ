local drawableSprite = require("structs.drawable_sprite")

local soulColor = {1.0, 0.5, 0.0, 1.0} -- Orange
local soulIndex = 1

local braverySoulBoost = {}

braverySoulBoost.name = "DZ/BraverySoulBoost"
braverySoulBoost.depth = -1000000
braverySoulBoost.nodeLineRenderType = "line"
braverySoulBoost.nodeLimits = {1, -1}

braverySoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    abilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    invincibilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    }
}

braverySoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera", "canSkip", "oneUse",
    "boostSpeed", "abilityDuration",
    "invincibilityDuration", "breakSpinners"
}

braverySoulBoost.placements = {
    {
        name = "bravery_soul_boost",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 360.0,
            abilityDuration = 2.5,
            invincibilityDuration = 1.5,
            breakSpinners = true
        }
    }
}

function braverySoulBoost.sprite(room, entity)
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

function braverySoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local nodeSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", {x = node.x, y = node.y})
    nodeSprite:setJustification(0.5, 0.5)
    nodeSprite:setColor({soulColor[1], soulColor[2], soulColor[3], 0.4})
    table.insert(sprites, nodeSprite)
    
    return sprites
end

function braverySoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return braverySoulBoost
