-- CS09_UntilNextTimeCreditTrigger.lua
-- Loenn trigger definition for the Chapter 9 credits cutscene
-- "Until Next Time..." credits sequence with helper mod acknowledgments

local cs09_credit_trigger = {}

cs09_credit_trigger.name = "DZ/CS09_UntilNextTimeCreditTrigger"

cs09_credit_trigger.placements = {
    {
        name = "CS09_UntilNextTimeCreditTrigger",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireFlag = false,
            requiredFlag = ""
        }
    },
    {
        name = "after_trap",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            requireFlag = true,
            requiredFlag = "ch9_fake_save_trap_triggered"
        }
    },
    {
        name = "end_game",
        data = {
            width = 32,
            height = 32,
            triggerOnce = true,
            requireFlag = false,
            requiredFlag = ""
        }
    }
}

cs09_credit_trigger.fieldInformation = {
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

cs09_credit_trigger.fieldOrder = {
    "x", "y", "width", "height",
    "triggerOnce", "requireFlag", "requiredFlag"
}

-- Gold color for credits trigger (matching the ending)
function cs09_credit_trigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    -- Gold tint for credits sequence
    local color = {0.9, 0.7, 0.2, 0.7}
    
    if entity.requireFlag then
        -- Orange tint if requires flag
        color = {0.9, 0.5, 0.2, 0.7}
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

function cs09_credit_trigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return cs09_credit_trigger
