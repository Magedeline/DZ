-- Madeline Tentacle Entity
-- Represents Madeline's tentacle manifestations during her rage in Chapter 16

local madelineTentacle = {}

madelineTentacle.name = "MaggyHelper/MadelineTentacle"

madelineTentacle.placements = {
    {
        name = "appear",
        data = {
            tentacleType = "appear",
            growthStage = 1,
            attackTarget = "flowey",
            animationSpeed = 1.0,
            maxLength = 100.0,
            thickness = 8.0,
            color = "darkred",
            enableParticles = true,
            triggerOnDialog = true,
            dialogTrigger = "dz_trigger 0 madeline tentacle appear",
            autoGrow = false,
            persistent = false
        }
    },
    {
        name = "appear_more",
        data = {
            tentacleType = "appear_more", 
            growthStage = 2,
            attackTarget = "flowey",
            animationSpeed = 1.2,
            maxLength = 150.0,
            thickness = 12.0,
            color = "darkred",
            enableParticles = true,
            triggerOnDialog = true,
            dialogTrigger = "dz_trigger 1 madeline tentacle appear more",
            autoGrow = false,
            persistent = false
        }
    },
    {
        name = "grow_more", 
        data = {
            tentacleType = "grow_even_more",
            growthStage = 3,
            attackTarget = "flowey",
            animationSpeed = 1.5,
            maxLength = 200.0,
            thickness = 16.0,
            color = "crimson",
            enableParticles = true,
            triggerOnDialog = true,
            dialogTrigger = "dz_trigger 2 madeline tentacle appear grow even more",
            autoGrow = false,
            persistent = false
        }
    },
    {
        name = "attack",
        data = {
            tentacleType = "attack_flowey",
            growthStage = 4,
            attackTarget = "flowey",
            animationSpeed = 2.0,
            maxLength = 250.0,
            thickness = 20.0,
            color = "blood",
            enableParticles = true,
            triggerOnDialog = true,
            dialogTrigger = "dz_trigger 0 madeline tentacle attack flowey but flowey deflected",
            autoGrow = true,
            persistent = false,
            canBeDeflected = true
        }
    }
}

madelineTentacle.fieldInformation = {
    tentacleType = {
        fieldType = "string",
        options = {
            "appear",
            "appear_more",
            "grow_even_more", 
            "grow_even_even_more",
            "grow_MOOOORE",
            "attack_flowey"
        }
    },
    growthStage = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 5
    },
    attackTarget = {
        fieldType = "string",
        options = {
            "flowey",
            "none",
            "Kglobal::Player",
            "environment"
        }
    },
    animationSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    maxLength = {
        fieldType = "number", 
        minimumValue = 50.0,
        maximumValue = 500.0
    },
    thickness = {
        fieldType = "number",
        minimumValue = 4.0,
        maximumValue = 32.0
    },
    color = {
        fieldType = "string",
        options = {
            "darkred",
            "crimson", 
            "blood",
            "black",
            "purple",
            "corruption"
        }
    },
    enableParticles = {
        fieldType = "boolean"
    },
    triggerOnDialog = {
        fieldType = "boolean"
    },
    dialogTrigger = {
        fieldType = "string"
    },
    autoGrow = {
        fieldType = "boolean"
    },
    persistent = {
        fieldType = "boolean"
    },
    canBeDeflected = {
        fieldType = "boolean"
    }
}

function madelineTentacle.sprite(room, entity)
    local tentacleType = entity.tentacleType or "appear"
    local growthStage = entity.growthStage or 1
    
    local spritePath = "characters/madeline/tentacles/"
    
    if tentacleType == "appear" then
        spritePath = spritePath .. "tentacle_appear_01"
    elseif tentacleType == "appear_more" then
        spritePath = spritePath .. "tentacle_appear_02"
    elseif tentacleType:find("grow") then
        spritePath = spritePath .. "tentacle_grow_" .. string.format("%02d", growthStage)
    elseif tentacleType == "attack_flowey" then
        spritePath = spritePath .. "tentacle_attack_01"
    else
        spritePath = spritePath .. "tentacle_idle"
    end
    
    return spritePath
end

function madelineTentacle.selection(room, entity)
    local length = entity.maxLength or 100.0
    local thickness = entity.thickness or 8.0
    
    return utils.rectangle(entity.x - thickness/2, entity.y - thickness/2, 
                          length, thickness)
end

-- Color coding for different tentacle types and rage levels
local tentacleColors = {
    appear = {0.6, 0.1, 0.1, 0.8},           -- Dark red
    appear_more = {0.7, 0.1, 0.1, 0.8},      -- Slightly brighter red
    grow_even_more = {0.8, 0.2, 0.2, 0.8},   -- Crimson
    attack_flowey = {0.9, 0.1, 0.1, 0.9}     -- Bright blood red
}

function madelineTentacle.rectangle(room, entity)
    local tentacleType = entity.tentacleType or "appear"
    local length = entity.maxLength or 100.0
    local thickness = entity.thickness or 8.0
    
    local colorKey = tentacleType
    if tentacleType:find("grow") then
        colorKey = "grow_even_more"
    end
    
    local color = tentacleColors[colorKey] or tentacleColors.appear
    
    return utils.rectangle(entity.x - thickness/2, entity.y - thickness/2,
                          length, thickness), color
end

-- Node support for tentacle path/target
function madelineTentacle.nodeSprite(room, entity, node, nodeIndex)
    local tentacleType = entity.tentacleType or "appear"
    
    if tentacleType == "attack_flowey" then
        return "objects/DesoloZantas/tentacle_target"
    else
        return "objects/DesoloZantas/tentacle_waypoint"
    end
end

function madelineTentacle.nodeSelection(room, entity, node)
    return utils.rectangle(node.x - 6, node.y - 6, 12, 12)
end

function madelineTentacle.nodeLimits()
    return {0, 5} -- Up to 5 waypoints for tentacle movement
end

return madelineTentacle
