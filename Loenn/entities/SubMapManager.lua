local utils = require("utils")

local SubMapManager = {}

SubMapManager.name = "MaggyHelper/SubMapManager"
SubMapManager.depth = -1000000
SubMapManager.justification = {0.5, 0.5}
SubMapManager.texture = "objects/IngesteHelper/submap_manager"

SubMapManager.placements = {
    {
        name = "submap_manager",
        data = {
            autoInitialize = true,
            debugMode = false
        }
    }
}

SubMapManager.fieldInformation = {
    autoInitialize = {
        fieldType = "boolean",
        description = "Whether to automatically initialize submaps on level load"
    },
    debugMode = {
        fieldType = "boolean", 
        description = "Enable debug logging for submap operations"
    }
}

-- Visual selection rectangle
function SubMapManager.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

-- Draw manager with distinctive icon
function SubMapManager.draw(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    
    -- Draw manager icon (gear-like shape)
    love.graphics.setColor(0.2, 0.7, 1.0, 0.8)
    love.graphics.circle("fill", x, y, 12)
    
    love.graphics.setColor(0.2, 0.7, 1.0, 1.0)
    love.graphics.circle("line", x, y, 12)
    
    -- Draw gear teeth
    for i = 0, 7 do
        local angle = i * (math.pi / 4)
        local outerX = x + math.cos(angle) * 12
        local outerY = y + math.sin(angle) * 12
        local innerX = x + math.cos(angle) * 8
        local innerY = y + math.sin(angle) * 8
        
        love.graphics.line(innerX, innerY, outerX, outerY)
    end
    
    -- Draw center
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
    love.graphics.circle("fill", x, y, 4)
    love.graphics.setColor(0.2, 0.7, 1.0, 1.0)
    love.graphics.circle("line", x, y, 4)
    
    -- Reset color
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
end

return SubMapManager