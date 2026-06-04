-- Loenn integration for Darkener Fountain entity
-- Creates a fountain effect that darkens or transforms the environment

local selectionBounds = require("libraries.selection_bounds")

local darkernerFountain = {}

local defaultBounds = {
    width = 24,
    height = 36
}

darkernerFountain.name = "MaggyHelper/DarkernerFountain"
darkernerFountain.depth = -5
darkernerFountain.texture = "objects/fountain_darkener"

darkernerFountain.fieldInformation = {
    fountainType = {
        fieldType = "string",
        options = {
            "Chaos",
            "Pure",
            "Shadow",
            "Void"
        }
    },
    activationRadius = {
        fieldType = "number",
        minimumValue = 16,
        maximumValue = 256
    },
    intensity = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0
    },
    duration = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 30.0
    },
    particleCount = {
        fieldType = "integer",
        minimumValue = 10,
        maximumValue = 100
    },
    autoActivate = {
        fieldType = "boolean"
    },
    requiresFlag = {
        fieldType = "string",
        options = {}
    },
    transformsKglobal::global::Celeste.Player= {
        fieldType = "boolean"
    },
    persistentEffect = {
        fieldType = "boolean"
    },
    soundEffect = {
        fieldType = "string",
        options = {
            "guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}",
            "guid://{b4af9a48-0da6-4374-972b-c4e4966b7003}",
            "guid://{884bccb8-958a-4278-8a1b-daf74fff37c0}"
        },
        editable = true
    }
}

darkernerFountain.placements = {
    {
        name = "chaos_fountain",
        data = {
            width = 32,
            height = 48,
            fountainType = "Chaos",
            activationRadius = 64,
            intensity = 1.5,
            duration = 10.0,
            particleCount = 50,
            autoActivate = false,
            requiresFlag = "",
            transformsKglobal::global::Celeste.Player= true,
            persistentEffect = false,
            soundEffect = "guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}"
        }
    },
    {
        name = "pure_fountain",
        data = {
            width = 24,
            height = 36,
            fountainType = "Pure",
            activationRadius = 48,
            intensity = 1.0,
            duration = 15.0,
            particleCount = 30,
            autoActivate = true,
            requiresFlag = "",
            transformsKglobal::global::Celeste.Player= false,
            persistentEffect = true,
            soundEffect = "event:/char/Maggy/DesoloZantas/madeline/dreamblock_enter"
        }
    },
    {
        name = "shadow_fountain",
        data = {
            width = 40,
            height = 56,
            fountainType = "Shadow",
            activationRadius = 96,
            intensity = 2.0,
            duration = 8.0,
            particleCount = 75,
            autoActivate = false,
            requiresFlag = "shadow_fountain_unlocked",
            transformsKglobal::global::Celeste.Player= true,
            persistentEffect = false,
            soundEffect = "guid://{884bccb8-958a-4278-8a1b-daf74fff37c0}"
        }
    }
}

function darkernerFountain.sprite(room, entity)
    local bounds = selectionBounds.resolve(entity, defaultBounds)
    local sprite = {}
    
    sprite.texture = darkernerFountain.texture
    sprite.x = bounds.x
    sprite.y = bounds.y
    sprite.scaleX = bounds.width / 24
    sprite.scaleY = bounds.height / 36
    
    -- Add visual indicator for fountain type
    if entity.fountainType == "Chaos" then
        sprite.color = {1.0, 0.2, 0.8, 0.8}  -- Pink/magenta
    elseif entity.fountainType == "Pure" then
        sprite.color = {0.9, 0.9, 1.0, 0.9}  -- Light blue/white
    elseif entity.fountainType == "Shadow" then
        sprite.color = {0.3, 0.1, 0.5, 0.9}  -- Dark purple
    elseif entity.fountainType == "Void" then
        sprite.color = {0.1, 0.1, 0.1, 0.9}  -- Very dark
    end
    
    return sprite
end

return darkernerFountain

