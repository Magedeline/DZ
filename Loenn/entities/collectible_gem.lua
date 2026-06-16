local utils = require("utils")

local collectibleGem = {}

collectibleGem.name = "DZ/CollectibleGem"
collectibleGem.depth = 0

collectibleGem.placements = {
    {
        name = "blue",
        data = {
            gemType = "blue",
            isCollected = false,
            value = 1,
            sparkleEffect = true
        }
    },
    {
        name = "red",
        data = {
            gemType = "red",
            isCollected = false,
            value = 3,
            sparkleEffect = true
        }
    },
    {
        name = "purple",
        data = {
            gemType = "purple",
            isCollected = false,
            value = 5,
            sparkleEffect = true
        }
    },
    {
        name = "golden",
        data = {
            gemType = "golden",
            isCollected = false,
            value = 10,
            sparkleEffect = true
        }
    }
}

collectibleGem.fieldInformation = {
    gemType = {
        options = {"blue", "red", "purple", "golden", "rainbow"},
        editable = false
    },
    isCollected = {
        fieldType = "boolean"
    },
    value = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 50
    },
    sparkleEffect = {
        fieldType = "boolean"
    }
}

function collectibleGem.sprite(room, entity)
    local colors = {
        blue = {0.2, 0.5, 1.0, 1.0},
        red = {1.0, 0.3, 0.3, 1.0},
        purple = {0.8, 0.3, 1.0, 1.0},
        golden = {1.0, 0.8, 0.1, 1.0},
        rainbow = {0.9, 0.6, 0.9, 1.0}
    }
    
    local color = colors[entity.gemType] or colors.blue
    
    if entity.isCollected then
        color[4] = 0.3
    end
    
    return {
        {
            texture = "collectables/strawberry/normal00",
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function collectibleGem.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return collectibleGem
