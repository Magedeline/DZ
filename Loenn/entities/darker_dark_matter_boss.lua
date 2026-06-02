local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local darkerDarkMatterBoss = {}

darkerDarkMatterBoss.name = "MaggyHelper/DarkerDarkMatterBoss"
darkerDarkMatterBoss.depth = -10000
darkerDarkMatterBoss.justification = {0.5, 1.0}

darkerDarkMatterBoss.placements = {
    {
        name = "default",
        data = {
            health = 1600,
            maxHealth = 1600,
            eyeFormSpriteRoot = "characters/darkmatter_boss_runtime",
            eyeDormantAnimationPath = "dormant",
            eyeIdleAnimationPath = "idle",
            eyeAttackAnimationPath = "attack",
            eyeTransformAnimationPath = "transform",
            eyeEnragedAnimationPath = "enraged",
            eyeDefeatAnimationPath = "defeat",
            swordsmanFormSpriteRoot = "characters/darkerdark_swordsman_runtime",
            swordsmanIdleAnimationPath = "idle",
            swordsmanReadyAnimationPath = "ready",
            swordsmanSlashAnimationPath = "slash",
            swordsmanRainbowAnimationPath = "rainbow",
            swordsmanDefeatAnimationPath = "defeat",
            previewTexture = "characters/darkmatter_boss_runtime/idle00"
        }
    }
}

darkerDarkMatterBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 1
    },
    maxHealth = {
        fieldType = "integer",
        minimumValue = 1
    },
    eyeFormSpriteRoot = {
        fieldType = "string",
        editable = true
    },
    eyeDormantAnimationPath = {
        fieldType = "string",
        editable = true
    },
    eyeIdleAnimationPath = {
        fieldType = "string",
        editable = true
    },
    eyeAttackAnimationPath = {
        fieldType = "string",
        editable = true
    },
    eyeTransformAnimationPath = {
        fieldType = "string",
        editable = true
    },
    eyeEnragedAnimationPath = {
        fieldType = "string",
        editable = true
    },
    eyeDefeatAnimationPath = {
        fieldType = "string",
        editable = true
    },
    swordsmanFormSpriteRoot = {
        fieldType = "string",
        editable = true
    },
    swordsmanIdleAnimationPath = {
        fieldType = "string",
        editable = true
    },
    swordsmanReadyAnimationPath = {
        fieldType = "string",
        editable = true
    },
    swordsmanSlashAnimationPath = {
        fieldType = "string",
        editable = true
    },
    swordsmanRainbowAnimationPath = {
        fieldType = "string",
        editable = true
    },
    swordsmanDefeatAnimationPath = {
        fieldType = "string",
        editable = true
    },
    previewTexture = {
        fieldType = "string",
        editable = true
    }
}

darkerDarkMatterBoss.fieldOrder = {
    "x", "y",
    "health",
    "maxHealth",
    "eyeFormSpriteRoot",
    "eyeDormantAnimationPath",
    "eyeIdleAnimationPath",
    "eyeAttackAnimationPath",
    "eyeTransformAnimationPath",
    "eyeEnragedAnimationPath",
    "eyeDefeatAnimationPath",
    "swordsmanFormSpriteRoot",
    "swordsmanIdleAnimationPath",
    "swordsmanReadyAnimationPath",
    "swordsmanSlashAnimationPath",
    "swordsmanRainbowAnimationPath",
    "swordsmanDefeatAnimationPath",
    "previewTexture"
}

local function combineTexturePath(root, animationPath)
    if not root or root == "" or not animationPath or animationPath == "" then
        return nil
    end

    local normalizedRoot = root:gsub("/+$", "")
    local normalizedAnimation = animationPath:gsub("^/+", "")
    return normalizedRoot .. "/" .. normalizedAnimation .. "00"
end

local function getPreviewTextures(entity)
    return {
        entity.previewTexture,
        combineTexturePath(entity.eyeFormSpriteRoot, entity.eyeIdleAnimationPath),
        combineTexturePath(entity.swordsmanFormSpriteRoot, entity.swordsmanIdleAnimationPath),
        "characters/darkmatter_boss_runtime/idle00",
        "characters/darkmatter/idle00"
    }
end

function darkerDarkMatterBoss.sprite(room, entity)
    for _, texture in ipairs(getPreviewTextures(entity)) do
        if texture and texture ~= "" then
            local ok, sprite = pcall(drawableSprite.fromTexture, texture, entity)
            if ok and sprite then
                sprite:setJustification(0.5, 1.0)
                return {sprite}
            end
        end
    end

    return {}
end

function darkerDarkMatterBoss.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 48, 48, 48)
end

return darkerDarkMatterBoss
