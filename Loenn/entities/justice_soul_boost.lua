local drawableSprite = require("structs.drawable_sprite")

local soulColor = {1.0, 1.0, 0.0, 1.0} -- Yellow
local soulIndex = 2

local justiceSoulBoost = {}

justiceSoulBoost.name = "DZ/JusticeSoulBoost"
justiceSoulBoost.depth = -1000000
justiceSoulBoost.nodeLineRenderType = "line"
justiceSoulBoost.nodeLimits = {1, -1}

justiceSoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    abilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    projectileCount = {
        fieldType = "integer",
        minimumValue = 1
    },
    projectileSpeed = {
        fieldType = "number",
        minimumValue = 100.0
    }
}

justiceSoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera", "canSkip", "oneUse",
    "boostSpeed", "abilityDuration",
    "projectileCount", "projectileSpeed", "spreadShot"
}

justiceSoulBoost.placements = {
    {
        name = "main",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 320.0,
            abilityDuration = 2.0,
            projectileCount = 5,
            projectileSpeed = 400.0,
            spreadShot = true
        }
    }
}

function justiceSoulBoost.sprite(room, entity)
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

function justiceSoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local nodeSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", {x = node.x, y = node.y})
    nodeSprite:setJustification(0.5, 0.5)
    nodeSprite:setColor({soulColor[1], soulColor[2], soulColor[3], 0.4})
    table.insert(sprites, nodeSprite)
    
    return sprites
end

function justiceSoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return justiceSoulBoost
