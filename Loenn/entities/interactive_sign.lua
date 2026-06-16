local interactiveSign = {}

interactiveSign.name = "DZ/InteractiveSign"
interactiveSign.depth = 0

interactiveSign.placements = {
    {
        name = "normal",
        data = {
            dialogKey = "SIGN_DEFAULT",
            signType = "wooden",
            isActive = true,
            showPrompt = true
        }
    },
    {
        name = "stone",
        data = {
            dialogKey = "SIGN_STONE",
            signType = "stone",
            isActive = true,
            showPrompt = true
        }
    },
    {
        name = "magical",
        data = {
            dialogKey = "SIGN_MAGICAL",
            signType = "magical",
            isActive = true,
            showPrompt = false
        }
    }
}

interactiveSign.fieldInformation = {
    dialogKey = {
        fieldType = "string"
    },
    signType = {
        options = {"wooden", "stone", "magical", "ancient", "metal"},
        editable = false
    },
    isActive = {
        fieldType = "boolean"
    },
    showPrompt = {
        fieldType = "boolean"
    }
}

function interactiveSign.sprite(room, entity)
    local colors = {
        wooden = {0.8, 0.6, 0.4, 1.0},
        stone = {0.7, 0.7, 0.8, 1.0},
        magical = {0.6, 0.4, 1.0, 1.0},
        ancient = {0.5, 0.8, 0.3, 1.0},
        metal = {0.8, 0.8, 0.9, 1.0}
    }
    
    local color = colors[entity.signType] or colors.wooden
    
    if not entity.isActive then
        color = {0.5, 0.5, 0.5, 0.7}
    end
    
    return {
        {
            texture = "objects/memorial/memorial_text",
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function interactiveSign.selection(room, entity)
    return {entity.x - 16, entity.y - 16, 32, 32}
end

return interactiveSign
