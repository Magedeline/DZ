-- DX Asriel Transcendence Boss - DX-Side Ultimate Asriel
-- Features Cosmic Judgment finale and dimensional rift mechanics
-- Inspired by Desolo Zantas content

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local dxAsrielTranscendence = {}

dxAsrielTranscendence.name = "DZ/DXAsrielTranscendenceBoss"
dxAsrielTranscendence.depth = -100000
dxAsrielTranscendence.justification = {0.5, 0.5}
dxAsrielTranscendence.nodeLimits = {0, 4}
dxAsrielTranscendence.nodeLineRenderType = "line"

dxAsrielTranscendence.placements = {
    {
        name = "main",
        data = {
            maxHealth = 3500,
            maxTranscendence = 100.0,
            autoStart = false,
            showHealthBar = true,
            enableDivineFury = true,
            enableAstralStorm = true,
            enableTranscendenceRift = true,
            enableSoulConvergence = true,
            enableCosmicJudgment = true,
            musicAwakening = "",
            musicFury = "",
            musicTranscendence = "",
            musicJudgment = "",
            dialogAwakening = "DXSIDE_ASRIEL_AWAKENING",
            dialogRedemption = "DXSIDE_ASRIEL_REDEMPTION"
        }
    },
    {
        name = "DXAsrielTranscendence_Ultimate",
        data = {
            maxHealth = 5000,
            maxTranscendence = 100.0,
            autoStart = false,
            showHealthBar = true,
            enableDivineFury = true,
            enableAstralStorm = true,
            enableTranscendenceRift = true,
            enableSoulConvergence = true,
            enableCosmicJudgment = true,
            musicAwakening = "",
            musicFury = "",
            musicTranscendence = "",
            musicJudgment = "",
            dialogAwakening = "DXSIDE_ASRIEL_AWAKENING",
            dialogRedemption = "DXSIDE_ASRIEL_REDEMPTION"
        }
    }
}

dxAsrielTranscendence.fieldInformation = {
    maxHealth = {
        fieldType = "integer",
        minimumValue = 100,
        maximumValue = 20000
    },
    maxTranscendence = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 200.0
    },
    autoStart = { fieldType = "boolean" },
    showHealthBar = { fieldType = "boolean" },
    enableDivineFury = { fieldType = "boolean" },
    enableAstralStorm = { fieldType = "boolean" },
    enableTranscendenceRift = { fieldType = "boolean" },
    enableSoulConvergence = { fieldType = "boolean" },
    enableCosmicJudgment = { fieldType = "boolean" },
    musicAwakening = { fieldType = "string" },
    musicFury = { fieldType = "string" },
    musicTranscendence = { fieldType = "string" },
    musicJudgment = { fieldType = "string" },
    dialogAwakening = { fieldType = "string" },
    dialogRedemption = { fieldType = "string" }
}

dxAsrielTranscendence.fieldOrder = {
    "x", "y", "maxHealth", "maxTranscendence",
    "autoStart", "showHealthBar",
    "enableDivineFury", "enableAstralStorm",
    "enableTranscendenceRift", "enableSoulConvergence",
    "enableCosmicJudgment",
    "musicAwakening", "musicFury", "musicTranscendence", "musicJudgment",
    "dialogAwakening", "dialogRedemption"
}

function dxAsrielTranscendence.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/dx_asriel_transcendence/face/00", entity)
    sprite:setJustification(0.5, 0.5)
    return sprite
end

return dxAsrielTranscendence
