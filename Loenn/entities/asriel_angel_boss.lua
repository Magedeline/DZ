-- Asriel Angel of Death Boss - Chapter 20: The End
-- Multi-phase boss with barrier mechanics, lost soul salvation, and emotional story beats
-- Sprite path: characters/asrielangelofdeathboss

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local asrielAngelBoss = {}

asrielAngelBoss.name = "DZ/AsrielAngelOfDeathBoss"
asrielAngelBoss.depth = -100000
asrielAngelBoss.texture = "bosses/angelofdeath/simple00"
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
            musicBurnInDespair = "guid://{970368cc-d45a-4159-a9ec-e56e27d2f7ae}",
            musicHisTheme01 = "event:/new_content/music/pusheen/lvl20/his_theme01",
            musicHisTheme02 = "event:/new_content/music/pusheen/lvl20/his_theme02",
            musicKirbyVsAsriel = "event:/new_content/music/pusheen/lvl20/kirby_vs_asriel_fight_02",
            -- Phase configuration
            barrierWidth = 400,
            barrierHeight = 300,
            riseSpeed = 3.0,
            -- Dialog keys
            dialogPhase1 = "DZ_CH20_ASRIEL_ZERO_RISE_KILL",
            dialogStruggle = "DZ_CH20_ASRIEL_ZERO_STRUGGLE_START",
            dialogVoidAnswer = "DZ_CH20_ASRIEL_ZERO_VOID_ANSWERS",
            dialogFlashback = "DZ_CH20_ASRIEL_REMEMBER_A"
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
            musicBurnInDespair = "guid://{970368cc-d45a-4159-a9ec-e56e27d2f7ae}",
            musicHisTheme01 = "event:/new_content/music/pusheen/lvl20/his_theme01",
            musicHisTheme02 = "event:/new_content/music/pusheen/lvl20/his_theme02",
            musicKirbyVsAsriel = "event:/new_content/music/pusheen/lvl20/kirby_vs_asriel_fight_02",
            barrierWidth = 350,
            barrierHeight = 280,
            riseSpeed = 2.0,
            dialogPhase1 = "DZ_CH20_ASRIEL_ZERO_RISE_KILL",
            dialogStruggle = "DZ_CH20_ASRIEL_ZERO_STRUGGLE_START",
            dialogVoidAnswer = "DZ_CH20_ASRIEL_ZERO_VOID_ANSWERS",
            dialogFlashback = "DZ_CH20_ASRIEL_REMEMBER_A"
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
            musicBurnInDespair = "guid://{970368cc-d45a-4159-a9ec-e56e27d2f7ae}",
            musicHisTheme01 = "event:/new_content/music/pusheen/lvl20/his_theme01",
            musicHisTheme02 = "event:/new_content/music/pusheen/lvl20/his_theme02",
            musicKirbyVsAsriel = "event:/new_content/music/pusheen/lvl20/kirby_vs_asriel_fight_02",
            barrierWidth = 400,
            barrierHeight = 300,
            riseSpeed = 3.0,
            dialogPhase1 = "DZ_CH20_ASRIEL_ZERO_RISE_KILL",
            dialogStruggle = "DZ_CH20_ASRIEL_ZERO_STRUGGLE_START",
            dialogVoidAnswer = "DZ_CH20_ASRIEL_ZERO_VOID_ANSWERS",
            dialogFlashback = "DZ_CH20_ASRIEL_REMEMBER_A"
        }
    }
}

asrielAngelBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10000,
        description = "Boss current health (2500 default)"
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10000,
        description = "Boss maximum health (2500 default)"
    },
    enableBarrier = {
        fieldType = "boolean",
        description = "Enable Undertale-style barrier that traps the player"
    },
    enableLostSouls = {
        fieldType = "boolean",
        description = "Enable lost souls salvation mechanic (saving souls weakens Asriel)"
    },
    enableFlashback = {
        fieldType = "boolean",
        description = "Enable flashback sequence where Asriel remembers his identity"
    },
    enableFinalBeam = {
        fieldType = "boolean",
        description = "Enable final beam attack sequence (Els possessing Asriel)"
    },
    barrierWidth = {
        fieldType = "integer",
        minimumValue = 100,
        maximumValue = 1000,
        description = "Width of the Undertale-style barrier"
    },
    barrierHeight = {
        fieldType = "integer",
        minimumValue = 100,
        maximumValue = 800,
        description = "Height of the Undertale-style barrier"
    },
    riseSpeed = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 10.0,
        description = "Speed at which boss rises from below screen (seconds)"
    },
    musicBurnInDespair = {
        fieldType = "string",
        editable = true,
        description = "FMOD event path for 'Burn in Despair' music phase"
    },
    musicHisTheme01 = {
        fieldType = "string",
        editable = true,
        description = "FMOD event path for hopeful 'His Theme' phase"
    },
    musicHisTheme02 = {
        fieldType = "string",
        editable = true,
        description = "FMOD event path for emotional 'His Theme' climax"
    },
    musicKirbyVsAsriel = {
        fieldType = "string",
        editable = true,
        description = "FMOD event path for intense final battle music"
    },
    dialogPhase1 = {
        fieldType = "string",
        editable = true,
        description = "Dialog key for Phase 1 (boss rise/kill sequence)"
    },
    dialogStruggle = {
        fieldType = "string",
        editable = true,
        description = "Dialog key for struggle phase (player trapped, calling for help)"
    },
    dialogVoidAnswer = {
        fieldType = "string",
        editable = true,
        description = "Dialog key when Astral Birth Void answers the call"
    },
    dialogFlashback = {
        fieldType = "string",
        editable = true,
        description = "Dialog key for flashback trigger (calling 'Azzy')"
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
        local fallback = drawableSprite.fromTexture("characters/asriel/idle00", entity)
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

