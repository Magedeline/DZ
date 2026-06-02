local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local galactaKnightClone = {}

galactaKnightClone.name = "MaggyHelper/GalactaKnightClone"
galactaKnightClone.depth = -8600
galactaKnightClone.justification = {0.5, 1.0}

galactaKnightClone.placements = {
    {
        name = "default",
        data = {
            health = 12,
            moveSpeed = 150.0,
            attackCooldown = 1.15,
            orbitRadius = 74.0,
            dashSpeed = 340.0,
            playMusicOnStart = false,
            bossMusic = "event:/pusheen/music/lvl18/galacta_knight",
            spritePath = "characters/els_true_final_boss/siamo_zero_aeon_hero_fake/",
            idleAnimationPath = "idle",
            moveAnimationPath = "move",
            chargeAnimationPath = "awaken",
            slashAnimationPath = "rapid_slash",
            warpAnimationPath = "fly_start"
        }
    }
}

galactaKnightClone.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    moveSpeed = {
        minimumValue = 1.0
    },
    attackCooldown = {
        minimumValue = 0.1
    },
    orbitRadius = {
        minimumValue = 8.0
    },
    dashSpeed = {
        minimumValue = 10.0
    },
    playMusicOnStart = {
        fieldType = "boolean"
    },
    bossMusic = {
        fieldType = "string",
        editable = true
    },
    spritePath = {
        fieldType = "string",
        editable = true
    },
    idleAnimationPath = {
        fieldType = "string",
        editable = true
    },
    moveAnimationPath = {
        fieldType = "string",
        editable = true
    },
    chargeAnimationPath = {
        fieldType = "string",
        editable = true
    },
    slashAnimationPath = {
        fieldType = "string",
        editable = true
    },
    warpAnimationPath = {
        fieldType = "string",
        editable = true
    }
}

galactaKnightClone.fieldOrder = {
    "x", "y",
    "health",
    "moveSpeed",
    "attackCooldown",
    "orbitRadius",
    "dashSpeed",
    "playMusicOnStart",
    "bossMusic",
    "spritePath",
    "idleAnimationPath",
    "moveAnimationPath",
    "chargeAnimationPath",
    "slashAnimationPath",
    "warpAnimationPath"
}

local fallbackPreviewTextures = {
    "characters/els_true_final_boss/siamo_zero_aeon_hero_fake/idle00",
    "objects/bosses/metaKnightBoss/idle00"
}

local function combineTexturePath(root, animationPath)
    if not root or root == "" or not animationPath or animationPath == "" then
        return nil
    end

    local normalizedRoot = root:gsub("/+$", "")
    local normalizedAnimation = animationPath:gsub("^/+", "")
    return normalizedRoot .. "/" .. normalizedAnimation .. "00"
end

local function getSprite(entity)
    local candidateTextures = {
        combineTexturePath(entity.spritePath, entity.idleAnimationPath)
    }

    for _, texture in ipairs(fallbackPreviewTextures) do
        table.insert(candidateTextures, texture)
    end

    for _, texture in ipairs(candidateTextures) do
        if texture then
            local ok, sprite = pcall(drawableSprite.fromTexture, texture, entity)
            if ok and sprite then
                sprite:setJustification(0.5, 1.0)
                sprite:setColor({1.0, 0.73, 0.92, 1.0})
                return sprite
            end
        end
    end

    return nil
end

function galactaKnightClone.sprite(room, entity)
    local sprite = getSprite(entity)
    return sprite and {sprite} or {}
end

function galactaKnightClone.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 36, 32, 40)
end

return galactaKnightClone
