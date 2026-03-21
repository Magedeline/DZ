-- Heart Staff Door entity for Lönn map editor
-- Large door that requires all 7 Heart Staffs to open
-- Opens to reveal the final chapter

local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local heartStaffDoor = {}

heartStaffDoor.name = "MaggyHelper/HeartStaffDoor"
heartStaffDoor.depth = 0
heartStaffDoor.nodeLimits = {0, 1}
heartStaffDoor.nodeLineRenderType = "line"
heartStaffDoor.minimumSize = {64, 8}
heartStaffDoor.canResize = {true, false}

-- Staff colors for icon display
local staffColors = {
    {1.0, 0.27, 0.27, 1.0},  -- Red
    {0.27, 0.53, 1.0, 1.0},  -- Blue
    {1.0, 0.87, 0.27, 1.0},  -- Yellow
    {0.27, 1.0, 0.53, 1.0},  -- Green
    {0.67, 0.27, 1.0, 1.0},  -- Purple
    {1.0, 0.53, 0.27, 1.0},  -- Orange
    {1.0, 0.53, 0.8, 1.0}    -- Pink
}

heartStaffDoor.placements = {
    {
        name = "heart_staff_door",
        data = {
            width = 80,
            requires = 7,
            startHidden = false,
            doorId = ""
        }
    },
    {
        name = "heart_staff_door_hidden",
        data = {
            width = 80,
            requires = 7,
            startHidden = true,
            doorId = ""
        }
    },
    {
        name = "heart_staff_door_partial",
        data = {
            width = 80,
            requires = 3,
            startHidden = false,
            doorId = ""
        }
    }
}

heartStaffDoor.fieldInformation = {
    width = {
        fieldType = "integer",
        minimumValue = 64,
        description = "Width of the door in pixels"
    },
    requires = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 7,
        description = "Number of Heart Staffs required to open (1-7)"
    },
    startHidden = {
        fieldType = "boolean",
        description = "Door starts hidden and appears when player approaches"
    },
    doorId = {
        fieldType = "string",
        description = "Unique identifier for this door (auto-generated if empty)"
    }
}

heartStaffDoor.fieldOrder = {
    "x", "y", "width", "requires", "startHidden", "doorId"
}

function heartStaffDoor.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 80
    local requires = entity.requires or 7
    local startHidden = entity.startHidden or false
    
    local sprites = {}
    
    -- Door body color
    local doorColor = startHidden 
        and {0.4, 0.5, 0.9, 0.5} 
        or {0.1, 0.15, 0.4, 0.9}
    
    -- Draw top door section
    local topRect = drawableRectangle.fromRectangle(
        "fill",
        x, y - 96,
        width, 96,
        doorColor
    )
    table.insert(sprites, topRect)
    
    -- Draw bottom door section
    local bottomRect = drawableRectangle.fromRectangle(
        "fill",
        x, y,
        width, 96,
        doorColor
    )
    table.insert(sprites, bottomRect)
    
    -- Draw door borders
    local borderColor = {0.8, 0.85, 1.0, 0.9}
    
    -- Top border
    local topBorder = drawableRectangle.fromRectangle(
        "line",
        x, y - 96,
        width, 96,
        borderColor
    )
    table.insert(sprites, topBorder)
    
    -- Bottom border
    local bottomBorder = drawableRectangle.fromRectangle(
        "line",
        x, y,
        width, 96,
        borderColor
    )
    table.insert(sprites, bottomBorder)
    
    -- Draw center seam (where door opens)
    local seamLine = drawableLine.fromPoints(
        {x, y, x + width, y},
        {1.0, 0.9, 0.5, 0.8},
        2
    )
    table.insert(sprites, seamLine)
    
    -- Draw staff icons in a circle pattern
    local centerX = x + width / 2
    local centerY = y
    local iconRadius = math.min(width / 3, 35)
    
    for i = 1, 7 do
        local angle = ((i - 1) / 7) * math.pi * 2 - math.pi / 2
        local iconX = centerX + math.cos(angle) * iconRadius
        local iconY = centerY + math.sin(angle) * iconRadius
        
        local staffColor = staffColors[i]
        local isRequired = i <= requires
        
        -- Draw staff icon circle
        local iconAlpha = isRequired and 1.0 or 0.3
        local iconColor = {
            staffColor[1], 
            staffColor[2], 
            staffColor[3], 
            iconAlpha * (startHidden and 0.5 or 1.0)
        }
        
        -- Try to draw heart icon, fallback to circle
        local heartSprite = drawableSprite.fromTexture("collectables/heartGem/0/00")
        if heartSprite then
            heartSprite.x = iconX
            heartSprite.y = iconY
            heartSprite.scaleX = 0.5
            heartSprite.scaleY = 0.5
            heartSprite:setColor(iconColor)
            heartSprite:setJustification(0.5, 0.5)
            table.insert(sprites, heartSprite)
        end
    end
    
    -- Draw requirement counter
    local counterText = tostring(requires) .. "/7"
    -- Note: Text rendering is limited in Lönn sprites, 
    -- the counter will be shown in-game
    
    return sprites
end

function heartStaffDoor.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local width = entity.width or 80
    
    -- Selection covers both door halves
    return utils.rectangle(x, y - 96, width, 192)
end

-- Node selection (for open distance)
function heartStaffDoor.nodeRectangle(room, entity, node)
    return utils.rectangle(node.x - 4, node.y - 4, 8, 8)
end

-- Tooltip
function heartStaffDoor.tooltip(room, entity)
    local requires = entity.requires or 7
    local hidden = entity.startHidden and " (Hidden)" or ""
    return "Heart Staff Door - Requires " .. requires .. " staffs" .. hidden
end

return heartStaffDoor
