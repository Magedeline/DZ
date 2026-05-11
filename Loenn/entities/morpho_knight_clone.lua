local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local morphoKnightClone = {}

morphoKnightClone.name = "MaggyHelper/MorphoKnightClone"
morphoKnightClone.depth = -8600
morphoKnightClone.justification = {0.5, 1.0}

morphoKnightClone.placements = {
    {
        name = "default",
        data = {
            health = 10,
            moveSpeed = 142.0,
            attackCooldown = 1.05,
            orbitRadius = 66.0,
            dashSpeed = 320.0,
            playMusicOnStart = false,
            bossMusic = "event:/desolo_zantas/music/lvl15/morpho_knight",
            spritePath = "characters/els_true_final_boss/Maggy/DesoloZantas/siamo_zero_morpho_knight_fake/",
            idleAnimationPath = "swords",
            moveAnimationPath = "swords",
            chargeAnimationPath = "vortex_summon",
            slashAnimationPath = "double_side_slash",
            warpAnimationPath = "vortex_strike"
        }
    }
}

morphoKnightClone.fieldInformation = {
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

morphoKnightClone.fieldOrder = {
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
    "characters/els_true_final_boss/Maggy/DesoloZantas/siamo_zero_morpho_knight_fake/swords00",
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
            sprite:setColor({1.0, 0.67, 0.42, 1.0})
            return sprite
        end
        end
    end

    return nil
end

function morphoKnightClone.sprite(room, entity)
    local sprite = getSprite(entity)
    return sprite and {sprite} or {}
end

function morphoKnightClone.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 36, 32, 40)
end

return morphoKnightClone
