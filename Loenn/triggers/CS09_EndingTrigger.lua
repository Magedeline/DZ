-- CS09_EndingTrigger.lua
-- Loenn trigger definition for the Chapter 9 ending sequence
-- Chains: Credits -> Sans Message -> Area Complete

local cs09_ending_trigger = {}

cs09_ending_trigger.name = "MaggyHelper/CS09_EndingTrigger"

cs09_ending_trigger.placements = {
    {
        name = "full_ending",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireTrapComplete = true,
            skipCredits = false,
            skipMessage = false,
            nextLevel = ""
        }
    },
    {
        name = "credits_only",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireTrapComplete = false,
            skipCredits = false,
            skipMessage = true,
            nextLevel = ""
        }
    },
    {
        name = "message_only",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireTrapComplete = false,
            skipCredits = true,
            skipMessage = false,
            nextLevel = ""
        }
    },
    {
        name = "area_complete_only",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireTrapComplete = false,
            skipCredits = true,
            skipMessage = true,
            nextLevel = ""
        }
    },
    {
        name = "after_trap_full",
        data = {
            width = 32,
            height = 32,
            triggerOnce = true,
            requireTrapComplete = true,
            skipCredits = false,
            skipMessage = false,
            nextLevel = ""
        }
    }
}

cs09_ending_trigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean"
    },
    requireTrapComplete = {
        fieldType = "boolean"
    },
    skipCredits = {
        fieldType = "boolean"
    },
    skipMessage = {
        fieldType = "boolean"
    },
    nextLevel = {
        fieldType = "string"
    }
}

-- Color-coded sprite based on configuration
function cs09_ending_trigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    -- Color based on configuration
    local color = {0.9, 0.7, 0.2, 0.7} -- Gold for full ending
    
    if entity.skipCredits and entity.skipMessage then
        color = {0.2, 0.9, 0.2, 0.7} -- Green for area complete only
    elseif entity.skipCredits then
        color = {0.2, 0.5, 0.9, 0.7} -- Blue for message + complete
    elseif entity.skipMessage then
        color = {0.9, 0.5, 0.2, 0.7} -- Orange for credits + complete
    end
    
    if entity.requireTrapComplete then
        -- Add red tint if requires trap
        color[1] = math.min(1.0, color[1] + 0.2)
    end
    
    return {
        {
            texture = "ahorn/entityTrigger",
            x = entity.x,
            y = entity.y,
            scaleX = width / 8,
            scaleY = height / 8,
            color = color
        }
    }
end

function cs09_ending_trigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return cs09_ending_trigger
