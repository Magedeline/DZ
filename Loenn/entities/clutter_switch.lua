local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local clutterSwitch = {}

local variants = {"ClutterSwitch red", "ClutterSwitch green", "ClutterSwitch blue", "ClutterSwitch yellow"}
local variantOptions = {
    Red = "ClutterSwitch red",
    Green = "ClutterSwitch green", 
    Blue = "ClutterSwitch blue",
    Lightning = "ClutterSwitch yellow"
}

clutterSwitch.name = "MaggyHelper/ClutterSwitch"
clutterSwitch.depth = 0
clutterSwitch.fillColor = {0.4, 0.4, 0.4, 0.8}
clutterSwitch.borderColor = {1.0, 1.0, 1.0, 1.0}
clutterSwitch.fieldInformation = {
    type = {
        options = variantOptions,
        editable = false
    },
    lightingAlphaAdd = {
        minimumValue = 0.0
    }
}

-- Create placements for each variant
clutterSwitch.placements = {}
for i, variant in ipairs(variants) do
    clutterSwitch.placements[i] = {
        name = variant,
        data = {
            type = variant,
            musicEvent = "guid://{d49a04ce-06fb-43bb-8880-1b95a4f6544f}",
            absorbCutsceneSound = "guid://{ab48ef65-2a19-4e26-bd96-c91188020dd6}",
            progressMusic = true,
            lightingAlphaAdd = 0.05
        }
    }
end

function clutterSwitch.sprite(room, entity)
    local variant = entity.type or "ClutterSwitch red"
    local width, height = 32, 16
    
    -- Create the switch base
    local rectangle = utils.rectangle(entity.x, entity.y, width, height)
    local drawableRectangleSprites = drawableRectangle.fromRectangle("bordered", rectangle, clutterSwitch.fillColor, clutterSwitch.borderColor):getDrawableSprite()
    
    -- Add the switch icon based on variant
    local iconTexture = string.format("objects/resortclutter/icon_%s", string.lower(variant))
    local iconSprite = drawableSprite.fromTexture(iconTexture, entity)
    iconSprite:addPosition(16, 8) -- Center the icon
    
    -- Add a switch indicator
    local switchTexture = "objects/resortclutter/switch"
    local switchSprite = drawableSprite.fromTexture(switchTexture, entity)
    switchSprite:addPosition(16, 16) -- Position at bottom center
    
    table.insert(drawableRectangleSprites, iconSprite)
    table.insert(drawableRectangleSprites, switchSprite)
    
    return drawableRectangleSprites
end

function clutterSwitch.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 32, 16)
end

return clutterSwitch
