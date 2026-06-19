local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local mysteryManCoreMessage = {}

mysteryManCoreMessage.name = "DZ/MysteryManCoreMessage"
mysteryManCoreMessage.depth = -1000000
mysteryManCoreMessage.texture = "collectables/heartGem/0/00"

mysteryManCoreMessage.placements = {
    {
        name = "main",
        data = {
            dialogKey = "DZ_CH18_ENDING",
            line = 0,
            shimmerSpeed = 2.0,
            textScale = 1.25,
            useRainbowShimmer = false,
            visibilityDistance = 128.0,
            baseColor = "E0FFFF",
            shimmerColor = "FFD700"
        }
    },
    {
        name = "mystery_man_cyan",
        data = {
            dialogKey = "DZ_CH18_ENDING",
            line = 0,
            shimmerSpeed = 2.5,
            textScale = 1.25,
            useRainbowShimmer = false,
            visibilityDistance = 128.0,
            baseColor = "00FFFF",
            shimmerColor = "FFFFFF"
        }
    },
    {
        name = "mystery_man_golden",
        data = {
            dialogKey = "DZ_CH18_ENDING",
            line = 0,
            shimmerSpeed = 1.5,
            textScale = 1.25,
            useRainbowShimmer = false,
            visibilityDistance = 128.0,
            baseColor = "FFD700",
            shimmerColor = "FFF8DC"
        }
    },
    {
        name = "mystery_man_rainbow",
        data = {
            dialogKey = "DZ_CH18_ENDING",
            line = 0,
            shimmerSpeed = 3.0,
            textScale = 1.25,
            useRainbowShimmer = true,
            visibilityDistance = 128.0,
            baseColor = "FFFFFF",
            shimmerColor = "FFFFFF"
        }
    },
    {
        name = "mystery_man_void",
        data = {
            dialogKey = "DZ_CH18_ENDING",
            line = 0,
            shimmerSpeed = 1.0,
            textScale = 1.5,
            useRainbowShimmer = false,
            visibilityDistance = 160.0,
            baseColor = "9370DB",
            shimmerColor = "E6E6FA"
        }
    },
    {
        name = "madeline",
        data = {
            dialogKey = "DZ_CH18_ENDING",
            line = 0,
            shimmerSpeed = 2.0,
            textScale = 1.25,
            useRainbowShimmer = false,
            visibilityDistance = 128.0,
            baseColor = "DC143C",
            shimmerColor = "FF69B4"
        }
    },
    {
        name = "badeline",
        data = {
            dialogKey = "DZ_CH18_ENDING",
            line = 0,
            shimmerSpeed = 2.0,
            textScale = 1.25,
            useRainbowShimmer = false,
            visibilityDistance = 128.0,
            baseColor = "76428A",
            shimmerColor = "9932CC"
        }
    }
}

mysteryManCoreMessage.fieldInformation = {
    dialogKey = {
        fieldType = "string"
    },
    line = {
        fieldType = "integer",
        minimumValue = 0
    },
    shimmerSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    textScale = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 5.0
    },
    visibilityDistance = {
        fieldType = "number",
        minimumValue = 32.0,
        maximumValue = 512.0
    },
    baseColor = {
        fieldType = "color"
    },
    shimmerColor = {
        fieldType = "color"
    }
}

mysteryManCoreMessage.fieldOrder = {
    "x", "y",
    "dialogKey", "line",
    "baseColor", "shimmerColor",
    "shimmerSpeed", "textScale",
    "visibilityDistance", "useRainbowShimmer"
}

function mysteryManCoreMessage.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("collectables/heartGem/0/00", entity)
    sprite:setJustification(0.5, 0.5)
    
    -- Color based on entity settings
    local baseColor = entity.baseColor or "E0FFFF"
    sprite:setColor(utils.getColor(baseColor))
    
    return sprite
end

function mysteryManCoreMessage.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return mysteryManCoreMessage
