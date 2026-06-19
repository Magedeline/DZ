-- Heart Staff collectible entity for Lönn map editor
-- Inspired by Kirby Star Allies Friend Hearts
-- Collect all 7 colored staffs to unlock the final chapter door

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local heartStaff = {}

heartStaff.name = "DZ/HeartStaff"
heartStaff.depth = -100
heartStaff.justification = {0.5, 0.5}

-- Staff colors matching Kirby Star Allies Friend Hearts
local staffColors = {
    "red",      -- Fire/Power (Blazing Heart Staff)
    "blue",     -- Water/Ice (Glacial Heart Staff)
    "yellow",   -- Spark/Electric (Spark Heart Staff)
    "green",    -- Leaf/Nature (Nature Heart Staff)
    "purple",   -- Poison/Dark (Void Heart Staff)
    "orange",   -- Beam/Light (Radiant Heart Staff)
    "pink"      -- Heart/Love (Love Heart Staff - Final)
}

-- Color values for visual display
local colorValues = {
    red = {1.0, 0.27, 0.27, 1.0},
    blue = {0.27, 0.53, 1.0, 1.0},
    yellow = {1.0, 0.87, 0.27, 1.0},
    green = {0.27, 1.0, 0.53, 1.0},
    purple = {0.67, 0.27, 1.0, 1.0},
    orange = {1.0, 0.53, 0.27, 1.0},
    pink = {1.0, 0.53, 0.8, 1.0}
}

-- Staff names for tooltips
local staffNames = {
    red = "Blazing Heart Staff",
    blue = "Glacial Heart Staff",
    yellow = "Spark Heart Staff",
    green = "Nature Heart Staff",
    purple = "Void Heart Staff",
    orange = "Radiant Heart Staff",
    pink = "Love Heart Staff"
}

heartStaff.placements = {
    {
        name = "main",
        data = {
            staffColor = "red",
            staffId = ""
        }
    },
    {
        name = "blue",
        data = {
            staffColor = "blue",
            staffId = ""
        }
    },
    {
        name = "yellow",
        data = {
            staffColor = "yellow",
            staffId = ""
        }
    },
    {
        name = "green",
        data = {
            staffColor = "green",
            staffId = ""
        }
    },
    {
        name = "purple",
        data = {
            staffColor = "purple",
            staffId = ""
        }
    },
    {
        name = "orange",
        data = {
            staffColor = "orange",
            staffId = ""
        }
    },
    {
        name = "pink",
        data = {
            staffColor = "pink",
            staffId = ""
        }
    }
}

heartStaff.fieldInformation = {
    staffColor = {
        options = staffColors,
        editable = false,
        fieldType = "string"
    },
    staffId = {
        fieldType = "string",
        description = "Unique identifier for this staff (auto-generated if empty)"
    }
}

heartStaff.fieldOrder = {
    "x", "y", "staffColor", "staffId"
}

-- Get texture path based on staff color
function heartStaff.texture(room, entity)
    local color = entity.staffColor or "red"
    
    -- Try custom heart staff textures first
    local customPath = "collectables/heartstaff/" .. color .. "/00"
    
    -- Fallback to heart gem textures with color tinting
    return "collectables/heartGem/0/00"
end

-- Draw the staff with proper color tinting
function heartStaff.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local color = entity.staffColor or "red"
    local colorValue = colorValues[color] or colorValues.red
    
    local sprites = {}
    
    -- Try to use custom staff sprite, fallback to heart gem
    local staffSprite = drawableSprite.fromTexture("collectables/heartstaff/" .. color .. "/00", entity)
    
    if not staffSprite then
        -- Fallback to heart gem texture
        staffSprite = drawableSprite.fromTexture("collectables/heartGem/0/00", entity)
    end
    
    if staffSprite then
        staffSprite.x = x
        staffSprite.y = y
        staffSprite:setColor(colorValue)
        staffSprite:setJustification(0.5, 0.5)
        table.insert(sprites, staffSprite)
    end
    
    -- Add glow circle behind
    -- Draw a subtle glow indicator
    local glowColor = {colorValue[1], colorValue[2], colorValue[3], 0.3}
    
    return sprites
end

-- Selection rectangle
function heartStaff.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 16, 20, 32)
end

-- Tooltip showing staff name
function heartStaff.tooltip(room, entity)
    local color = entity.staffColor or "red"
    local name = staffNames[color] or "Heart Staff"
    return name
end

return heartStaff
