local drawableSprite = require("structs.drawable_sprite")

local infernoBigEyeball = {}

infernoBigEyeball.name = "MaggyHelper/InfernoBigEyeball"
infernoBigEyeball.depth = -10000

infernoBigEyeball.placements = {
    {
        name = "normal",
        data = {}
    }
}

function infernoBigEyeball.sprite(room, entity)
    local sprites = {}

    -- Use the temple eyeball body sprite
    local bodySprite = drawableSprite.fromTexture("danger/Maggy/DesoloZantas/templeeye/body00", entity)
    table.insert(sprites, bodySprite)

    -- Add the pupil on top
    local pupilSprite = drawableSprite.fromTexture("danger/Maggy/DesoloZantas/templeeye/pupil", entity)
    table.insert(sprites, pupilSprite)

    return sprites
end

function infernoBigEyeball.selection(room, entity)
    local utils = require("utils")
    return utils.rectangle(entity.x - 24, entity.y - 32, 48, 64)
end

return infernoBigEyeball
