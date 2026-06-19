-- CS09_FakeSavePointTrigger.lua
-- Loenn trigger definition for the CS09 Fake Save Point cutscene trigger
-- Triggers the multi-stage cutscene sequence (A through E, then trap)

local cs09_fakesavepoint_trigger = {}

cs09_fakesavepoint_trigger.name = "DZ/CS09_FakeSavePointTrigger"

cs09_fakesavepoint_trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "auto"
        }
    },
    {
        name = "stage_a",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "stageA"
        }
    },
    {
        name = "stage_b",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "stageB"
        }
    },
    {
        name = "stage_c",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "stageC"
        }
    },
    {
        name = "stage_d",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "stageD"
        }
    },
    {
        name = "stage_e",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "stageE"
        }
    },
    {
        name = "pretrap",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "pretrap"
        }
    },
    {
        name = "trap",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "trap"
        }
    },
    {
        name = "madeline_freakout",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            specificStage = "madelineFreakout"
        }
    }
}

cs09_fakesavepoint_trigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean"
    },
    playerOnly = {
        fieldType = "boolean"
    },
    specificStage = {
        fieldType = "string",
        editable = true,
        options = {
            "auto",
            "stageA",
            "stageB",
            "stageC",
            "stageD",
            "stageE",
            "pretrap",
            "trap",
            "madelineFreakout"
        }
    }
}

-- Color-coded sprite based on stage
function cs09_fakesavepoint_trigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    -- Color based on stage
    local stageColors = {
        auto = {0.5, 0.5, 1.0, 0.7},           -- Blue for auto
        stageA = {0.3, 0.8, 0.3, 0.7},         -- Green for stage A
        stageB = {0.4, 0.9, 0.4, 0.7},         -- Light green for stage B
        stageC = {0.9, 0.9, 0.3, 0.7},         -- Yellow for stage C
        stageD = {0.9, 0.6, 0.2, 0.7},         -- Orange for stage D
        stageE = {0.9, 0.4, 0.2, 0.7},         -- Dark orange for stage E
        pretrap = {0.9, 0.3, 0.3, 0.7},        -- Red for pretrap
        trap = {0.6, 0.1, 0.1, 0.7},           -- Dark red for trap
        madelineFreakout = {0.8, 0.2, 0.8, 0.7} -- Purple for freakout
    }
    
    local stage = entity.specificStage or "auto"
    local color = stageColors[stage] or stageColors.auto
    
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

function cs09_fakesavepoint_trigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return cs09_fakesavepoint_trigger
