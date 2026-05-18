-- DX Flowey Omega Boss - DX-Side Enhanced Flowey
-- Corruption Overload finale with remixed phases from the original
-- Inspired by Desolo Zantas content

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local dxFloweyOmega = {}

dxFloweyOmega.name = "MaggyHelper/DXFloweyOmegaBoss"
dxFloweyOmega.depth = -100000
dxFloweyOmega.justification = {0.5, 1.0}
dxFloweyOmega.nodeLimits = {0, 0}

dxFloweyOmega.placements = {
    {
        name = "DXFloweyOmega",
        data = {
            maxHealth = 1500,
            corruptionLevel = 0.0,
            maxCorruption = 100.0,
            autoStart = true,
            showHealthBar = true,
            enableCorruptedGarden = true,
            enableAbyssalCathedral = true,
            enableNightmareNexus = true,
            enableArsenalOverdrive = true,
            enableSoulHarvest = true,
            enableCorruptionOverload = true,
            bossMusic = ""
        }
    },
    {
        name = "DXFloweyOmega_Nightmare",
        data = {
            maxHealth = 2200,
            corruptionLevel = 25.0,
            maxCorruption = 100.0,
            autoStart = true,
            showHealthBar = true,
            enableCorruptedGarden = true,
            enableAbyssalCathedral = true,
            enableNightmareNexus = true,
            enableArsenalOverdrive = true,
            enableSoulHarvest = true,
            enableCorruptionOverload = true,
            bossMusic = ""
        }
    }
}

dxFloweyOmega.fieldInformation = {
    maxHealth = {
        fieldType = "integer",
        minimumValue = 100,
        maximumValue = 10000
    },
    corruptionLevel = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 100.0
    },
    maxCorruption = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 200.0
    },
    autoStart = { fieldType = "boolean" },
    showHealthBar = { fieldType = "boolean" },
    enableCorruptedGarden = { fieldType = "boolean" },
    enableAbyssalCathedral = { fieldType = "boolean" },
    enableNightmareNexus = { fieldType = "boolean" },
    enableArsenalOverdrive = { fieldType = "boolean" },
    enableSoulHarvest = { fieldType = "boolean" },
    enableCorruptionOverload = { fieldType = "boolean" },
    bossMusic = { fieldType = "string" }
}

dxFloweyOmega.fieldOrder = {
    "x", "y", "maxHealth", "corruptionLevel", "maxCorruption",
    "autoStart", "showHealthBar",
    "enableCorruptedGarden", "enableAbyssalCathedral",
    "enableNightmareNexus", "enableArsenalOverdrive",
    "enableSoulHarvest", "enableCorruptionOverload",
    "bossMusic"
}

function dxFloweyOmega.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/dx_flowey_omega/Maggy/DesoloZantas/flowey/idle00", entity)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

return dxFloweyOmega
