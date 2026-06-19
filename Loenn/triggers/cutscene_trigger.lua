local cutsceneTrigger = {}

cutsceneTrigger.name = "DZ/CutsceneTrigger"
cutsceneTrigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            cutsceneId = "ingeste_intro",
            triggerOnce = true,
            playerOnly = true,
            autoStart = false
        }
    },
    {
        name = "auto_start",
        data = {
            width = 32,
            height = 32,
            cutsceneId = "ingeste_auto",
            triggerOnce = false,
            playerOnly = true,
            autoStart = true
        }
    }
}

cutsceneTrigger.fieldInformation = {
    cutsceneId = {
        fieldType = "string"
    },
    triggerOnce = {
        fieldType = "boolean"
    },
    playerOnly = {
        fieldType = "boolean"
    },
    autoStart = {
        fieldType = "boolean"
    }
}

function cutsceneTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    local color = {0.9, 0.4, 0.8, 0.7}
    
    if entity.autoStart then
        color = {0.9, 0.8, 0.2, 0.7}
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

function cutsceneTrigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return cutsceneTrigger
