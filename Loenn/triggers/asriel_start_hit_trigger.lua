-- Asriel Start Hit Trigger
-- Triggers the AsrielGodBoss to start attacking when the player enters the trigger.
-- Optionally moves Asriel to a target position defined by the first node.

local asrielStartHitTrigger = {}

asrielStartHitTrigger.name = "MaggyHelper/AsrielStartHitTrigger"

asrielStartHitTrigger.nodeLimits = {0, 1}
asrielStartHitTrigger.nodeLineRenderType = "line"

asrielStartHitTrigger.placements = {
    {
        name = "AsrielStartHitTrigger",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            moveAsriel = false,
            moveSpeed = 300.0
        }
    },
    {
        name = "with_movement",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            moveAsriel = true,
            moveSpeed = 300.0
        }
    }
}

asrielStartHitTrigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean"
    },
    moveAsriel = {
        fieldType = "boolean"
    },
    moveSpeed = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 1000.0
    }
}

-- Custom rendering for the trigger (purple/pink color scheme to match Asriel theme)
function asrielStartHitTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    -- Purple color for Asriel-related triggers
    local color = {0.7, 0.3, 0.9, 0.6}
    
    if entity.moveAsriel then
        -- Lighter purple when movement is enabled
        color = {0.9, 0.5, 1.0, 0.6}
    end
    
    return {
        {
            x = 0,
            y = 0,
            width = width,
            height = height,
            color = color
        }
    }
end

return asrielStartHitTrigger
