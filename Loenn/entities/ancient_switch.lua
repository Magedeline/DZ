local utils = require("utils")

local ancientSwitch = {}

ancientSwitch.name = "MaggyHelper/AncientSwitch"
ancientSwitch.depth = 0

ancientSwitch.placements = {
    {
        name = "normal",
        data = {
            isActivated = false,
            switchType = "pressure",
            targetEntity = "",
            persistent = true,
            requiresWeight = false
        }
    },
    {
        name = "lever",
        data = {
            isActivated = false,
            switchType = "lever",
            targetEntity = "",
            persistent = true,
            requiresWeight = false
        }
    },
    {
        name = "crystal",
        data = {
            isActivated = false,
            switchType = "crystal",
            targetEntity = "",
            persistent = false,
            requiresWeight = false
        }
    }
}

ancientSwitch.fieldInformation = {
    isActivated = {
        fieldType = "boolean"
    },
    switchType = {
        options = {"pressure", "lever", "crystal", "button", "magical"},
        editable = false
    },
    targetEntity = {
        fieldType = "string"
    },
    persistent = {
        fieldType = "boolean"
    },
    requiresWeight = {
        fieldType = "boolean"
    }
}

function ancientSwitch.sprite(room, entity)
    local colors = {
        pressure = {0.8, 0.7, 0.6, 1.0},
        lever = {0.6, 0.8, 0.4, 1.0},
        crystal = {0.4, 0.6, 1.0, 1.0},
        button = {0.9, 0.5, 0.3, 1.0},
        magical = {0.8, 0.4, 0.9, 1.0}
    }
    
    local color = colors[entity.switchType] or colors.pressure
    
    if entity.isActivated then
        color = {color[1], color[2], color[3], 0.8}
    end
    
    return {
        {
            texture = "objects/Maggy/DesoloZantas/temple/switch00",
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function ancientSwitch.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return ancientSwitch
