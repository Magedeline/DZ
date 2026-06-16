local drawableSprite = require("structs.drawable_sprite")

local soulColor = {0.0, 1.0, 1.0, 1.0} -- Cyan
local soulIndex = 4

local patienceSoulBoost = {}

patienceSoulBoost.name = "DZ/PatienceSoulBoost"
patienceSoulBoost.depth = -1000000
patienceSoulBoost.nodeLineRenderType = "line"
patienceSoulBoost.nodeLimits = {1, -1}

patienceSoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    abilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    slowdownDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    slowdownAmount = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 1.0
    },
    extendedAirTime = {
        fieldType = "number",
        minimumValue = 1.0
    }
}

patienceSoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera", "canSkip", "oneUse",
    "boostSpeed", "abilityDuration",
    "slowdownDuration", "slowdownAmount", "extendedAirTime"
}

patienceSoulBoost.placements = {
    {
        name = "patience_soul_boost",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 240.0,
            abilityDuration = 3.0,
            slowdownDuration = 2.0,
            slowdownAmount = 0.5,
            extendedAirTime = 1.5
        }
    }
}

function patienceSoulBoost.sprite(room, entity)
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

function patienceSoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local nodeSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", {x = node.x, y = node.y})
    nodeSprite:setJustification(0.5, 0.5)
    nodeSprite:setColor({soulColor[1], soulColor[2], soulColor[3], 0.4})
    table.insert(sprites, nodeSprite)
    
    return sprites
end

function patienceSoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return patienceSoulBoost
