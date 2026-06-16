-- Large Heart Door Mod entity for L�nn map editor
-- Larger door that requires more heart gems and plays a cutscene when opening

local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local largeHeartDoorMod = {}

largeHeartDoorMod.name = "DZ/LargeHeartDoorMod"
largeHeartDoorMod.depth = 0
largeHeartDoorMod.nodeLimits = {0, 1}
largeHeartDoorMod.nodeLineRenderType = "line"

largeHeartDoorMod.placements = {
    {
        name = "large_heart_door",
        data = {
            width = 64,
            requires = 0,
            startHidden = false
        }
    },
    {
        name = "large_heart_door_hidden",
        data = {
            width = 64,
            requires = 0,
            startHidden = true
        }
    }
}

largeHeartDoorMod.fieldInformation = {
    width = {
        fieldType = "integer",
        minimumValue = 64
    },
    requires = {
        fieldType = "integer",
        minimumValue = 0
    },
    startHidden = {
        fieldType = "boolean"
    }
}

function largeHeartDoorMod.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 64
    local requires = entity.requires or 0
    local startHidden = entity.startHidden or false
    
    local sprites = {}
    
    -- Draw the large door body (twice as tall)
    local doorColor = startHidden and {0.5, 0.7, 1.0, 0.6} or {0.1, 0.4, 0.56, 1.0}
    
    local doorRect = drawableRectangle.fromRectangle(
        "fill",
        x - width / 2, y - 64,
        width, 128,
        doorColor
    )
    table.insert(sprites, doorRect)
    
    -- Draw border
    local borderRect = drawableRectangle.fromRectangle(
        "line",
        x - width / 2, y - 64,
        width, 128,
        {1.0, 1.0, 1.0, 0.8}
    )
    table.insert(sprites, borderRect)
    
    -- Draw heart icon indicators (larger door, more hearts visible)
    if requires > 0 then
        local heartsToShow = math.min(requires, 8)
        for i = 1, heartsToShow do
            local row = math.floor((i - 1) / 4)
            local col = ((i - 1) % 4)
            
            local heartSprite = drawableSprite.fromTexture("collectables/heartGem/0/00")
            heartSprite.x = x + (col - 1.5) * 10
            heartSprite.y = y + (row - 0.5) * 10
            heartSprite.scaleX = 0.6
            heartSprite.scaleY = 0.6
            heartSprite:setColor({1.0, 1.0, 1.0, startHidden and 0.5 or 1.0})
            table.insert(sprites, heartSprite)
        end
    end
    
    return sprites
end

function largeHeartDoorMod.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 64
    
    return utils.rectangle(x - width / 2, y - 64, width, 128)
end

function largeHeartDoorMod.rectangle(room, entity)
    local width = entity.width or 64
    return {-width / 2, -64, width, 128}
end

return largeHeartDoorMod
