-- CS09_FakeSaveStageBTrigger.lua
-- Loenn trigger definition for CS09 Fake Save Point Stage B
-- Theo questions Chara about the location
-- Sets flag: ch9_fakesave_stage_b

local trigger = {}

trigger.name = "DZ/CS09_FakeSaveStageBTrigger"
trigger.depth = 2000

trigger.placements = {
    {
        name = "CS09_FakeSaveStageBTrigger",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            playerOnly = true
        }
    }
}

trigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean"
    },
    playerOnly = {
        fieldType = "boolean"
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "triggerOnce", "playerOnly"
}

function trigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    local color = {0.2, 0.6, 0.9, 0.7} -- Blue tint for stage B
    
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
