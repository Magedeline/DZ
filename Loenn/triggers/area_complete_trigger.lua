local areaCompleteTrigger = {}

areaCompleteTrigger.name = "MaggyHelper/AreaCompleteTrigger"

areaCompleteTrigger.placements = {
    {
        name = "AreaCompleteTrigger",
        data = {
            width = 16,
            height = 16,
            nextLevel = "",
            hasGoldenStrawberry = false,
            hasPinkPlatinumBerry = false,
            skipCredits = false,
            triggerOnce = true
        }
    },
    {
        name = "with_next_level",
        data = {
            width = 32,
            height = 16,
            nextLevel = "lvl_chapter20_intro",
            hasGoldenStrawberry = false,
            hasPinkPlatinumBerry = false,
            skipCredits = false,
            triggerOnce = true
        }
    },
    {
        name = "golden_ending",
        data = {
            width = 32,
            height = 16,
            nextLevel = "",
            hasGoldenStrawberry = true,
            hasPinkPlatinumBerry = false,
            skipCredits = false,
            triggerOnce = true
        }
    },
    {
        name = "pink_platinum_ending",
        data = {
            width = 32,
            height = 16,
            nextLevel = "",
            hasGoldenStrawberry = false,
            hasPinkPlatinumBerry = true,
            skipCredits = false,
            triggerOnce = true
        }
    },
    {
        name = "quick_transition",
        data = {
            width = 16,
            height = 16,
            nextLevel = "lvl_epilogue",
            hasGoldenStrawberry = false,
            hasPinkPlatinumBerry = false,
            skipCredits = true,
            triggerOnce = true
        }
    }
}

areaCompleteTrigger.fieldInformation = {
    nextLevel = {
        fieldType = "string",
        options = {
            "",
            "lvl_chapter20_intro",
            "lvl_chapter20_start",
            "lvl_epilogue",
            "lvl_end-greengreens",
            "lvl_credits",
            "lvl_outro"
        },
        editable = true
    },
    hasGoldenStrawberry = {
        fieldType = "boolean"
    },
    hasPinkPlatinumBerry = {
        fieldType = "boolean"
    },
    skipCredits = {
        fieldType = "boolean"
    },
    triggerOnce = {
        fieldType = "boolean"
    }
}

areaCompleteTrigger.fieldOrder = {
    "x", "y", "width", "height",
    "nextLevel", "hasGoldenStrawberry", "hasPinkPlatinumBerry", "skipCredits", "triggerOnce"
}

function areaCompleteTrigger.texture(room, entity)
    return "ahorn/entityTrigger"
end

function areaCompleteTrigger.color(room, entity)
    -- White color for area complete (matching the fade effect)
    if entity.hasGoldenStrawberry then
        -- Golden color for golden strawberry ending
        return {1.0, 0.85, 0.0, 0.8}
    elseif entity.hasPinkPlatinumBerry then
        -- Pink/magenta color for pink platinum berry ending
        return {1.0, 0.4, 0.8, 0.8}
    elseif entity.nextLevel ~= "" then
        -- Light blue for transition to next level
        return {0.7, 0.9, 1.0, 0.8}
    else
        -- White/light gray for normal completion
        return {0.95, 0.95, 0.95, 0.8}
    end
end

function areaCompleteTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    return {
        {
            texture = "ahorn/entityTrigger",
            x = 0,
            y = 0,
            scaleX = width / 8,
            scaleY = height / 8,
            color = areaCompleteTrigger.color(room, entity)
        }
    }
end

return areaCompleteTrigger
