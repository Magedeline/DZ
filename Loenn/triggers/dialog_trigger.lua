local dialogTrigger = {}

dialogTrigger.name = "DZ/DialogTrigger"
dialogTrigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
            dialogKey = "DIALOG_DEFAULT",
            triggerOnce = true,
            requireInteraction = false,
            npcName = ""
        }
    },
    {
        name = "interaction",
        data = {
            width = 24,
            height = 24,
            dialogKey = "DIALOG_INTERACT",
            triggerOnce = false,
            requireInteraction = true,
            npcName = "Chara"
        }
    }
}

dialogTrigger.fieldInformation = {
    dialogKey = {
        fieldType = "string"
    },
    triggerOnce = {
        fieldType = "boolean"
    },
    requireInteraction = {
        fieldType = "boolean"
    },
    npcName = {
        fieldType = "string"
    }
}

function dialogTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    local color = {0.3, 0.8, 0.9, 0.7}
    
    if entity.requireInteraction then
        color = {0.9, 0.7, 0.3, 0.7}
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

function dialogTrigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return dialogTrigger
