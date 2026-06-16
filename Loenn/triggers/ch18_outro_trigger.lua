local ch18OutroTrigger = {}

ch18OutroTrigger.name = "DZ/CH18OutroTrigger"
ch18OutroTrigger.placements = {
    {
        name = "ch18_outro_trigger",
        data = {
            width = 32,
            height = 32,
            triggerOnce = true
        }
    }
}

ch18OutroTrigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean",
        description = "Whether this trigger only fires once per level load"
    }
}

function ch18OutroTrigger.sprite(room, entity)
    local width = entity.width or 32
    local height = entity.height or 32
    
    -- Red color to indicate this is a special/dangerous trigger
    local color = {1.0, 0.2, 0.2, 0.8}
    
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

function ch18OutroTrigger.selection(room, entity)
    local width = entity.width or 32
    local height = entity.height or 32
    return {entity.x, entity.y, width, height}
end

return ch18OutroTrigger