-- Lost Soul Entity
-- Represents the lost human souls that Madeline can collect and save in Chapter 16

local lostSoul = {}

lostSoul.name = "DZ/LostSoul"

lostSoul.placements = {
    {
        name = "main",
        data = {
            soulId = "LOST_SOUL_1",
            soulType = "human",
            soulIndex = 1,
            floatingHeight = 20.0,
            floatingSpeed = 1.0,
            glowIntensity = 0.8,
            dialogKey = "DZ_CH16_LOST_SOULS_1", 
            message = "Help... us...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "white",
            corruption = 0.0,
            canBeHealed = true
        }
    },
    {
        name = "soul_patience",
        data = {
            soulId = "LOST_SOUL_PATIENCE",
            soulType = "patience",
            soulIndex = 1,
            floatingHeight = 25.0,
            floatingSpeed = 0.8,
            glowIntensity = 1.0,
            dialogKey = "DZ_CH16_LOST_SOULS_2",
            message = "we were... once humans...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "cyan",
            corruption = 0.0,
            canBeHealed = true
        }
    },
    {
        name = "soul_bravery", 
        data = {
            soulId = "LOST_SOUL_BRAVERY",
            soulType = "bravery", 
            soulIndex = 2,
            floatingHeight = 30.0,
            floatingSpeed = 1.2,
            glowIntensity = 1.0,
            dialogKey = "DZ_CH16_LOST_SOULS_3",
            message = "Please... remember us...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "orange",
            corruption = 0.0,
            canBeHealed = true
        }
    },
    {
        name = "soul_integrity",
        data = {
            soulId = "LOST_SOUL_INTEGRITY", 
            soulType = "integrity",
            soulIndex = 3,
            floatingHeight = 22.0,
            floatingSpeed = 0.9,
            glowIntensity = 1.0,
            dialogKey = "DZ_CH16_LOST_SOULS_4",
            message = "Together... we are stronger...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "blue",
            corruption = 0.0,
            canBeHealed = true
        }
    },
    {
        name = "soul_perseverance",
        data = {
            soulId = "LOST_SOUL_PERSEVERANCE",
            soulType = "perseverance",
            soulIndex = 4,
            floatingHeight = 28.0,
            floatingSpeed = 0.7,
            glowIntensity = 1.0,
            dialogKey = "DZ_CH16_LOST_SOULS_5",
            message = "Madeline... we hear you...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "purple",
            corruption = 0.0,
            canBeHealed = true
        }
    },
    {
        name = "soul_kindness",
        data = {
            soulId = "LOST_SOUL_KINDNESS",
            soulType = "kindness",
            soulIndex = 5,
            floatingHeight = 24.0,
            floatingSpeed = 1.1,
            glowIntensity = 1.0,
            dialogKey = "DZ_CH16_LOST_SOULS_6",
            message = "Please... remember us...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "green",
            corruption = 0.0,
            canBeHealed = true
        }
    },
    {
        name = "soul_justice",
        data = {
            soulId = "LOST_SOUL_JUSTICE",
            soulType = "justice",
            soulIndex = 6,
            floatingHeight = 26.0,
            floatingSpeed = 1.0,
            glowIntensity = 1.0,
            dialogKey = "DZ_CH16_LOST_SOULS_7",
            message = "We believe... in your cause...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "yellow",
            corruption = 0.0,
            canBeHealed = true
        }
    },
    {
        name = "soul_determination",
        data = {
            soulId = "LOST_SOUL_DETERMINATION",
            soulType = "determination",
            soulIndex = 7,
            floatingHeight = 32.0,
            floatingSpeed = 1.3,
            glowIntensity = 1.2,
            dialogKey = "DZ_CH16_LOST_SOULS_8",
            message = "End this nightmare... for all of us...",
            autoCollectable = false,
            requiresInteraction = true,
            enableParticles = true,
            soulColor = "red",
            corruption = 0.0,
            canBeHealed = true
        }
    }
}

lostSoul.fieldInformation = {
    soulId = {
        fieldType = "string"
    },
    soulType = {
        fieldType = "string",
        options = {
            "human",
            "patience",
            "bravery", 
            "integrity",
            "perseverance",
            "kindness",
            "justice",
            "determination"
        }
    },
    soulIndex = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 8
    },
    floatingHeight = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 100.0
    },
    floatingSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0
    },
    glowIntensity = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 2.0
    },
    dialogKey = {
        fieldType = "string",
        options = {
            "DZ_CH16_LOST_SOULS_1",
            "DZ_CH16_LOST_SOULS_2",
            "DZ_CH16_LOST_SOULS_3",
            "DZ_CH16_LOST_SOULS_4",
            "DZ_CH16_LOST_SOULS_5",
            "DZ_CH16_LOST_SOULS_6",
            "DZ_CH16_LOST_SOULS_7",
            "DZ_CH16_LOST_SOULS_8"
        }
    },
    message = {
        fieldType = "string"
    },
    autoCollectable = {
        fieldType = "boolean"
    },
    requiresInteraction = {
        fieldType = "boolean"
    },
    enableParticles = {
        fieldType = "boolean"
    },
    soulColor = {
        fieldType = "string",
        options = {
            "white",
            "cyan",
            "orange", 
            "blue",
            "purple",
            "green",
            "yellow",
            "red"
        }
    },
    corruption = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    canBeHealed = {
        fieldType = "boolean"
    }
}

function lostSoul.sprite(room, entity)
    local soulType = entity.soulType or "human"
    local corruption = entity.corruption or 0.0
    
    local spritePath = "characters/souls/"
    
    if corruption > 0.5 then
        spritePath = spritePath .. "corrupted_soul_" .. soulType
    else
        spritePath = spritePath .. "lost_soul_" .. soulType
    end
    
    return spritePath
end

function lostSoul.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

-- Soul colors for visual identification
local soulColors = {
    human = {1.0, 1.0, 1.0, 0.9},        -- White
    patience = {0.2, 0.9, 0.9, 0.9},     -- Cyan
    bravery = {1.0, 0.6, 0.2, 0.9},      -- Orange
    integrity = {0.2, 0.4, 1.0, 0.9},    -- Blue
    perseverance = {0.8, 0.2, 0.9, 0.9}, -- Purple
    kindness = {0.2, 0.9, 0.2, 0.9},     -- Green
    justice = {0.9, 0.9, 0.2, 0.9},      -- Yellow
    determination = {0.9, 0.2, 0.2, 0.9} -- Red
}

function lostSoul.rectangle(room, entity)
    local soulType = entity.soulType or "human"
    local glowIntensity = entity.glowIntensity or 0.8
    local corruption = entity.corruption or 0.0
    
    local color = soulColors[soulType] or soulColors.human
    
    -- Adjust color based on corruption and glow
    color = {
        color[1] * glowIntensity * (1 - corruption * 0.5),
        color[2] * glowIntensity * (1 - corruption * 0.5), 
        color[3] * glowIntensity * (1 - corruption * 0.5),
        color[4]
    }
    
    local size = 16 + (glowIntensity - 1) * 8
    return utils.rectangle(entity.x - size/2, entity.y - size/2, size, size), color
end

-- Node support for soul movement paths
function lostSoul.nodeSprite(room, entity, node, nodeIndex)
    return "objects/DesoloZantas/soul_waypoint"
end

function lostSoul.nodeSelection(room, entity, node)
    return utils.rectangle(node.x - 4, node.y - 4, 8, 8)
end

function lostSoul.nodeLimits()
    return {0, 6} -- Up to 6 waypoints for soul floating pattern
end

return lostSoul