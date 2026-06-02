local drawableSprite = require("structs.drawable_sprite")

local infernoEye = {}

infernoEye.name = "MaggyHelper/InfernoEye"
infernoEye.depth = 8990

infernoEye.placements = {
    {
        name = "normal",
        data = {}
    }
}

function infernoEye.sprite(room, entity)
    local sprites = {}

    -- Use the foreground eye texture as default preview
    local eyeSprite = drawableSprite.fromTexture("scenery/temple/eye/fg_eye", entity)
    table.insert(sprites, eyeSprite)

    local pupilSprite = drawableSprite.fromTexture("scenery/temple/eye/fg_pupil", entity)
    table.insert(sprites, pupilSprite)

    return sprites
end

function infernoEye.selection(room, entity)
    local utils = require("utils")
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return infernoEye
