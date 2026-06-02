local soul_fragment = {}

soul_fragment.name = "DesoloZantas/SoulFragment"
soul_fragment.depth = -100
soul_fragment.placements = {
    {
        name = "Soul Fragment (Red)",
        data = {
            barrierId = "",
            color = "red"
        }
    },
    {
        name = "Soul Fragment (Orange)",
        data = {
            barrierId = "",
            color = "orange"
        }
    },
    {
        name = "Soul Fragment (Yellow)",
        data = {
            barrierId = "",
            color = "yellow"
        }
    },
    {
        name = "Soul Fragment (Green)",
        data = {
            barrierId = "",
            color = "green"
        }
    },
    {
        name = "Soul Fragment (Cyan)",
        data = {
            barrierId = "",
            color = "cyan"
        }
    },
    {
        name = "Soul Fragment (Blue)",
        data = {
            barrierId = "",
            color = "blue"
        }
    },
    {
        name = "Soul Fragment (Purple)",
        data = {
            barrierId = "",
            color = "purple"
        }
    }
}

soul_fragment.fieldInformation = {
    color = {
        fieldType = "string",
        options = {"red", "orange", "yellow", "green", "cyan", "blue", "purple"}
    }
}

local colorMap = {
    red = {1.0, 0.2, 0.2, 1.0},
    orange = {1.0, 0.6, 0.2, 1.0},
    yellow = {1.0, 1.0, 0.2, 1.0},
    green = {0.2, 1.0, 0.2, 1.0},
    cyan = {0.2, 1.0, 1.0, 1.0},
    blue = {0.2, 0.2, 1.0, 1.0},
    purple = {0.6, 0.2, 1.0, 1.0}
}

function soul_fragment.sprite(room, entity)
    local sprite = require("structs.drawable_sprite").fromTexture("collectables/heartGem/0/00", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setScale(0.5, 0.5)
    
    local color = colorMap[entity.color] or colorMap.red
    sprite:setColor(color)
    
    return sprite
end

return soul_fragment
