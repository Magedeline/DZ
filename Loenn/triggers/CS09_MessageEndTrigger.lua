-- CS09_MessageEndTrigger.lua
-- Loenn trigger definition for the Sans voice message cutscene
-- Standalone trigger to play the Chapter 9 ending message

local cs09_message_end_trigger = {}

cs09_message_end_trigger.name = "MaggyHelper/CS09_MessageEndTrigger"

cs09_message_end_trigger.placements = {
    {
        name = "CS09_MessageEndTrigger",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireFlag = false,
            requiredFlag = ""
        }
    },
    {
        name = "after_credits",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireFlag = true,
            requiredFlag = "ch9_credits_complete"
        }
    },
    {
        name = "after_trap",
        data = {
            width = 32,
            height = 32,
            triggerOnce = true,
            requireFlag = true,
            requiredFlag = "ch9_fake_save_trap_triggered"
        }
    }
}

cs09_message_end_trigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean"
    },
    requireFlag = {
        fieldType = "boolean"
    },
    requiredFlag = {
        fieldType = "string"
    }
}

cs09_message_end_trigger.fieldOrder = {
    "x", "y", "width", "height",
    "triggerOnce", "requireFlag", "requiredFlag"
}

-- Blue color for message trigger
function cs09_message_end_trigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    -- Blue tint for message cutscene
    local color = {0.2, 0.5, 0.9, 0.7}
    
    if entity.requireFlag then
        -- Add cyan tint if requires flag
        color = {0.2, 0.7, 0.9, 0.7}
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

function cs09_message_end_trigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return cs09_message_end_trigger
