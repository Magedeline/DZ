local utils = require("utils")

local SubMapLobby = {}

SubMapLobby.name = "MaggyHelper/SubMapLobby"
SubMapLobby.depth = -100
SubMapLobby.justification = {0.5, 1.0}
SubMapLobby.texture = "objects/IngesteHelper/submap_lobby"

SubMapLobby.placements = {
    {
        name = "submap_lobby_ch10",
        data = {
            chapterNumber = 10
        }
    },
    {
        name = "submap_lobby_ch11", 
        data = {
            chapterNumber = 11
        }
    },
    {
        name = "submap_lobby_ch12",
        data = {
            chapterNumber = 12
        }
    },
    {
        name = "submap_lobby_ch13",
        data = {
            chapterNumber = 13
        }
    },
    {
        name = "submap_lobby_ch14",
        data = {
            chapterNumber = 14
        }
    }
}

SubMapLobby.fieldInformation = {
    chapterNumber = {
        fieldType = "integer",
        minimumValue = 10,
        maximumValue = 14,
        description = "Chapter number for the submap lobby (10-14)"
    }
}

-- Visual selection rectangle
function SubMapLobby.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 16, 32, 32)
end

-- Draw lobby with chapter-specific color and portal indicators
function SubMapLobby.draw(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local chapter = entity.chapterNumber or 10
    
    -- Chapter color mapping
    local chapterColors = {
        [10] = {1.0, 0.5, 0.0, 0.8},  -- Orange for ruins
        [11] = {0.5, 0.8, 1.0, 0.8},  -- Light blue for snowdin
        [12] = {0.8, 0.3, 0.8, 0.8},  -- Purple for wateredge
        [13] = {1.0, 0.8, 0.0, 0.8},  -- Gold for next chapter
        [14] = {0.7, 0.7, 0.7, 0.8}   -- Silver for final chapter
    }
    
    local color = chapterColors[chapter] or {1.0, 1.0, 1.0, 0.8}
    
    -- Draw main lobby circle
    love.graphics.setColor(color[1], color[2], color[3], color[4] * 0.3)
    love.graphics.circle("fill", x, y, 32)
    
    -- Draw lobby outline
    love.graphics.setColor(color[1], color[2], color[3], color[4])
    love.graphics.circle("line", x, y, 32)
    love.graphics.setLineWidth(2)
    
    -- Draw portal positions for submaps 1-6
    for i = 1, 6 do
        -- Spread evenly in a circle
        local angle = ((i - 1) / 6) * (2 * math.pi)
        local portalX = x + math.cos(angle) * 24
        local portalY = y + math.sin(angle) * 24
        
        -- Draw portal indicators
        love.graphics.setColor(color[1], color[2], color[3], color[4] * 0.6)
        love.graphics.circle("fill", portalX, portalY, 6)
        love.graphics.setColor(color[1], color[2], color[3], color[4])
        love.graphics.circle("line", portalX, portalY, 6)
        
        -- Draw submap number
        love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
    love.graphics.print(tostring(i), portalX - 3, portalY - 6)
    end
    
    -- Draw chapter number in center
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
    love.graphics.print("CH" .. tostring(chapter), x - 12, y - 6)
    
    -- Reset line width and color
    love.graphics.setLineWidth(1)
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
end

return SubMapLobby