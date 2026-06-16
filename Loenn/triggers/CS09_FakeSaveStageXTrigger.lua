-- CS09_FakeSaveStageXTrigger.lua
-- Generic stage trigger for CS09 Fake Save Point sequence.

local trigger = {}

trigger.name = "DZ/CS09_FakeSaveStageXTrigger"
trigger.depth = 2000

trigger.placements = {
    {
        name = "CS09_FakeSaveStageXTrigger",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true,
            stage = "stageA"
        }
    }
}

trigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean"
    },
    playerOnly = {
        fieldType = "boolean"
    },
    stage = {
        fieldType = "string",
        editable = true,
        options = {
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

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "triggerOnce", "playerOnly", "stage"
}

function trigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    local stageColors = {
        stageA = {0.3, 0.8, 0.3, 0.7},
        stageB = {0.2, 0.6, 0.9, 0.7},
        stageC = {0.6, 0.2, 0.9, 0.7},
        stageD = {0.2, 0.9, 0.4, 0.7},
        stageE = {0.9, 0.2, 0.2, 0.7},
        pretrap = {0.9, 0.5, 0.2, 0.7},
        trap = {0.6, 0.1, 0.1, 0.7},
        madelineFreakout = {0.8, 0.2, 0.8, 0.7}
    }

    local stage = entity.stage or "stageA"
    local color = stageColors[stage] or stageColors.stageA

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

function trigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return trigger