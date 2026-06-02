-- DX Dark Matter Boss - DX-Side Exclusive Boss
-- Primordial void entity with Dark Construct echo system
-- Inspired by Desolo Zantas content

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local dxDarkMatter = {}

dxDarkMatter.name = "MaggyHelper/DXDarkMatterBoss"
dxDarkMatter.depth = -100000
dxDarkMatter.justification = {0.5, 0.5}
dxDarkMatter.nodeLimits = {0, 0}

dxDarkMatter.placements = {
    {
        name = "DXDarkMatter",
        data = {
            maxHealth = 2000,
            autoStart = true,
            showHealthBar = true,
            enableDarkConstruct = true,
            enableVoidHeart = true,
            enableEventHorizon = true,
            enableSingularity = true,
            echoFlowey = true,
            echoAsriel = true,
            echoDedede = true,
            bossMusic = ""
        }
    },
    {
        name = "DXDarkMatter_Void",
        data = {
            maxHealth = 3000,
            autoStart = true,
            showHealthBar = true,
            enableDarkConstruct = true,
            enableVoidHeart = true,
            enableEventHorizon = true,
            enableSingularity = true,
            echoFlowey = true,
            echoAsriel = true,
            echoDedede = true,
            bossMusic = ""
        }
    }
}

dxDarkMatter.fieldInformation = {
    maxHealth = {
        fieldType = "integer",
        minimumValue = 100,
        maximumValue = 10000
    },
    autoStart = { fieldType = "boolean" },
    showHealthBar = { fieldType = "boolean" },
    enableDarkConstruct = { fieldType = "boolean" },
    enableVoidHeart = { fieldType = "boolean" },
    enableEventHorizon = { fieldType = "boolean" },
    enableSingularity = { fieldType = "boolean" },
    echoFlowey = { fieldType = "boolean" },
    echoAsriel = { fieldType = "boolean" },
    echoDedede = { fieldType = "boolean" },
    bossMusic = { fieldType = "string" }
}

dxDarkMatter.fieldOrder = {
    "x", "y", "maxHealth",
    "autoStart", "showHealthBar",
    "enableDarkConstruct", "enableVoidHeart",
    "enableEventHorizon", "enableSingularity",
    "echoFlowey", "echoAsriel", "echoDedede",
    "bossMusic"
}

function dxDarkMatter.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/dx_darkmatter/darkmatter/dormant00", entity)
    sprite:setJustification(0.5, 0.5)
    return sprite
end

return dxDarkMatter
