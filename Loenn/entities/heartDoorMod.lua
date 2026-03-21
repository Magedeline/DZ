-- Heart Door Mod entity for Lönn map editor
-- Standard door that requires a specific number of heart gems to open

local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local heartDoorMod = {}

heartDoorMod.name = "MaggyHelper/HeartDoorMod"
heartDoorMod.depth = 0
heartDoorMod.nodeLimits = {0, 1}
heartDoorMod.nodeLineRenderType = "line"

heartDoorMod.placements = {
    {
        name = "heart_door",
        data = {
            width = 40,
            requires = 0,
            startHidden = false
        }
    },
    {
        name = "heart_door_hidden",
        data = {
            width = 40,
            requires = 0,
            startHidden = true
        }
    }
}

heartDoorMod.fieldInformation = {
    width = {
        fieldType = "integer",
        minimumValue = 40
    },
    requires = {
        fieldType = "integer",
        minimumValue = 0
    },
    startHidden = {
        fieldType = "boolean"
    }
}

function heartDoorMod.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 40
    local requires = entity.requires or 0
    local startHidden = entity.startHidden or false
    
    local sprites = {}
    
    -- Draw the door body
    local doorColor = startHidden and {0.5, 0.7, 1.0, 0.6} or {0.1, 0.4, 0.56, 1.0}
    
    local doorRect = drawableRectangle.fromRectangle(
        "fill",
        x - width / 2, y - 32,
        width, 64,
        doorColor
    )
    table.insert(sprites, doorRect)
    
    -- Draw border
    local borderRect = drawableRectangle.fromRectangle(
        "line",
        x - width / 2, y - 32,
        width, 64,
        {1.0, 1.0, 1.0, 0.8}
    )
    table.insert(sprites, borderRect)
    
    -- Draw heart icon indicators
    if requires > 0 then
        local heartsToShow = math.min(requires, 5)
        for i = 1, heartsToShow do
            local heartSprite = drawableSprite.fromTexture("collectables/heartGem/0/00")
            heartSprite.x = x + (i - 3) * 8
            heartSprite.y = y
            heartSprite.scaleX = 0.5
            heartSprite.scaleY = 0.5
            heartSprite:setColor({1.0, 1.0, 1.0, startHidden and 0.5 or 1.0})
            table.insert(sprites, heartSprite)
        end
    end
    
    return sprites
end

function heartDoorMod.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 40
    
    return utils.rectangle(x - width / 2, y - 32, width, 64)
end

return heartDoorMod
