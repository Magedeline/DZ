-- Asriel Angel of Death Boss - Chapter 20: The End
-- Multi-phase boss with barrier mechanics, lost soul salvation, and emotional story beats
-- Sprite path: characters/asrielangelofdeathboss

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local asrielAngelBoss = {}

asrielAngelBoss.name = "MaggyHelper/AsrielAngelOfDeathBoss"
asrielAngelBoss.depth = -100000
asrielAngelBoss.texture = "characters/asrielangelofdeathboss/face/00"
asrielAngelBoss.justification = {0.5, 0.5}
asrielAngelBoss.nodeLimits = {0, 4}
asrielAngelBoss.nodeLineRenderType = "line"

asrielAngelBoss.placements = {
    {
        name = "AsrielAngelOfDeathBoss",
        data = {
            health = 2500,
            maxHealth = 2500,
            enableBarrier = true,
            enableLostSouls = true,
            enableFlashback = true,
            enableFinalBeam = true,
            -- Music tracks
            musicBurnInDespair = "event:/desolo_zantas/final_content/music/lvl20/burn_in_despair",
            musicHisTheme01 = "event:/desolo_zantas/final_content/music/lvl20/his_theme01",
            musicHisTheme02 = "event:/desolo_zantas/final_content/music/lvl20/his_theme02",
            musicKirbyVsAsriel = "event:/desolo_zantas/final_content/music/lvl20/kirby_vs_asriel_fight_02",
            -- Phase configuration
            barrierWidth = 400,
            barrierHeight = 300,
            riseSpeed = 3.0,
            -- Dialog keys
            dialogPhase1 = "CH20_ASRIEL_ZERO_RISE_KILL",
            dialogStruggle = "CH20_ASRIEL_ZERO_STRUGGLE_START",
            dialogVoidAnswer = "CH20_ASRIEL_ZERO_VOID_ANSWERS",
            dialogFlashback = "CH20_ASRIEL_REMEMBER_A"
        }
    },
    {
        name = "hard_mode",
        data = {
            health = 3500,
            maxHealth = 3500,
            enableBarrier = true,
            enableLostSouls = true,
            enableFlashback = true,
            enableFinalBeam = true,
            musicBurnInDespair = "event:/desolo_zantas/final_content/music/lvl20/burn_in_despair",
            musicHisTheme01 = "event:/desolo_zantas/final_content/music/lvl20/his_theme01",
            musicHisTheme02 = "event:/desolo_zantas/final_content/music/lvl20/his_theme02",
            musicKirbyVsAsriel = "event:/desolo_zantas/final_content/music/lvl20/kirby_vs_asriel_fight_02",
            barrierWidth = 350,
            barrierHeight = 280,
            riseSpeed = 2.0,
            dialogPhase1 = "CH20_ASRIEL_ZERO_RISE_KILL",
            dialogStruggle = "CH20_ASRIEL_ZERO_STRUGGLE_START",
            dialogVoidAnswer = "CH20_ASRIEL_ZERO_VOID_ANSWERS",
            dialogFlashback = "CH20_ASRIEL_REMEMBER_A"
        }
    },
    {
        name = "cutscene_only",
        data = {
            health = 1,
            maxHealth = 1,
            enableBarrier = false,
            enableLostSouls = false,
            enableFlashback = true,
            enableFinalBeam = false,
            musicBurnInDespair = "event:/desolo_zantas/final_content/music/lvl20/burn_in_despair",
            musicHisTheme01 = "event:/desolo_zantas/final_content/music/lvl20/his_theme01",
            musicHisTheme02 = "event:/desolo_zantas/final_content/music/lvl20/his_theme02",
            musicKirbyVsAsriel = "event:/desolo_zantas/final_content/music/lvl20/kirby_vs_asriel_fight_02",
            barrierWidth = 400,
            barrierHeight = 300,
            riseSpeed = 3.0,
            dialogPhase1 = "CH20_ASRIEL_ZERO_RISE_KILL",
            dialogStruggle = "CH20_ASRIEL_ZERO_STRUGGLE_START",
            dialogVoidAnswer = "CH20_ASRIEL_ZERO_VOID_ANSWERS",
            dialogFlashback = "CH20_ASRIEL_REMEMBER_A"
        }
    }
}

asrielAngelBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10000
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10000
    },
    barrierWidth = {
        fieldType = "integer",
        minimumValue = 100,
        maximumValue = 1000
    },
    barrierHeight = {
        fieldType = "integer",
        minimumValue = 100,
        maximumValue = 800
    },
    riseSpeed = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 10.0
    },
    musicBurnInDespair = {
        fieldType = "string",
        editable = true
    },
    musicHisTheme01 = {
        fieldType = "string",
        editable = true
    },
    musicHisTheme02 = {
        fieldType = "string",
        editable = true
    },
    musicKirbyVsAsriel = {
        fieldType = "string",
        editable = true
    },
    dialogPhase1 = {
        fieldType = "string",
        editable = true
    },
    dialogStruggle = {
        fieldType = "string",
        editable = true
    },
    dialogVoidAnswer = {
        fieldType = "string",
        editable = true
    },
    dialogFlashback = {
        fieldType = "string",
        editable = true
    }
}

asrielAngelBoss.fieldOrder = {
    "x", "y",
    "health", "maxHealth",
    "enableBarrier", "enableLostSouls", "enableFlashback", "enableFinalBeam",
    "barrierWidth", "barrierHeight", "riseSpeed",
    "musicBurnInDespair", "musicHisTheme01", "musicHisTheme02", "musicKirbyVsAsriel",
    "dialogPhase1", "dialogStruggle", "dialogVoidAnswer", "dialogFlashback"
}

-- Custom sprite rendering for the multi-component boss
function asrielAngelBoss.sprite(room, entity)
    local sprites = {}
    
    -- Background layer
    local bgSprite = drawableSprite.fromTexture("characters/asrielangelofdeathboss/bg/00", entity)
    if bgSprite then
        bgSprite:setJustification(0.5, 0.5)
        bgSprite:setColor({1.0, 1.0, 1.0, 0.5})
        table.insert(sprites, bgSprite)
    end
    
    -- Cosmowing layer (rainbow wings)
    local cosmowingSprite = drawableSprite.fromTexture("characters/asrielangelofdeathboss/cosmoswing/00", entity)
    if cosmowingSprite then
        cosmowingSprite:setJustification(0.5, 0.5)
        table.insert(sprites, cosmowingSprite)
    end
    
    -- Stem layer
    local stemSprite = drawableSprite.fromTexture("characters/asrielangelofdeathboss/stem/00", entity)
    if stemSprite then
        stemSprite:setJustification(0.5, 0.5)
        table.insert(sprites, stemSprite)
    end
    
    -- Shoulder layer
    local shoulderSprite = drawableSprite.fromTexture("characters/asrielangelofdeathboss/shoulder/00", entity)
    if shoulderSprite then
        shoulderSprite:setJustification(0.5, 0.5)
        table.insert(sprites, shoulderSprite)
    end
    
    -- Orb wings layer
    local orbwingSprite = drawableSprite.fromTexture("characters/asrielangelofdeathboss/orbwing/00", entity)
    if orbwingSprite then
        orbwingSprite:setJustification(0.5, 0.5)
        table.insert(sprites, orbwingSprite)
    end
    
    -- Orb layer
    local orbSprite = drawableSprite.fromTexture("characters/asrielangelofdeathboss/orb/00", entity)
    if orbSprite then
        orbSprite:setJustification(0.5, 0.5)
        table.insert(sprites, orbSprite)
    end
    
    -- Face layer (main visible part)
    local faceSprite = drawableSprite.fromTexture("characters/asrielangelofdeathboss/face/00", entity)
    if faceSprite then
        faceSprite:setJustification(0.5, 0.5)
        table.insert(sprites, faceSprite)
    end
    
    -- If no sprites loaded, use fallback
    if #sprites == 0 then
        local fallback = drawableSprite.fromTexture("characters/Maggy/DesoloZantas/asriel/idle00", entity)
        if fallback then
            fallback:setJustification(0.5, 1.0)
            table.insert(sprites, fallback)
        end
    end
    
    return sprites
end

-- Selection rectangle for the boss
function asrielAngelBoss.selection(room, entity)
    return utils.rectangle(entity.x - 64, entity.y - 96, 128, 192)
end

return asrielAngelBoss

