-- Example Audio Trigger Entity for DesoloZantas
-- Demonstrates how to use the audio_library.lua for organized sound management

local audioLib = require("libraries.audio_library")
local selectionBounds = require("libraries.selection_bounds")

local audioTrigger = {}

local defaultBounds = {
    width = 16,
    height = 16
}

audioTrigger.name = "DesoloZantas/AudioTrigger"
audioTrigger.depth = 0
audioTrigger.placements = {
    {
        name = "Audio Trigger",
        data = {
            width = 16,
            height = 16,
            audioCategory = "sfx",
            audioSubcategory = "dialogue",
            audioItem = "kirby",
            oneUse = false,
            persistent = false,
            eventName = "",
            customGuid = ""
        }
    }
}

-- Audio category options for the dropdown
local audioCategories = {
    "music",
    "ambience", 
    "sfx"
}

-- Dynamic subcategory options based on selected category
local function getSubcategories(category)
    if category == "music" then
        return {"cassettes", "classic"}
    elseif category == "ambience" then
        return {"chapters", "local_environments"}
    elseif category == "sfx" then
        return {"dialogue", "movement", "mechanics", "classic"}
    end
    return {}
end

-- Dynamic item options based on selected category and subcategory
local function getItems(category, subcategory)
    local categoryData = audioLib[category]
    if not categoryData or not categoryData[subcategory] then
        return {}
    end
    
    local items = {}
    for key, _ in pairs(categoryData[subcategory]) do
        table.insert(items, key)
    end
    return items
end

audioTrigger.fieldInformation = {
    audioCategory = {
        options = audioCategories,
        editable = false
    },
    audioSubcategory = {
        options = function(room, entity)
            return getSubcategories(entity.audioCategory or "sfx")
        end,
        editable = false
    },
    audioItem = {
        options = function(room, entity)
            return getItems(entity.audioCategory or "sfx", entity.audioSubcategory or "dialogue")
        end,
        editable = false
    },
    eventName = {
        description = "Custom event name (overrides category selection)"
    },
    customGuid = {
        description = "Custom GUID (overrides category selection)"
    }
}

audioTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "audioCategory", "audioSubcategory", "audioItem",
    "eventName", "customGuid",
    "oneUse", "persistent"
}

function audioTrigger.rectangle(room, entity)
    return selectionBounds.rectangle(entity, defaultBounds)
end

function audioTrigger.selection(room, entity)
    return selectionBounds.rectangle(entity, defaultBounds)
end

-- Generate tooltip showing the actual audio path and GUID
function audioTrigger.tooltip(room, entity)
    local tooltip = "Audio Trigger\n"
    
    if entity.eventName and entity.eventName ~= "" then
        tooltip = tooltip .. "Custom Event: " .. entity.eventName .. "\n"
    elseif entity.customGuid and entity.customGuid ~= "" then
        tooltip = tooltip .. "Custom GUID: " .. entity.customGuid .. "\n"
    else
        local audioEvent = audioLib.getAudioEvent(
            entity.audioCategory or "sfx",
            entity.audioSubcategory or "dialogue", 
            entity.audioItem or "kirby"
        )
        
        if audioEvent then
            tooltip = tooltip .. "Event: " .. audioEvent.path .. "\n"
            tooltip = tooltip .. "GUID: " .. audioEvent.guid .. "\n"
        else
            tooltip = tooltip .. "Invalid audio selection\n"
        end
    end
    
    if entity.oneUse then
        tooltip = tooltip .. "One-time use"
    else
        tooltip = tooltip .. "Reusable"
    end
    
    return tooltip
end

function audioTrigger.draw(room, entity)
    local bounds = selectionBounds.resolve(entity, defaultBounds)
    local x, y = bounds.x, bounds.y
    local width, height = bounds.width, bounds.height
    
    -- Draw trigger area
    love.graphics.setColor(0.2, 0.6, 1.0, 0.4)
    love.graphics.rectangle("fill", x, y, width, height)
    
    -- Draw border
    love.graphics.setColor(0.2, 0.6, 1.0, 0.8)
    love.graphics.rectangle("line", x, y, width, height)
    
    -- Draw speaker icon in center
    local centerX = x + width / 2
    local centerY = y + height / 2
    
    love.graphics.setColor(1.0, 1.0, 1.0, 0.9)
    
    -- Simple speaker icon
    love.graphics.rectangle("fill", centerX - 3, centerY - 2, 2, 4)
    love.graphics.polygon("fill", 
        centerX - 1, centerY - 2,
        centerX + 2, centerY - 4,
        centerX + 2, centerY + 4,
        centerX - 1, centerY + 2
    )
    
    -- Sound waves
    love.graphics.arc("line", centerX + 3, centerY, 3, -math.pi/4, math.pi/4)
    love.graphics.arc("line", centerX + 3, centerY, 5, -math.pi/6, math.pi/6)
    
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
end

return audioTrigger