-- Loenn plugin for DZ - Kirby Knight Trigger
-- Enables/disables Knight mode or sets chapter-specific flags (Chapter 19/20)

local trigger = {}

trigger.name = "DZ/KirbyKnightTrigger"
trigger.depth = 2000

trigger.fieldInformation = {
    mode = {
        options = {
            "Enable",       -- Enable knight mode
            "Disable",      -- Disable knight mode
            "Toggle",       -- Toggle knight mode
            "SetFinalRun",  -- Set Chapter 19 final run flag
            "SetLastPush",  -- Set Chapter 20 last push flag
            "Unlock"        -- Just unlock knight (don't transform)
        },
        editable = false
    },
    autoTransform = {
        fieldType = "boolean"
    },
    playEffects = {
        fieldType = "boolean"
    },
    requiredFlag = {
        fieldType = "string"
    },
    onlyOnce = {
        fieldType = "boolean"
    },
    transformDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "mode",
    "autoTransform",
    "playEffects",
    "requiredFlag",
    "onlyOnce",
    "transformDelay"
}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            mode = "Enable",
            autoTransform = false,
            playEffects = true,
            requiredFlag = "",
            onlyOnce = true,
            transformDelay = 0.0
        }
    },
    {
        name = "knight_disable",
        data = {
            width = 16,
            height = 16,
            mode = "Disable",
            autoTransform = false,
            playEffects = true,
            requiredFlag = "",
            onlyOnce = true,
            transformDelay = 0.0
        }
    },
    {
        name = "knight_toggle",
        data = {
            width = 16,
            height = 16,
            mode = "Toggle",
            autoTransform = false,
            playEffects = true,
            requiredFlag = "",
            onlyOnce = false,
            transformDelay = 0.0
        }
    },
    {
        name = "knight_final_run",
        data = {
            width = 16,
            height = 16,
            mode = "SetFinalRun",
            autoTransform = true,
            playEffects = true,
            requiredFlag = "",
            onlyOnce = true,
            transformDelay = 0.0
        }
    },
    {
        name = "knight_last_push",
        data = {
            width = 16,
            height = 16,
            mode = "SetLastPush",
            autoTransform = true,
            playEffects = true,
            requiredFlag = "",
            onlyOnce = true,
            transformDelay = 0.0
        }
    },
    {
        name = "knight_unlock",
        data = {
            width = 16,
            height = 16,
            mode = "Unlock",
            autoTransform = false,
            playEffects = true,
            requiredFlag = "",
            onlyOnce = true,
            transformDelay = 0.0
        }
    },
    {
        name = "knight_delayed_enable",
        data = {
            width = 16,
            height = 16,
            mode = "Enable",
            autoTransform = true,
            playEffects = true,
            requiredFlag = "",
            onlyOnce = true,
            transformDelay = 1.0
        }
    }
}

-- Color based on mode for visual distinction in editor
function trigger.color(room, entity)
    local mode = entity.mode or "Enable"

    if mode == "Enable" then
        return {1.0, 0.84, 0.0, 0.6}  -- Gold
    elseif mode == "Disable" then
        return {0.5, 0.5, 0.5, 0.6}   -- Gray
    elseif mode == "Toggle" then
        return {0.8, 0.6, 0.0, 0.6}   -- Orange
    elseif mode == "SetFinalRun" then
        return {1.0, 0.5, 0.0, 0.6}   -- Orange-red (Chapter 19)
    elseif mode == "SetLastPush" then
        return {1.0, 0.0, 0.0, 0.6}   -- Red (Chapter 20)
    elseif mode == "Unlock" then
        return {0.8, 0.8, 0.0, 0.6}   -- Yellow
    end

    return {1.0, 0.84, 0.0, 0.6}  -- Default gold
end

return trigger
