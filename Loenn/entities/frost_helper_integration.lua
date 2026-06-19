-- FrostHelper Integration Entity for Loenn
-- Demonstrates FrostHelper compatibility features in the map editor

local ingesteHelper = require("libraries.ingeste_helper_main")

local frostHelperIntegrationEntity = {}

frostHelperIntegrationEntity.name = "DZ/FrostHelperIntegration"
frostHelperIntegrationEntity.depth = 0
frostHelperIntegrationEntity.texture = "objects/IngesteHelper/frost_integration"
frostHelperIntegrationEntity.justification = {0.5, 0.5}

-- Placements
frostHelperIntegrationEntity.placements = {
    {
        name = "main",
        data = {
            integrationMode = "Auto",
            shareParticles = true,
            shareUtilities = true,
            compatibilityMode = true,
            debugOutput = false
        }
    },
    {
        name = "debug_mode",
        data = {
            integrationMode = "Debug",
            shareParticles = true,
            shareUtilities = true,
            compatibilityMode = true,
            debugOutput = true
        }
    },
    {
        name = "force_enable",
        data = {
            integrationMode = "Force Enable",
            shareParticles = true,
            shareUtilities = true,
            compatibilityMode = false,
            debugOutput = false
        }
    }
}

-- Field information for the map editor
frostHelperIntegrationEntity.fieldInformation = {
    integrationMode = {
        options = {"Auto", "Force Enable", "Force Disable", "Debug"},
        editable = false
    },
    shareParticles = {
        fieldType = "boolean"
    },
    shareUtilities = {
        fieldType = "boolean"
    },
    compatibilityMode = {
        fieldType = "boolean"
    },
    debugOutput = {
        fieldType = "boolean"
    }
}

-- Visual representation
function frostHelperIntegrationEntity.sprite(room, entity)
    local sprites = {}
    
    -- Base color for the main sprite based on integration mode
    local integrationColors = {
        ["Auto"] = ingesteHelper.colors.interactive,
        ["Force Enable"] = {0.2, 0.8, 0.2, 1.0},      -- Green
        ["Force Disable"] = {0.8, 0.2, 0.2, 1.0},     -- Red
        ["Debug"] = {0.8, 0.8, 0.2, 1.0}              -- Yellow
    }
    
    local mainColor = integrationColors[entity.integrationMode] or ingesteHelper.colors.interactive
    
    -- Main entity sprite
    local mainSprite = {
        texture = "objects/memorial/memorial_text",  -- Using existing texture as fallback
        x = entity.x,
        y = entity.y,
        color = mainColor,
        justificationX = 0.5,
        justificationY = 0.5
    }
    
    table.insert(sprites, mainSprite)
    
    -- Add compatibility indicator if needed
    if entity.compatibilityMode then
        local compatSprite = {
            texture = "decals/1-forsakencity/cobweb",
            x = entity.x + 12,
            y = entity.y - 12,
            color = {0.2, 0.8, 0.2, 0.6}, -- Green for compatibility
            justificationX = 0.5,
            justificationY = 0.5,
            scale = 0.5
        }
        table.insert(sprites, compatSprite)
    end
    
    -- Add debug indicator if debug output is enabled
    if entity.debugOutput then
        local debugSprite = {
            texture = "decals/1-forsakencity/cobweb",
            x = entity.x - 12,
            y = entity.y - 12,
            color = {0.8, 0.8, 0.2, 0.6}, -- Yellow for debug
            justificationX = 0.5,
            justificationY = 0.5,
            scale = 0.3
        }
        table.insert(sprites, debugSprite)
    end
    
    return sprites
end

-- Selection rectangle
function frostHelperIntegrationEntity.selection(room, entity)
    return {entity.x - 16, entity.y - 16, 32, 32}
end

-- Node limits (no nodes for this entity)
frostHelperIntegrationEntity.nodeLimits = {0, 0}

-- Rectangle for collision/bounds
function frostHelperIntegrationEntity.rectangle(room, entity)
    return {entity.x - 16, entity.y - 16, 32, 32}
end

return frostHelperIntegrationEntity