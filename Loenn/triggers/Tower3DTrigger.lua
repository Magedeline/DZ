local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local tower3DTrigger = {}

tower3DTrigger.name = "MaggyHelper/Tower3DTrigger"
tower3DTrigger.placements = {
    {
        name = "activate_tower",
        data = {
            triggerType = "Activate",
            towerHeight = 1000.0,
            enableClimbing = true,
            climbingSpeed = 100.0,
            rotationSpeed = 1.0,
            oneUse = true,
            requirePlayerOnGround = false,
            flagToSet = "",
            requiredFlag = "",
            triggerDelay = 0.0,
            width = 16,
            height = 16
        }
    },
    {
        name = "deactivate_tower",
        data = {
            triggerType = "Deactivate",
            towerHeight = 0.0,
            enableClimbing = false,
            climbingSpeed = 100.0,
            rotationSpeed = 0.0,
            oneUse = false,
            requirePlayerOnGround = false,
            flagToSet = "",
            requiredFlag = "",
            triggerDelay = 0.0,
            width = 16,
            height = 16
        }
    },
    {
        name = "modify_tower",
        data = {
            triggerType = "Modify",
            towerHeight = 1500.0,
            enableClimbing = true,
            climbingSpeed = 120.0,
            rotationSpeed = 2.0,
            oneUse = false,
            requirePlayerOnGround = false,
            flagToSet = "tower_modified",
            requiredFlag = "",
            triggerDelay = 1.0,
            width = 16,
            height = 16
        }
    },
    {
        name = "tower_checkpoint",
        data = {
            triggerType = "Checkpoint",
            towerHeight = 0.0,
            enableClimbing = false,
            climbingSpeed = 100.0,
            rotationSpeed = 1.0,
            oneUse = false,
            requirePlayerOnGround = true,
            flagToSet = "tower_checkpoint_reached",
            requiredFlag = "",
            triggerDelay = 0.0,
            width = 32,
            height = 16
        }
    }
}

tower3DTrigger.fieldInformation = {
    triggerType = {
        options = {"Activate", "Deactivate", "Modify", "Checkpoint"},
        editable = false
    },
    towerHeight = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 3000.0
    },
    climbingSpeed = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 200.0
    },
    rotationSpeed = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    triggerDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    flagToSet = {
        fieldType = "string"
    },
    requiredFlag = {
        fieldType = "string"
    }
}

local triggerColors = {
    Activate = {0.2, 1.0, 0.2, 0.6}, -- Green
    Deactivate = {1.0, 0.2, 0.2, 0.6}, -- Red
    Modify = {0.2, 0.2, 1.0, 0.6}, -- Blue
    Checkpoint = {1.0, 1.0, 0.2, 0.6} -- Yellow
}

function tower3DTrigger.sprite(room, entity)
    local sprites = {}
    local triggerType = entity.triggerType or "Activate"
    
    -- Main trigger rectangle
    local width = entity.width or 16
    local height = entity.height or 16
    local color = triggerColors[triggerType] or {0.5, 0.5, 0.5, 0.6}
    
    local triggerSprite = drawableSprite.fromTexture("util/pixel", entity)
    triggerSprite:setColor(color)
    triggerSprite:setScale(width, height)
    table.insert(sprites, triggerSprite)
    
    -- Icon based on trigger type
    local iconTexture = "objects/IngesteHelper/triggers/tower3d_" .. string.lower(triggerType)
    local iconSprite = drawableSprite.fromTexture(iconTexture, entity)
    iconSprite:setColor({1.0, 1.0, 1.0, 0.9})
    table.insert(sprites, iconSprite)
    
    -- Type label
    local labelSprite = drawableSprite.fromText(triggerType, entity.x, entity.y - height/2 - 12, "small")
    labelSprite:setColor({1.0, 1.0, 1.0, 1.0})
    table.insert(sprites, labelSprite)
    
    -- Flag indicators
    if entity.flagToSet and entity.flagToSet ~= "" then
        local flagText = "Set: " .. entity.flagToSet
        local flagSprite = drawableSprite.fromText(flagText, entity.x, entity.y + height/2 + 8, "small")
        flagSprite:setColor({0.2, 1.0, 0.2, 0.8})
        table.insert(sprites, flagSprite)
    end
    
    if entity.requiredFlag and entity.requiredFlag ~= "" then
        local reqText = "Req: " .. entity.requiredFlag
        local reqSprite = drawableSprite.fromText(reqText, entity.x, entity.y + height/2 + 20, "small")
        reqSprite:setColor({1.0, 0.8, 0.2, 0.8})
        table.insert(sprites, reqSprite)
    end
    
    return sprites
end

function tower3DTrigger.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 16, entity.height or 16)
end

return tower3DTrigger