local utils = require("utils")

local SmallHeartGem = {}

SmallHeartGem.name = "MaggyHelper/SmallHeartGem"
SmallHeartGem.depth = -100
SmallHeartGem.justification = {0.5, 0.5}
SmallHeartGem.texture = "collectables/heartgem/0/00"

SmallHeartGem.placements = {
    {
        name = "small_heart_gem_ch10",
        data = {
            chapterNumber = 10,
            submapNumber = 1,
            gemId = ""
        }
    },
    {
        name = "small_heart_gem_ch11",
        data = {
            chapterNumber = 11,
            submapNumber = 1,
            gemId = ""
        }
    },
    {
        name = "small_heart_gem_ch12",
        data = {
            chapterNumber = 12,
            submapNumber = 1,
            gemId = ""
        }
    },
    {
        name = "small_heart_gem_ch13",
        data = {
            chapterNumber = 13,
            submapNumber = 1,
            gemId = ""
        }
    },
    {
        name = "small_heart_gem_ch14",
        data = {
            chapterNumber = 14,
            submapNumber = 1,
            gemId = ""
        }
    }
}

SmallHeartGem.fieldInformation = {
    chapterNumber = {
        fieldType = "integer",
        minimumValue = 10,
        maximumValue = 14,
        description = "Chapter number (10-14)"
    },
    submapNumber = {
        fieldType = "integer",
    minimumValue = 1,
        maximumValue = 6,
    description = "Submap number (1-6)"
    },
    gemId = {
        fieldType = "string",
        description = "Unique identifier for this gem (optional, auto-generated if empty)"
    }
}

-- Get chapter-specific color for texture
function SmallHeartGem.texture(room, entity)
    local chapter = entity.chapterNumber or 10
    
    -- Return different heart gem textures based on chapter
    local textures = {
        [10] = "collectables/heartgem/2/00",  -- Orange/red gem
        [11] = "collectables/heartgem/1/00",  -- Blue gem
        [12] = "collectables/heartgem/0/00",  -- Pink gem
        [13] = "collectables/heartgem/3/00",  -- Gold gem
        [14] = "collectables/heartgem/0/00"   -- Default gem
    }
    
    return textures[chapter] or "collectables/heartgem/0/00"
end

-- Visual selection rectangle
function SmallHeartGem.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

-- Draw gem with chapter-specific color and collection particles
function SmallHeartGem.draw(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local chapter = entity.chapterNumber or 10
    local submap = entity.submapNumber or 1
    
    -- Chapter color mapping
    local chapterColors = {
        [10] = {1.0, 0.5, 0.0},  -- Orange
        [11] = {0.5, 0.8, 1.0},  -- Light blue
        [12] = {0.8, 0.3, 0.8},  -- Purple
        [13] = {1.0, 0.8, 0.0},  -- Gold
        [14] = {0.7, 0.7, 0.7}   -- Silver
    }
    
    local color = chapterColors[chapter] or {1.0, 1.0, 1.0}
    
    -- Draw gem glow effect
    love.graphics.setColor(color[1], color[2], color[3], 0.3)
    love.graphics.circle("fill", x, y, 12)
    
    -- Draw gem body
    love.graphics.setColor(color[1], color[2], color[3], 0.8)
    love.graphics.circle("fill", x, y, 8)
    
    -- Draw gem outline
    love.graphics.setColor(color[1], color[2], color[3], 1.0)
    love.graphics.circle("line", x, y, 8)
    
    -- Draw inner sparkle
    love.graphics.setColor(1.0, 1.0, 1.0, 0.9)
    love.graphics.circle("fill", x - 2, y - 2, 2)
    
    -- Draw chapter and submap numbers
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
    love.graphics.print(tostring(chapter), x - 6, y + 12)
    love.graphics.print("." .. tostring(submap), x + 2, y + 12)
    
    -- Reset color
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
end

return SmallHeartGem