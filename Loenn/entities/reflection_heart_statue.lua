local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local reflectionHeartStatue = {}

reflectionHeartStatue.name = "MaggyHelper/ReflectionHeartStatue"
reflectionHeartStatue.depth = 8999
reflectionHeartStatue.nodeLimits = {5, 5}
reflectionHeartStatue.nodeLineRenderType = "line"

reflectionHeartStatue.fieldInformation = {
    code = {
        fieldType = "string",
    },
    flagPrefix = {
        fieldType = "string",
    },
    statueSprite = {
        fieldType = "string",
    },
    torchSprite = {
        fieldType = "string",
    },
    hintSprite = {
        fieldType = "string",
    },
    gemSprite = {
        fieldType = "string",
    },
    heartSprite = {
        fieldType = "string",
    },
    dashSound = {
        fieldType = "string",
    },
    torchSoundPrefix = {
        fieldType = "string",
    },
    heartAppearSound = {
        fieldType = "string",
    },
    }

reflectionHeartStatue.placements = {
    {
        name = "default",
        data = {
            code = "U,L,DR,UR,L,UL",
            flagPrefix = "heartTorch_",
            statueSprite = "objects/reflectionHeart/statue",
            torchSprite = "objects/reflectionHeart/torch",
            hintSprite = "objects/reflectionHeart/hint",
            gemSprite = "objects/reflectionHeart/gem",
            heartSprite = "collectables/heartgem/white00",
            dashSound = "guid://{91dc7f98-2590-4c28-adcb-02c1881cf8d2}",
            torchSoundPrefix = "event:/game/06_reflection/supersecret_torch_",
            heartAppearSound = "guid://{8b071bc7-bb71-4db6-9a0c-78c576426dc8}",
    }
    }
}

function reflectionHeartStatue.sprite(room, entity)
    local texturePath = entity.statueSprite or "objects/reflectionHeart/statue"
    local sprite = drawableSprite.fromTexture(texturePath, entity)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function reflectionHeartStatue.nodeSprite(room, entity, node, nodeIndex)
    if nodeIndex <= 4 then
        -- Torch nodes (0-3)
        local texturePath = entity.torchSprite or "objects/reflectionHeart/torch"
        local sprite = drawableSprite.fromTexture(texturePath .. "00", node)
        sprite:setJustification(0.5, 1.0)
        return sprite
    else
        -- Gem display node (4)
        local texturePath = entity.gemSprite or "objects/reflectionHeart/gem"
        local sprite = drawableSprite.fromTexture(texturePath, node)
        sprite:setJustification(0.5, 0.5)
        return sprite
    end
end

function reflectionHeartStatue.selection(room, entity)
    return utils.rectangle(entity.x - 16, entity.y - 32, 32, 32)
end

function reflectionHeartStatue.nodeSelection(room, entity, node, nodeIndex)
    return utils.rectangle(node.x - 8, node.y - 16, 16, 16)
end

return reflectionHeartStatue

